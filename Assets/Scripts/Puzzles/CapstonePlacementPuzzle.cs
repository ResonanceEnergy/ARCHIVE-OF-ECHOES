using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// E4 — Capstone Placement Puzzle (The Last Panel).
    ///
    /// The Ark's missing capstone is displayed above a pyramid silhouette slot.
    /// The player must:
    ///   1. Orient the capstone using a single-finger rotate gesture until it locks to
    ///      the correct alignment (pyramid angle ≈ 51.8°).
    ///   2. Then place it with a deliberate two-finger downward drag into the slot.
    ///
    /// On successful placement the circuit-close cinematic fires, the ending determination
    /// runs, and the chosen ending scene begins.
    ///
    /// This puzzle cannot be failed — the capstone cannot be dropped, only re-oriented.
    /// The deliberateness of the two-finger press IS the game's closing ceremony.
    ///
    /// Issue 12 ("The Last Panel").
    /// </summary>
    public class CapstonePlacementPuzzle : PuzzleBase
    {
        public static event Action OnCapstoneSeated;

        [Header("Capstone")]
        [SerializeField] private RectTransform capstone;
        [SerializeField] private RectTransform pyramidSlot;
        [SerializeField] private float         correctAngle    = 51.8f;   // pyramid slope angle
        [SerializeField] private float         angleTolerance  = 3f;
        [SerializeField] private float         rotateSensitivity = 1.0f;

        [Header("Placement")]
        [SerializeField] private float         placementThreshold = 80f;  // px downward drag needed
        [SerializeField] private float         slotSnapDist       = 48f;

        [Header("Cinematic")]
        [SerializeField] private Image         circuitFlash;
        [SerializeField] private float         flashDuration = 1.2f;
        [SerializeField] private CanvasGroup   sceneGroup;

        private float _currentAngle;
        private bool  _oriented;
        private bool  _placing;
        private float _placeProgress;
        private Vector2 _lastRotateDragPos;

        private void OnEnable()
        {
            TouchInputManager.Instance.OnDrag          += HandleRotate;
            TouchInputManager.Instance.OnTwoFingerDrag += HandleTwoFingerPlace;
        }

        private void OnDisable()
        {
            if (TouchInputManager.Instance == null) return;
            TouchInputManager.Instance.OnDrag          -= HandleRotate;
            TouchInputManager.Instance.OnTwoFingerDrag -= HandleTwoFingerPlace;
        }

        // ── Step 1: Orientation ───────────────────────────────────────────────────

        private void HandleRotate(Vector2 pos)
        {
            if (_oriented || _placing) return;
            if (Vector2.Distance(pos, capstone.position) > 60f) return;

            Vector2 delta = pos - _lastRotateDragPos;
            _lastRotateDragPos = pos;

            _currentAngle += delta.x * rotateSensitivity;
            capstone.localRotation = Quaternion.Euler(0, 0, _currentAngle);

            float diff = Mathf.Abs(Mathf.DeltaAngle(_currentAngle, correctAngle));
            if (diff <= angleTolerance)
            {
                _currentAngle = correctAngle;
                capstone.localRotation = Quaternion.Euler(0, 0, correctAngle);
                _oriented = true;

                AudioManager.Instance?.PlayMotif(MotifType.KnowledgeKeyCollected);
                Haptic.Play(HapticFeedback.ImpactMedium);
            }
        }

        // ── Step 2: Two-finger downward placement ─────────────────────────────────

        private void HandleTwoFingerPlace(Vector2 midpoint, Vector2 frameDelta)
        {
            if (!_oriented || _placing) return;

            // Downward drag only
            if (frameDelta.y >= 0) return;

            _placeProgress += -frameDelta.y;

            // Move capstone downward
            if (capstone)
            {
                var pos = capstone.anchoredPosition;
                pos.y -= -frameDelta.y;
                capstone.anchoredPosition = pos;
            }

            if (_placeProgress >= placementThreshold)
            {
                float dist = Vector2.Distance(capstone.position, pyramidSlot.position);
                if (dist <= slotSnapDist)
                {
                    _placing = true;
                    StartCoroutine(SeatCapstone());
                }
            }
        }

        // ── Placement cinematic ───────────────────────────────────────────────────

        private IEnumerator SeatCapstone()
        {
            // Snap to slot
            float t = 0, dur = 0.3f;
            Vector2 startPos = capstone.position;

            while (t < dur)
            {
                t += Time.deltaTime;
                capstone.position = Vector2.Lerp(startPos, pyramidSlot.position, t / dur);
                capstone.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(_currentAngle, correctAngle, t / dur));
                yield return null;
            }

            capstone.position = pyramidSlot.position;

            // Circuit flash — all five lens colors bloom
            if (circuitFlash != null)
            {
                circuitFlash.gameObject.SetActive(true);
                circuitFlash.color = Color.white;
                yield return new WaitForSeconds(flashDuration * 0.3f);

                // Fade to dark
                t = 0;
                while (t < flashDuration * 0.7f)
                {
                    t += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, t / (flashDuration * 0.7f));
                    circuitFlash.color = new Color(1, 1, 1, alpha);
                    yield return null;
                }

                circuitFlash.color = Color.clear;
                circuitFlash.gameObject.SetActive(false);
            }

            AudioManager.Instance?.PlayCircuitFinale();
            Haptic.Play(HapticFeedback.ImpactHeavy);

            OnCapstoneSeated?.Invoke();

            // Determine and trigger ending
            LensType lastLens = LensSystem.Instance?.ActiveLens ?? LensType.Mythic;
            NarrativeState.Instance?.TriggerEnding(
                GameManager.Instance?.State?.DetermineEnding(lastLens) ?? EndingVariant.LockedStability
            );

            yield return new WaitForSeconds(0.5f);
            Complete();
        }
    }
}
