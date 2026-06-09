using System;
using UnityEngine;
using UnityEngine.UI;

namespace MyVillage.GameKit.UI
{
    /// End-of-mission summary panel: final score, correct/incorrect tally,
    /// time taken, continue button. Programmatic — no prefab required.
    ///
    /// Usage:
    ///   var panel = ResultPanel.Create(host);
    ///   panel.Show(missionResult, onContinue: () => sceneController.GoHome());
    public sealed class ResultPanel : MonoBehaviour
    {
        Text _scoreText;
        Text _detailText;
        Text _titleText;
        Button _continueButton;
        Action _onContinue;
        bool _built;

        public static ResultPanel Create(IMissionHost host)
        {
            var go = new GameObject("ResultPanel (auto)");
            var panel = go.AddComponent<ResultPanel>();
            panel.gameObject.SetActive(false);
            return panel;
        }

        public void Show(MissionResult result, Action onContinue = null)
        {
            EnsureBuilt();
            gameObject.SetActive(true);
            _onContinue = onContinue;

            _titleText.text = "Mission Complete";
            _scoreText.text = $"Score: {result.FinalScore}";

            var details = new System.Text.StringBuilder();
            if (result.CorrectAnswers > 0 || result.IncorrectAnswers > 0)
                details.AppendLine($"Correct: {result.CorrectAnswers}   Incorrect: {result.IncorrectAnswers}");
            int s = Mathf.Max(0, result.DurationSeconds);
            details.AppendLine($"Time: {s / 60:00}:{s % 60:00}");
            if (!string.IsNullOrEmpty(result.Difficulty))
                details.AppendLine($"Difficulty: {result.Difficulty}");
            _detailText.text = details.ToString().TrimEnd();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        void EnsureBuilt()
        {
            if (_built) return;
            _built = true;

            var canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200; // above MissionHUD
            if (GetComponent<CanvasScaler>() == null)
            {
                var scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }
            if (GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            // Dimmed background
            var bg = new GameObject("Background");
            bg.transform.SetParent(transform, false);
            var bgRt = bg.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.7f);

            _titleText = CreateText("Title", string.Empty, new Vector2(0, 1), new Vector2(0, 0), new Vector2(540, 100), 56, TextAnchor.MiddleCenter);
            _scoreText = CreateText("Score", string.Empty, new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(540, 80), 48, TextAnchor.MiddleCenter);
            _detailText = CreateText("Details", string.Empty, new Vector2(0, 0.5f), new Vector2(0, -100), new Vector2(720, 200), 32, TextAnchor.UpperCenter);
            _continueButton = CreateContinueButton();
        }

        Text CreateText(string name, string content, Vector2 anchorMidpoint, Vector2 offset, Vector2 size, int fontSize, TextAnchor align)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMidpoint;
            rt.anchorMax = anchorMidpoint;
            rt.pivot = anchorMidpoint;
            // anchorMidpoint passed as (0, y) so we want screen-center horizontally
            // by anchoring to (0.5, y) instead
            rt.anchorMin = new Vector2(0.5f, anchorMidpoint.y);
            rt.anchorMax = new Vector2(0.5f, anchorMidpoint.y);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, offset.y);
            rt.sizeDelta = size;
            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = align;
            return text;
        }

        Button CreateContinueButton()
        {
            var go = new GameObject("Continue");
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.2f);
            rt.anchorMax = new Vector2(0.5f, 0.2f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(360, 96);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.69f, 0.49f, 0, 1); // MyVillage gold (#B07C00)
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var label = new GameObject("Label");
            label.transform.SetParent(go.transform, false);
            var labelRt = label.AddComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            var labelText = label.AddComponent<Text>();
            labelText.text = "Continue";
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 36;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleCenter;

            btn.onClick.AddListener(() => { _onContinue?.Invoke(); Hide(); });
            return btn;
        }
    }
}
