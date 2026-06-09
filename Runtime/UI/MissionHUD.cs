using System;
using UnityEngine;
using UnityEngine.UI;

namespace MyVillage.GameKit.UI
{
    /// Lightweight programmatic HUD for missions: top-left score, top-right
    /// timer, top-center pause button. No prefab required — call Show() and
    /// the HUD builds its own Canvas + children.
    ///
    /// Missions either:
    ///   1. Drop a pre-built Canvas with MissionHUD attached into the scene
    ///      and call SetScore / SetTimer / SetTitle directly, or
    ///   2. Skip the prefab and call MissionHUD.Create(host) at runtime —
    ///      this returns a fully-built instance ready to display.
    public sealed class MissionHUD : MonoBehaviour
    {
        Text _scoreText;
        Text _timerText;
        Text _titleText;
        Button _pauseButton;
        Text _pauseButtonText;
        IMissionHost _host;
        bool _paused;

        public void SetTitle(string title)
        {
            EnsureBuilt();
            _titleText.text = title ?? string.Empty;
        }

        public void SetScore(int score)
        {
            EnsureBuilt();
            _scoreText.text = $"Score: {score}";
        }

        public void SetTimer(float seconds)
        {
            EnsureBuilt();
            int s = Mathf.Max(0, Mathf.RoundToInt(seconds));
            _timerText.text = $"{s / 60:00}:{s % 60:00}";
        }

        public void Bind(IMissionHost host)
        {
            EnsureBuilt();
            _host = host;
        }

        /// Build a MissionHUD on a fresh GameObject + Canvas. Use when the
        /// mission scene doesn't ship its own HUD.
        public static MissionHUD Create(IMissionHost host)
        {
            var go = new GameObject("MissionHUD (auto)");
            var hud = go.AddComponent<MissionHUD>();
            hud.Bind(host);
            return hud;
        }

        void Awake()
        {
            EnsureBuilt();
        }

        void EnsureBuilt()
        {
            if (_scoreText != null) return; // already built

            // Canvas
            var canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            if (GetComponent<CanvasScaler>() == null)
            {
                var scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }
            if (GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            _scoreText = CreateText("Score", "Score: 0", new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), TextAnchor.UpperLeft);
            _timerText = CreateText("Timer", "00:00", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20), TextAnchor.UpperRight);
            _titleText = CreateText("Title", string.Empty, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -20), TextAnchor.UpperCenter);
            _pauseButton = CreatePauseButton();
        }

        Text CreateText(string name, string content, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, TextAnchor align)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = anchorMin;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(360, 60);
            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 36;
            text.color = Color.white;
            text.alignment = align;
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.7f);
            shadow.effectDistance = new Vector2(2, -2);
            return text;
        }

        Button CreatePauseButton()
        {
            var go = new GameObject("Pause");
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-20, -90);
            rt.sizeDelta = new Vector2(120, 60);
            var img = go.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.5f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = labelGo.AddComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            _pauseButtonText = labelGo.AddComponent<Text>();
            _pauseButtonText.text = "Pause";
            _pauseButtonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _pauseButtonText.fontSize = 28;
            _pauseButtonText.color = Color.white;
            _pauseButtonText.alignment = TextAnchor.MiddleCenter;

            btn.onClick.AddListener(TogglePause);
            return btn;
        }

        void TogglePause()
        {
            if (_host == null) return;
            _paused = !_paused;
            _pauseButtonText.text = _paused ? "Resume" : "Pause";
            if (_paused) _host.RequestPause();
            else _host.RequestResume();
        }
    }
}
