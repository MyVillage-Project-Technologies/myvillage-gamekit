using System.Collections.Generic;
using MyVillage.GameKit.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MyVillage.GameKit.Templates
{
    /// Pair-matching memory mission. Flip a card, flip a second card — if the
    /// faces match, the pair stays revealed and the player scores. Mismatched
    /// pairs flip back after a brief reveal. Win condition: all pairs matched.
    ///
    /// Builds its own grid UI programmatically. CardFaces in the config become
    /// the matchable sprites; the grid auto-sizes to GridSize.
    public sealed class MatchMission : MissionBase
    {
        [SerializeField] private MatchMissionConfig configOverride;

        MatchMissionConfig _config;
        Canvas _canvas;
        MissionHUD _hud;
        ResultPanel _resultPanel;

        readonly List<CardView> _cards = new();
        CardView _firstFlipped;
        bool _processing;
        int _score;
        int _matches;
        int _mismatches;

        protected override void OnInitialize()
        {
            _config = (Config as MatchMissionConfig) ?? configOverride;
            if (_config == null)
            {
                Host.LogError("MatchMission requires a MatchMissionConfig.");
                return;
            }
            if (_config.CardFaces == null || _config.CardFaces.Length == 0)
            {
                Host.LogError("MatchMissionConfig.CardFaces is empty.");
                return;
            }
            int total = _config.GridSize.x * _config.GridSize.y;
            if (total <= 0 || total % 2 != 0)
            {
                Host.LogError($"MatchMission grid must be positive and even (got {_config.GridSize.x}x{_config.GridSize.y}).");
                return;
            }
            _hud = MissionHUD.Create(Host);
            _hud.Bind(Host);
            _hud.SetTitle(string.IsNullOrEmpty(_config.DisplayName) ? "Match" : _config.DisplayName);
            _hud.SetScore(0);
            _resultPanel = ResultPanel.Create(Host);
            BuildGrid();
        }

        protected override void OnBegin()
        {
            if (_config == null) return;
            Host.LogEvent("match.begin", null);
        }

        protected override void OnEnd()
        {
            if (_canvas != null) _canvas.gameObject.SetActive(false);
        }

        void Update()
        {
            if (_config == null) return;
            _hud.SetTimer(TimeSinceBegin);
        }

        void OnCardClicked(CardView card)
        {
            if (_processing || card.IsRevealed || card.IsMatched) return;
            card.Reveal();

            if (_firstFlipped == null)
            {
                _firstFlipped = card;
                return;
            }

            _processing = true;
            if (_firstFlipped.PairId == card.PairId)
            {
                _firstFlipped.MarkMatched();
                card.MarkMatched();
                _matches++;
                _score += _config.PointsPerMatch;
                _firstFlipped = null;
                _processing = false;
                _hud.SetScore(_score);
                ReportProgress(_score);
                Host.LogEvent("match.pair", new Dictionary<string, object> {
                    { "score", _score },
                    { "matches", _matches },
                });
                if (_matches == _cards.Count / 2) Finish();
            }
            else
            {
                _mismatches++;
                _score = Mathf.Max(0, _score - _config.MismatchPenalty);
                _hud.SetScore(_score);
                ReportProgress(_score);
                // Brief reveal then flip both back
                Invoke(nameof(HideMismatchedPair), 0.8f);
            }
        }

        void HideMismatchedPair()
        {
            if (_firstFlipped != null) _firstFlipped.Hide();
            // Find the second flipped (most recently revealed non-matched card)
            foreach (var c in _cards)
                if (c.IsRevealed && !c.IsMatched) c.Hide();
            _firstFlipped = null;
            _processing = false;
        }

        void Finish()
        {
            CompleteMission(finalScore: _score, correctAnswers: _matches, incorrectAnswers: _mismatches);
            _resultPanel.Show(
                new MissionResult {
                    FinalScore = _score,
                    CorrectAnswers = _matches,
                    IncorrectAnswers = _mismatches,
                    DurationSeconds = Mathf.RoundToInt(TimeSinceBegin),
                },
                onContinue: null);
        }

        // ── UI ──

        void BuildGrid()
        {
            var go = new GameObject("MatchCanvas");
            go.transform.SetParent(transform, false);
            _canvas = go.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 10;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();

            var bg = new GameObject("Background");
            bg.transform.SetParent(_canvas.transform, false);
            var bgRt = bg.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.18f, 0.31f, 0.09f, 1f);

            int cols = _config.GridSize.x;
            int rows = _config.GridSize.y;
            int totalCards = cols * rows;
            int neededFaces = totalCards / 2;

            // Pair list: each face index appears twice, then shuffled
            var pairIds = new List<int>(totalCards);
            for (int i = 0; i < neededFaces; i++)
            {
                int faceIdx = i % _config.CardFaces.Length;
                pairIds.Add(faceIdx);
                pairIds.Add(faceIdx);
            }
            for (int i = pairIds.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (pairIds[i], pairIds[j]) = (pairIds[j], pairIds[i]);
            }

            var grid = new GameObject("Grid");
            grid.transform.SetParent(_canvas.transform, false);
            var gridRt = grid.AddComponent<RectTransform>();
            gridRt.anchorMin = new Vector2(0.5f, 0.5f);
            gridRt.anchorMax = new Vector2(0.5f, 0.5f);
            gridRt.pivot = new Vector2(0.5f, 0.5f);
            gridRt.anchoredPosition = new Vector2(0, -40);
            // Make grid take ~80% of width and proportional height
            float cellW = 880f / cols;
            float cellH = 880f / cols; // square cells based on column count
            gridRt.sizeDelta = new Vector2(cellW * cols, cellH * rows);

            int k = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var card = CreateCard(grid.transform, c, r, cols, rows, cellW, cellH, pairIds[k]);
                    _cards.Add(card);
                    k++;
                }
            }
        }

        CardView CreateCard(Transform parent, int col, int row, int cols, int rows, float cellW, float cellH, int pairId)
        {
            var go = new GameObject($"Card_{col}_{row}");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(cellW - 12, cellH - 12);
            rt.anchoredPosition = new Vector2(col * cellW + cellW / 2, -(row * cellH + cellH / 2));

            var img = go.AddComponent<Image>();
            img.color = new Color(0.69f, 0.49f, 0, 1); // gold (face down)
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            // Face sprite — added but disabled until revealed
            var face = new GameObject("Face");
            face.transform.SetParent(go.transform, false);
            var faceRt = face.AddComponent<RectTransform>();
            faceRt.anchorMin = Vector2.zero;
            faceRt.anchorMax = Vector2.one;
            faceRt.offsetMin = new Vector2(8, 8);
            faceRt.offsetMax = new Vector2(-8, -8);
            var faceImg = face.AddComponent<Image>();
            faceImg.sprite = _config.CardFaces[pairId % _config.CardFaces.Length];
            faceImg.preserveAspect = true;
            face.SetActive(false);

            var card = new CardView {
                PairId = pairId,
                Back = img,
                FaceGo = face,
                Button = btn,
            };
            btn.onClick.AddListener(() => OnCardClicked(card));
            return card;
        }

        sealed class CardView
        {
            public int PairId;
            public Image Back;
            public GameObject FaceGo;
            public Button Button;
            public bool IsRevealed;
            public bool IsMatched;

            public void Reveal()
            {
                IsRevealed = true;
                FaceGo.SetActive(true);
                Back.color = new Color(1, 1, 1, 1);
            }

            public void Hide()
            {
                IsRevealed = false;
                FaceGo.SetActive(false);
                Back.color = new Color(0.69f, 0.49f, 0, 1);
            }

            public void MarkMatched()
            {
                IsMatched = true;
                Button.interactable = false;
                Back.color = new Color(0.27f, 0.7f, 0.27f, 1f); // green tint
            }
        }
    }
}
