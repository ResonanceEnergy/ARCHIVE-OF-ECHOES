using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// D2 — Council Decree Puzzle.
    ///
    /// A clay-tablet excerpt shows crossed-out Annunaki council clauses. Three decree fragments
    /// are presented; the player drags and places each fragment over the appropriate gap
    /// in the tablet to reconstitute the suppressed decree.
    ///
    /// Fragment placement check: semantic matching via a string ID (not positional).
    /// Two fragments may plausibly fit a gap — the puzzle intentionally allows either
    /// and the chosen interpretation is written to ArchiveState for downstream narrative.
    ///
    /// Issue 02 (intro): 3 fragments, 1 ambiguous gap.
    /// Issue 07 (final council): 5 fragments, 2 ambiguous gaps (choice shapes Issue 08 panel caption).
    /// </summary>
    public class CouncilDecreePuzzle : PuzzleBase
    {
        [System.Serializable]
        public class DecreeGap
        {
            public string   gapId;
            public string[] acceptedFragmentIds;   // all valid answers (ambiguous = 2+ entries)
            public RectTransform gapTransform;
            public Image    gapHighlight;
            [HideInInspector] public string placedId;
            [HideInInspector] public bool   filled;
        }

        [System.Serializable]
        public class DecreeFragment
        {
            public string        fragmentId;
            public string        labelText;
            public RectTransform fragmentTransform;
        }

        [Header("Tablet")]
        [SerializeField] private DecreeGap[]      gaps;
        [SerializeField] private DecreeFragment[] fragments;
        [SerializeField] private float            snapRadius = 40f;

        [Header("Feedback")]
        [SerializeField] private Color correctGlow  = new(0.93f, 0.84f, 0.55f, 1f);
        [SerializeField] private Color wrongFlash   = new(1f, 0.3f, 0.3f, 1f);
        [SerializeField] private float wrongFlashSec = 0.3f;

        // Drag state
        private DecreeFragment _dragging;
        private Vector2        _dragOrigin;
        private int            _filledCount;

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
            if (_dragging == null)
            {
                foreach (var frag in fragments)
                {
                    if (!frag.fragmentTransform.gameObject.activeSelf) continue;
                    if (Vector2.Distance(pos, frag.fragmentTransform.position) <= 40f)
                    {
                        _dragging   = frag;
                        _dragOrigin = frag.fragmentTransform.position;
                        return;
                    }
                }
                return;
            }

            _dragging.fragmentTransform.position = pos;
        }

        private void HandleDragEnd(Vector2 _)
        {
            if (_dragging == null) return;

            Vector2 dropPos = TouchInputManager.Instance.LastPosition;
            DecreeGap target = FindNearestGap(dropPos);

            if (target != null && !target.filled)
            {
                if (IsAccepted(target, _dragging.fragmentId))
                {
                    PlaceFragment(target, _dragging);
                }
                else
                {
                    StartCoroutine(FlashWrong(target));
                    _dragging.fragmentTransform.position = _dragOrigin;
                }
            }
            else
            {
                _dragging.fragmentTransform.position = _dragOrigin;
            }

            _dragging = null;
        }

        // ── Placement ────────────────────────────────────────────────────────────

        private void PlaceFragment(DecreeGap gap, DecreeFragment frag)
        {
            gap.filled   = true;
            gap.placedId = frag.fragmentId;

            // Snap fragment to gap
            frag.fragmentTransform.position = gap.gapTransform.position;
            frag.fragmentTransform.SetParent(gap.gapTransform, true);

            if (gap.gapHighlight) gap.gapHighlight.color = correctGlow;

            // Record choice for narrative ambiguity
            var state = GameManager.Instance?.State;
            state?.RecordDecreeChoice(gap.gapId, frag.fragmentId);

            AudioManager.Instance?.PlayMotif(MotifType.PanelRestored);
            Haptic.Play(HapticFeedback.ImpactLight);

            _filledCount++;
            if (_filledCount >= gaps.Length) Complete();
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private bool IsAccepted(DecreeGap gap, string fragId)
        {
            foreach (string id in gap.acceptedFragmentIds)
                if (id == fragId) return true;
            return false;
        }

        private DecreeGap FindNearestGap(Vector2 pos)
        {
            DecreeGap best = null;
            float bestDist = snapRadius;
            foreach (var gap in gaps)
            {
                if (gap.filled) continue;
                float d = Vector2.Distance(pos, gap.gapTransform.position);
                if (d < bestDist) { bestDist = d; best = gap; }
            }
            return best;
        }

        private IEnumerator FlashWrong(DecreeGap gap)
        {
            if (gap.gapHighlight)
            {
                gap.gapHighlight.color = wrongFlash;
                yield return new WaitForSeconds(wrongFlashSec);
                gap.gapHighlight.color = Color.white;
            }
        }
    }
}
