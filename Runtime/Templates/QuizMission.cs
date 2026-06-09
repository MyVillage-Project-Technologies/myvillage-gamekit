using System.Collections.Generic;
using MyVillage.GameKit.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MyVillage.GameKit.Templates
{
    /// Data-driven quiz mission. Drop on a GameObject, reference a
    /// QuizMissionConfig asset, and ship. The mission builds its own UI
    /// programmatically — no prefab work required.
    ///
    /// To customize the look: attach this to a GameObject that already has
    /// a Canvas + child Text/Button references and call SetUIReferences()
    /// in OnInitialize before Begin runs.
    public sealed class QuizMission : MissionBase
    {
        [SerializeField] private QuizMissionConfig configOverride;

        QuizMissionConfig _config;
        Canvas _canvas;
        Text _promptText;
        Text _progressText;
        readonly List<Button> _answerButtons = new();
        readonly List<Text> _answerLabels = new();
        MissionHUD _hud;
        ResultPanel _resultPanel;

        int _questionIndex;
        int _score;
        int _correct;
        int _incorrect;
        float _questionStartTime;
        List<int> _questionOrder;
        bool _awaitingAnswer;

        protected override void OnInitialize()
        {
            _config = (Config as QuizMissionConfig) ?? configOverride;
            if (_config == null)
            {
                Host.LogError("QuizMission requires a QuizMissionConfig (in Host.Config or the configOverride field).");
                return;
            }
            if (_config.Questions == null || _config.Questions.Length == 0)
            {
                Host.LogError("QuizMissionConfig has no questions.");
                return;
            }
            BuildUI();
            _hud = MissionHUD.Create(Host);
            _hud.Bind(Host);
            _hud.SetTitle(string.IsNullOrEmpty(_config.DisplayName) ? "Quiz" : _config.DisplayName);
            _hud.SetScore(0);
            _resultPanel = ResultPanel.Create(Host);
        }

        protected override void OnBegin()
        {
            if (_config == null) return;
            _questionOrder = BuildQuestionOrder(_config.Questions.Length, _config.ShuffleQuestions);
            _questionIndex = 0;
            _score = 0;
            _correct = 0;
            _incorrect = 0;
            ShowNextQuestion();
            Host.LogEvent("quiz.begin", null);
        }

        protected override void OnEnd()
        {
            if (_canvas != null) _canvas.gameObject.SetActive(false);
        }

        void Update()
        {
            if (_config == null || !_awaitingAnswer) return;
            if (_config.SecondsPerQuestion <= 0) return;
            var remaining = Mathf.Max(0f, _config.SecondsPerQuestion - (Time.time - _questionStartTime));
            _hud.SetTimer(remaining);
            if (remaining <= 0f) OnAnswerSelected(-1); // timeout = incorrect
        }

        void ShowNextQuestion()
        {
            if (_questionIndex >= _questionOrder.Count)
            {
                Finish();
                return;
            }
            var q = _config.Questions[_questionOrder[_questionIndex]];
            _promptText.text = q.Prompt ?? string.Empty;
            _progressText.text = $"Q {_questionIndex + 1} / {_questionOrder.Count}";

            for (int i = 0; i < _answerButtons.Count; i++)
            {
                bool active = q.Choices != null && i < q.Choices.Length;
                _answerButtons[i].gameObject.SetActive(active);
                if (active)
                {
                    _answerLabels[i].text = q.Choices[i];
                    int captured = i;
                    _answerButtons[i].onClick.RemoveAllListeners();
                    _answerButtons[i].onClick.AddListener(() => OnAnswerSelected(captured));
                    _answerButtons[i].interactable = true;
                }
            }

            _questionStartTime = Time.time;
            _awaitingAnswer = true;
        }

        void OnAnswerSelected(int choiceIndex)
        {
            if (!_awaitingAnswer) return;
            _awaitingAnswer = false;

            var q = _config.Questions[_questionOrder[_questionIndex]];
            bool isCorrect = choiceIndex == q.CorrectChoiceIndex;
            if (isCorrect)
            {
                _score += q.Points;
                _correct++;
            }
            else
            {
                _incorrect++;
            }
            _hud.SetScore(_score);
            ReportProgress(_score);

            Host.LogEvent("quiz.answer", new Dictionary<string, object> {
                { "questionIndex", _questionIndex },
                { "choiceIndex", choiceIndex },
                { "correct", isCorrect },
                { "score", _score },
            });

            _questionIndex++;
            // Brief flash of the chosen answer before advancing
            for (int i = 0; i < _answerButtons.Count; i++)
                _answerButtons[i].interactable = false;
            Invoke(nameof(ShowNextQuestion), 0.4f);
        }

        void Finish()
        {
            CompleteMission(finalScore: _score, correctAnswers: _correct, incorrectAnswers: _incorrect);
            _resultPanel.Show(
                new MissionResult {
                    FinalScore = _score,
                    CorrectAnswers = _correct,
                    IncorrectAnswers = _incorrect,
                    DurationSeconds = Mathf.RoundToInt(TimeSinceBegin),
                },
                onContinue: null);
        }

        static List<int> BuildQuestionOrder(int count, bool shuffle)
        {
            var list = new List<int>(count);
            for (int i = 0; i < count; i++) list.Add(i);
            if (!shuffle) return list;
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
            return list;
        }

        // ── UI construction ──

        void BuildUI()
        {
            var go = new GameObject("QuizCanvas");
            go.transform.SetParent(transform, false);
            _canvas = go.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 10;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();

            // Background panel
            var bg = new GameObject("Background");
            bg.transform.SetParent(_canvas.transform, false);
            var bgRt = bg.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.02f, 0.22f, 0.13f, 1f); // MyVillage deep green

            _progressText = CreateText("Progress", string.Empty, new Vector2(0.5f, 0.85f), new Vector2(800, 60), 32);
            _promptText = CreateText("Prompt", string.Empty, new Vector2(0.5f, 0.65f), new Vector2(900, 240), 44);
            _promptText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _promptText.verticalOverflow = VerticalWrapMode.Truncate;

            // Up to 6 answer buttons in a vertical stack
            const int maxAnswers = 6;
            for (int i = 0; i < maxAnswers; i++)
            {
                var (btn, label) = CreateAnswerButton(i, maxAnswers);
                _answerButtons.Add(btn);
                _answerLabels.Add(label);
            }
        }

        Text CreateText(string name, string content, Vector2 anchorCenter, Vector2 size, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_canvas.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorCenter;
            rt.anchorMax = anchorCenter;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;
            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            return text;
        }

        (Button, Text) CreateAnswerButton(int index, int total)
        {
            var go = new GameObject($"Answer{index}");
            go.transform.SetParent(_canvas.transform, false);
            var rt = go.AddComponent<RectTransform>();
            // Stack vertically from 0.45 down to 0.05
            float y = Mathf.Lerp(0.42f, 0.06f, total <= 1 ? 0 : (float)index / (total - 1));
            rt.anchorMin = new Vector2(0.5f, y);
            rt.anchorMax = new Vector2(0.5f, y);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(800, 88);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.69f, 0.49f, 0, 0.95f); // MyVillage gold #B07C00
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var label = new GameObject("Label");
            label.transform.SetParent(go.transform, false);
            var labelRt = label.AddComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = new Vector2(20, 0);
            labelRt.offsetMax = new Vector2(-20, 0);
            var text = label.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 32;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            go.SetActive(false);
            return (btn, text);
        }
    }
}
