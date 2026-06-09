using System;
using System.Collections.Generic;
using MyVillage.GameKit.UI;
using UnityEngine;

namespace MyVillage.GameKit.MockHost
{
    /// In-editor stand-in for the M-UNI Universe host. Drop on a GameObject in
    /// your mission scene, hit Play, and your game runs end-to-end without a
    /// real bundle deploy — Complete Mission triggers the real ResultPanel,
    /// Report Progress updates the HUD, Log Event prints to the console.
    ///
    /// Auto-discovery rules:
    ///   - On Awake, this finds the first MissionBase in the scene and binds
    ///     itself as that mission's host (sets IMissionHost on the mission's
    ///     Object-scope Variables if a ScriptMachine is present, so Visual
    ///     Scripting Custom Units can resolve it too).
    ///   - When the mission is initialized, this calls Begin shortly after
    ///     (with a brief "Tap to begin" overlay if showTapToBegin is true,
    ///     or auto if false).
    ///
    /// This component is intended for editor preview only. It is harmless to
    /// leave on a GameObject in a shipped bundle — the real host's
    /// MissionHostAdapter overwrites the missionHost variable so the mock
    /// never runs in production. Preflight allows it because its assembly
    /// (MyVillage.GameKit) is allowlisted.
    [AddComponentMenu("MyVillage/Mock Mission Host")]
    [DisallowMultipleComponent]
    public sealed class MockMissionHost : MonoBehaviour, IMissionHost
    {
        [Header("Mission")]
        [Tooltip("MissionConfig asset injected as IMissionHost.Config. Optional — leave null if your mission doesn't read config.")]
        [SerializeField] private MissionConfig config;

        [Tooltip("Game slug returned by IMissionHost.GameSlug. Pretend value; the real host uses the catalog slug.")]
        [SerializeField] private string gameSlug = "mock-game";

        [Tooltip("Mission ID returned by IMissionHost.MissionId. Optional.")]
        [SerializeField] private string missionId = "";

        [Header("Behavior")]
        [Tooltip("If true, the mock host pauses for user input before calling Begin. If false, calls Begin immediately after Initialize.")]
        [SerializeField] private bool autoBegin = true;

        [Tooltip("If true, shows a small in-editor overlay with the latest host events. Useful while iterating; turn off for clean playtest.")]
        [SerializeField] private bool showDebugOverlay = true;

        public string GameSlug => gameSlug;
        public string SessionId { get; private set; } = "mock-session-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        public string MissionId => missionId;
        public MissionConfig Config => config;

        public event Action<bool> OnPauseChanged;

        readonly List<string> _eventLog = new();
        MissionBase _mission;
        ResultPanel _resultPanel;
        const int LOG_LINES = 8;

        void Awake()
        {
            // Find the mission to host. We deliberately look across the whole
            // scene so the MockMissionHost can live on a separate GameObject
            // from MissionBase if the dev wants.
            _mission = FindFirstObjectByType<MissionBase>(FindObjectsInactive.Exclude);
            if (_mission == null)
            {
                Debug.LogWarning("[MockMissionHost] No MissionBase found in scene. Add one to use this mock host.");
                enabled = false;
                return;
            }

            // Bind for Visual Scripting Custom Units if there's a ScriptMachine.
            InjectAsVisualScriptingVariable();
        }

        void Start()
        {
            if (_mission == null) return;
            _resultPanel = ResultPanel.Create(this);

            try
            {
                _mission.Initialize(this);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MockMissionHost] mission.Initialize threw: {e}");
                return;
            }

            if (autoBegin)
            {
                _mission.Begin();
            }
            // else: dev's gameplay code calls Begin on its own trigger
        }

        // ── IMissionHost ──

        public void ReportProgress(MissionProgress progress)
        {
            Log($"progress: score={progress.Score} lives={progress.Lives} level={progress.Level}");
        }

        public void CompleteMission(MissionResult result)
        {
            Log($"COMPLETE score={result.FinalScore} correct={result.CorrectAnswers} incorrect={result.IncorrectAnswers} duration={result.DurationSeconds}s");
            _resultPanel?.Show(result, onContinue: null);
        }

        public void FailMission(string reason)
        {
            Log($"FAIL reason='{reason}'");
            _resultPanel?.Show(
                new MissionResult { FinalScore = 0, Difficulty = "Failed: " + reason },
                onContinue: null);
        }

        public void AbandonMission()
        {
            Log("ABANDON");
        }

        public void RequestPause()
        {
            Log("pause requested");
            OnPauseChanged?.Invoke(true);
        }

        public void RequestResume()
        {
            Log("resume requested");
            OnPauseChanged?.Invoke(false);
        }

        public void LogEvent(string eventName, IDictionary<string, object> properties = null)
        {
            if (properties != null && properties.Count > 0)
            {
                var pairs = new List<string>();
                foreach (var kv in properties) pairs.Add($"{kv.Key}={kv.Value}");
                Log($"event {eventName} {string.Join(" ", pairs)}");
            }
            else
            {
                Log($"event {eventName}");
            }
        }

        public void LogError(string message, Exception exception = null)
        {
            if (exception != null)
                Debug.LogError($"[MockHost-game] {message}\n{exception}");
            else
                Debug.LogError($"[MockHost-game] {message}");
        }

        // ── Visual Scripting variable injection ──

        void InjectAsVisualScriptingVariable()
        {
            // Reflection-based so the SDK doesn't acquire a hard reference on
            // Unity.VisualScripting; the mock host is useful even in projects
            // that don't have Visual Scripting installed.
            try
            {
                var scriptMachineType = Type.GetType("Unity.VisualScripting.ScriptMachine, Unity.VisualScripting.Flow");
                if (scriptMachineType == null) return;
                if (_mission.gameObject.GetComponent(scriptMachineType) == null) return;

                var variablesType = Type.GetType("Unity.VisualScripting.Variables, Unity.VisualScripting.Core");
                if (variablesType == null) return;

                var objectMethod = variablesType.GetMethod("Object", new[] { typeof(GameObject) });
                var decls = objectMethod?.Invoke(null, new object[] { _mission.gameObject });
                if (decls == null) return;

                var setMethod = decls.GetType().GetMethod("Set", new[] { typeof(string), typeof(object) });
                setMethod?.Invoke(decls, new object[] { "missionHost", this });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MockMissionHost] Visual Scripting injection failed: {e.Message}");
            }
        }

        // ── Debug overlay ──

        void Log(string line)
        {
            Debug.Log($"[MockHost] {line}");
            _eventLog.Add(line);
            while (_eventLog.Count > LOG_LINES) _eventLog.RemoveAt(0);
        }

        void OnGUI()
        {
            if (!showDebugOverlay || _eventLog.Count == 0) return;
            const int width = 360;
            const int lineH = 18;
            int x = 10;
            int y = Screen.height - (LOG_LINES * lineH) - 30;
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = Color.white },
            };
            GUI.Box(new Rect(x - 4, y - 4, width + 8, _eventLog.Count * lineH + 8), GUIContent.none);
            GUI.Label(new Rect(x, y - lineH, width, lineH), "<MockHost>", style);
            for (int i = 0; i < _eventLog.Count; i++)
            {
                GUI.Label(new Rect(x, y + i * lineH, width, lineH), _eventLog[i], style);
            }
        }
    }
}
