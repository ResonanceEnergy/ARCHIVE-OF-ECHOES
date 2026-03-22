using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// E2 — Carry Constraint Puzzle.
    ///
    /// The assembled Ark must be carried across the screen using two fingers placed on
    /// the two carrying poles. The balance constraint: the two touch points must remain
    /// within maxTiltAngle degrees of horizontal while moving the Ark forward.
    ///
    /// If the tilt exceeds the threshold, the "trembling" state begins. Sustained trembling
    /// past maxTremblingTime triggers a drop (full reset). Correct carry for carryDistance
    /// units completes the puzzle.
    ///
    /// Issue 11 ("The Procession"). Cinematic puzzle — slow pace, deliberate.
    /// </summary>
    public class CarryConstraintPuzzle : PuzzleBase
    {
        [Header("Settings")]
        [SerializeField] private float carryDistance    = 280f;   // world units to travel
        [SerializeField] private float maxTiltAngle     = 18f;    // degrees from horizontal
        [SerializeField] private float maxTremblingTime = 1.2f;   // seconds before drop
        [SerializeField] private float moveSensitivity  = 0.8f;

        [Header("Ark Visual")]
        [SerializeField] private RectTransform arkContainer;
        [SerializeField] private Image         tiltIndicator;
        [SerializeField] private Image         trembleWarning;

        [Header("Pole Handles")]
        [SerializeField] private RectTransform poleA;
        [SerializeField] private RectTransform poleB;

        private float _distanceTravelled;
        private float _trembleTimer;
        private bool  _trembling;
        private bool  _carrying;

        private void OnEnable()
        {
            TouchInputManager.Instance.OnTwoFingerDrag += HandleTwoFingerDrag;
        }

        private void OnDisable()
        {
            if (TouchInputManager.Instance == null) return;
            TouchInputManager.Instance.OnTwoFingerDrag -= HandleTwoFingerDrag;
        }

        // ── Two-finger drag ───────────────────────────────────────────────────────

        private void HandleTwoFingerDrag(Vector2 midpoint, Vector2 frameDelta)
        {
            _carrying = true;

            // Estimate tilt from finger positions (delegate to input manager)
            float tiltAngle = TouchInputManager.Instance.TwoFingerTiltAngle;

            // Update tilt indicator
            if (tiltIndicator) tiltIndicator.rectTransform.localRotation = Quaternion.Euler(0, 0, tiltAngle);

            bool tiltOk = Mathf.Abs(tiltAngle) <= maxTiltAngle;

            // Trembling state
            if (!tiltOk)
            {
                _trembleTimer += Time.deltaTime;
                _trembling = true;
                if (trembleWarning) trembleWarning.gameObject.SetActive(true);
                StartCoroutine(ShakeArk());

                if (_trembleTimer >= maxTremblingTime)
                {
                    DropArk();
                    return;
                }
            }
            else
            {
                _trembleTimer = 0;
                _trembling    = false;
                if (trembleWarning) trembleWarning.gameObject.SetActive(false);
            }

            // Carry forward
            float moveX = frameDelta.x * moveSensitivity;
            if (arkContainer)
            {
                var pos = arkContainer.anchoredPosition;
                pos.x += moveX;
                arkContainer.anchoredPosition = pos;
            }

            _distanceTravelled += Mathf.Abs(moveX);
            if (_distanceTravelled >= carryDistance) StartCoroutine(ArrivalSequence());
        }

        // ── Drop ─────────────────────────────────────────────────────────────────

        private void DropArk()
        {
            _distanceTravelled = 0;
            _trembleTimer      = 0;
            _trembling         = false;
            _carrying          = false;

            if (arkContainer)
                arkContainer.anchoredPosition = Vector2.zero;

            if (trembleWarning) trembleWarning.gameObject.SetActive(false);

            AudioManager.Instance?.PlayMotif(MotifType.CorruptionFlash);
        }

        // ── Arrival ───────────────────────────────────────────────────────────────

        private IEnumerator ArrivalSequence()
        {
            _carrying = false;
            AudioManager.Instance?.PlayMotif(MotifType.KnowledgeKeyCollected);
            Haptic.Play(HapticFeedback.ImpactHeavy);
            yield return new WaitForSeconds(0.3f);
            Complete();
        }

        // ── Shake ────────────────────────────────────────────────────────────────

        private IEnumerator ShakeArk()
        {
            if (arkContainer == null) yield break;
            var origin = arkContainer.anchoredPosition;
            arkContainer.anchoredPosition = origin + new Vector2(Random.Range(-4f, 4f), Random.Range(-2f, 2f));
            yield return new WaitForSeconds(0.04f);
            if (!_trembling) arkContainer.anchoredPosition = origin;
        }
    }
}
