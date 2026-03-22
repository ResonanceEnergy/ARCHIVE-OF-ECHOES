using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// C3 — Correspondence Puzzle.
    ///
    /// Two columns of symbols are shown. The player draws a line from each symbol
    /// on the left to its matching counterpart on the right.
    /// A correct pairing is confirmed with glow; all pairs matched = complete.
    ///
    /// Used as the "as above, so below" motif in Issue 03 (city map ↔ star chart),
    /// Issue 05 (glyph ↔ modern text), and Issue 08 (pyramid ↔ Orion belt stars).
    ///
    /// Pair data set in Inspector via CorrespondencePair entries.
    /// </summary>
    public class CorrespondencePuzzle : PuzzleBase
    {
        [System.Serializable]
        public class CorrespondencePair
        {
            public string pairId;
            public RectTransform leftNode;
            public RectTransform rightNode;
            [HideInInspector] public bool matched;
        }

        [Header("Pairs")]
        [SerializeField] private CorrespondencePair[] pairs;

        [Header("Line Drawing")]
        [SerializeField] private RectTransform       lineContainer;
        [SerializeField] private GameObject          linePrefab;     // Image with LineRenderer or UI.Line
        [SerializeField] private Color               correctColor  = new(0.55f, 1f, 0.65f, 1f);
        [SerializeField] private Color               wrongColor    = new(1f, 0.35f, 0.35f, 1f);
        [SerializeField] private Color               activeColor   = new(1f, 0.92f, 0.55f, 1f);
        [SerializeField] private float               nodeRadius    = 28f;
        [SerializeField] private float               wrongFlashSec = 0.4f;

        private CorrespondencePair _activePair;
        private LineRenderer       _activeLine;
        private int                _matchedCount;

        private void OnEnable()
        {
            TouchInputManager.Instance.OnDrag    += HandleDrag;
            TouchInputManager.Instance.OnDragEnd += HandleDragEnd;
        }

        private void OnDisable()
        {
            if (TouchInputManager.Instance == null) return;
            TouchInputManager.Instance.OnDrag    -= HandleDrag;
            TouchInputManager.Instance.OnDragEnd -= HandleDragEnd;
        }

        // ── Input ─────────────────────────────────────────────────────────────────

        private void HandleDrag(Vector2 pos)
        {
            if (_activePair == null)
            {
                // Begin: check left-column tap
                foreach (var pair in pairs)
                {
                    if (pair.matched) continue;
                    if (Vector2.Distance(pos, pair.leftNode.position) <= nodeRadius)
                    {
                        _activePair = pair;
                        _activeLine = SpawnLine(pair.leftNode.position, pos, activeColor);
                        return;
                    }
                }
                return;
            }

            // Update active line endpoint
            if (_activeLine) UpdateLine(_activeLine, _activePair.leftNode.position, pos);
        }

        private void HandleDragEnd(Vector2 _)
        {
            if (_activePair == null) return;

            // Check if released on matching right node
            Vector2 endPos = TouchInputManager.Instance.LastPosition;
            foreach (var pair in pairs)
            {
                if (pair.pairId != _activePair.pairId) continue;
                if (Vector2.Distance(endPos, pair.rightNode.position) <= nodeRadius)
                {
                    ConfirmMatch(_activePair, _activeLine);
                    _activePair = null;
                    _activeLine = null;
                    return;
                }
            }

            // Wrong — flash and destroy
            StartCoroutine(FlashWrong(_activeLine));
            _activePair = null;
            _activeLine = null;
        }

        // ── Match / wrong ─────────────────────────────────────────────────────────

        private void ConfirmMatch(CorrespondencePair pair, LineRenderer line)
        {
            pair.matched = true;
            _matchedCount++;
            if (line)
            {
                SetLineColor(line, correctColor);
                UpdateLine(line, pair.leftNode.position, pair.rightNode.position);
            }

            AudioManager.Instance?.PlayMotif(MotifType.PanelRestored);
            Haptic.Play(HapticFeedback.ImpactLight);

            if (_matchedCount >= pairs.Length) Complete();
        }

        private System.Collections.IEnumerator FlashWrong(LineRenderer line)
        {
            if (line) SetLineColor(line, wrongColor);
            yield return new WaitForSeconds(wrongFlashSec);
            if (line) Destroy(line.gameObject);
        }

        // ── Line helpers ──────────────────────────────────────────────────────────

        private LineRenderer SpawnLine(Vector2 from, Vector2 to, Color color)
        {
            if (linePrefab == null || lineContainer == null) return null;
            var go = Instantiate(linePrefab, lineContainer);
            var lr = go.GetComponent<LineRenderer>() ?? go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, from);
            lr.SetPosition(1, to);
            SetLineColor(lr, color);
            return lr;
        }

        private static void UpdateLine(LineRenderer lr, Vector2 from, Vector2 to)
        {
            lr.SetPosition(0, from);
            lr.SetPosition(1, to);
        }

        private static void SetLineColor(LineRenderer lr, Color c)
        {
            lr.startColor = c;
            lr.endColor   = c;
        }
    }
}
