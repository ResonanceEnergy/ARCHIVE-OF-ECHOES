using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// G1 — Gutter Door Puzzle.
    ///
    /// A hidden word is embedded in the gutter (white space between panels).
    /// The player must trace the word by dragging their finger through the gutter area.
    /// Gutter Door panels are entirely optional and grant lore rewards only.
    ///
    /// Design notes (from GDD / issue_01.md):
    ///   - The word is visually implied by faint ink stains in the gutter — 
    ///     only visible with the Gutter Entity hint or if the player looks carefully.
    ///   - Issue 01 Gutter Door: word = "PYRAMID" (pays off visually in Issue 09)
    ///   - Success: gutter entity speaks its next line; Gutter Door marked complete
    ///   - No fail state — player can try indefinitely
    /// </summary>
    public class GutterDoorPuzzle : PuzzleBase
    {
        [Header("Word to trace")]
        [Tooltip("Uppercase word the player must trace through the gutter region.")]
        [SerializeField] private string targetWord = "PYRAMID";

        [Header("Gutter region — a thin RectTransform spanning the gutter gap")]
        [SerializeField] private RectTransform gutterRegion;

        [Header("Letter hint sprites (shown faintly in gutter)")]
        [SerializeField] private Image[] letterHints;

        [Header("Ink trail visual")]
        [SerializeField] private TrailRenderer inkTrail;

        [Header("Gutter entity response")]
        [SerializeField] private CanvasGroup gutterEntityBubble;
        [SerializeField] private Text gutterEntityText;
        [SerializeField, TextArea(1, 3)] private string gutterEntityResponse;

        private string _playerInput = string.Empty;
        private bool _draggingInGutter;
        private Vector2 _lastDragPosition;

        public override void Initialize(PuzzleConfig config)
        {
            base.Initialize(config);
            SetHintsAlpha(0.18f);   // barely visible until attention drawn to gutter

            if (gutterEntityBubble != null)
            {
                gutterEntityBubble.alpha = 0f;
                gutterEntityBubble.interactable = false;
                gutterEntityBubble.blocksRaycasts = false;
            }

            TouchInputManager.Instance.OnDrag    += OnDrag;
            TouchInputManager.Instance.OnDragEnd += OnDragEnd;
        }

        private void OnDestroy()
        {
            if (TouchInputManager.Instance == null) return;
            TouchInputManager.Instance.OnDrag    -= OnDrag;
            TouchInputManager.Instance.OnDragEnd -= OnDragEnd;
        }

        // ── Drag tracking ─────────────────────────────────────────────────────────

        private void OnDrag(Vector2 screenPosition)
        {
            if (!IsActive) return;
            bool inGutter = IsInGutter(screenPosition);

            if (inGutter && !_draggingInGutter)
            {
                // Entered the gutter — start recording
                _draggingInGutter = true;
                _playerInput = string.Empty;
                if (inkTrail != null) inkTrail.enabled = true;
            }
            else if (!inGutter && _draggingInGutter)
            {
                // Exited gutter
                _draggingInGutter = false;
            }

            if (_draggingInGutter)
            {
                _lastDragPosition = screenPosition;
                AppendLetterUnderFinger(screenPosition);
                if (inkTrail != null)
                {
                    // Move trail renderer to the touch position in world space
                    Vector3 world;
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(
                        gutterRegion, screenPosition, null, out world);
                    inkTrail.transform.position = world;
                }
            }
        }

        private void OnDragEnd(Vector2 _)
        {
            if (inkTrail != null) inkTrail.enabled = false;
            _draggingInGutter = false;
        }

        // ── Letter hit-test ───────────────────────────────────────────────────────

        private void AppendLetterUnderFinger(Vector2 screenPosition)
        {
            for (int i = 0; i < letterHints.Length && i < targetWord.Length; i++)
            {
                var rt = letterHints[i].rectTransform;
                if (!RectTransformUtility.RectangleContainsScreenPoint(rt, screenPosition)) continue;

                char expected = targetWord[_playerInput.Length];
                char actual   = (char)('A' + i); // naive: letter A=index0 — real implementation uses per-letter config
                // In the full implementation, each letterHint has a CharacterIndex field.
                // For now we track sequence by the count of distinct hints touched in order.
                if (_playerInput.Length < targetWord.Length)
                {
                    SetHintsAlpha(1f, i);   // illuminate this letter
                    _playerInput += targetWord[_playerInput.Length];
                }

                break;
            }

            if (_playerInput == targetWord)
                StartCoroutine(SolveDoor());
        }

        // ── Solution ──────────────────────────────────────────────────────────────

        private IEnumerator SolveDoor()
        {
            IsActive = false;
            SetHintsAlpha(1f);

            AudioManager.Instance?.OnGutterEntity();

            // Mark gutter door complete in state
            var state = GameManager.Instance.State;
            if (!state.completedGutterDoors.Contains(name))
                state.completedGutterDoors.Add(name);

            // Check all gutter content complete
            // (simple: if this is the last one, flag it — full check would enumerate all door IDs)
            state.allGutterContentComplete = CheckAllGutterDone(state);

            // Show gutter entity bubble
            if (gutterEntityBubble != null)
            {
                gutterEntityText.text = gutterEntityResponse;
                gutterEntityBubble.interactable = true;
                gutterEntityBubble.blocksRaycasts = true;
                yield return FadeGroup(gutterEntityBubble, 0f, 1f, 0.4f);
            }

            Complete();
        }

        private static bool CheckAllGutterDone(ArchiveState state)
        {
            // If all 5 lenses are unlocked AND at least 5 gutter doors complete,
            // we consider all gutter content done (approximation until full registry exists)
            return state.completedGutterDoors.Count >= 5
                && state.unlockedLenses.Count == 5;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private bool IsInGutter(Vector2 screenPosition) =>
            gutterRegion != null &&
            RectTransformUtility.RectangleContainsScreenPoint(gutterRegion, screenPosition);

        private void SetHintsAlpha(float alpha, int highlightIndex = -1)
        {
            for (int i = 0; i < letterHints.Length; i++)
            {
                float a = (highlightIndex >= 0 && i == highlightIndex) ? 1f : alpha;
                var c = letterHints[i].color;
                c.a = a;
                letterHints[i].color = c;
            }
        }

        private static IEnumerator FadeGroup(CanvasGroup cg, float from, float to, float dur)
        {
            float t = 0f;
            cg.alpha = from;
            while (t < dur) { t += Time.deltaTime; cg.alpha = Mathf.Lerp(from, to, t / dur); yield return null; }
            cg.alpha = to;
        }
    }
}
