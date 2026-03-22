using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// C1 — Cardinal Alignment Puzzle.
    ///
    /// A compass rose is displayed. The player rotates it by single-finger drag until
    /// all four arms snap onto their target cardinal or sub-cardinal bearings.
    ///
    /// Standard variant (Issues 00–05): 4 arms, fixed 90° targets (N/E/S/W).
    /// Fractional variant  (Issue 06 shaft apex): 4 arms, targets are multiples of 15.5°
    ///   to match the suspected shaft angle — tolerance is ±2.5°.
    ///
    /// Each arm "snaps" with a haptic + visual lock when the player drags within tolerance.
    /// All four locked = puzzle complete.
    /// Wrong release when at least one arm is unlocked = minor shake; no failure.
    /// </summary>
    public class CardinalAlignmentPuzzle : PuzzleBase
    {
        [System.Serializable]
        public class CompassArm
        {
            [Tooltip("Target bearing in degrees (0 = north, clockwise)")]
            public float targetBearing;
            public RectTransform armTransform;
            public Image lockIndicator;
            [HideInInspector] public bool locked;
        }

        [Header("Arms")]
        [SerializeField] private CompassArm[] arms;
        [SerializeField] private RectTransform compassRose;

        [Header("Settings")]
        [SerializeField] private float snapTolerance   = 5f;    // degrees
        [SerializeField] private float rotateSensitivity = 1.2f;
        [SerializeField] private bool  fractionalVariant = false;

        [Header("Feedback")]
        [SerializeField] private Color lockedColor   = new(0.55f, 1f, 0.65f, 1f);
        [SerializeField] private Color unlockedColor = new(0.8f,  0.8f, 0.8f, 1f);
        [SerializeField] private float snapAnimDuration = 0.12f;

        private float _currentAngle;
        private bool  _dragging;
        private Vector2 _lastDragPos;

        protected override void Awake()
        {
            base.Awake();
            if (fractionalVariant)
            {
                snapTolerance = 2.5f;
                // Override arm targets to 15.5° multiples
                for (int i = 0; i < arms.Length; i++)
                    arms[i].targetBearing = i * 15.5f;
            }
        }

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

        private void HandleDrag(Vector2 position)
        {
            if (!_dragging) { _dragging = true; _lastDragPos = position; return; }

            Vector2 center = compassRose.position;
            Vector2 prev   = _lastDragPos - center;
            Vector2 curr   = position - center;

            float angleDelta = Vector2.SignedAngle(prev, curr) * rotateSensitivity;
            _currentAngle = (_currentAngle + angleDelta) % 360f;
            if (_currentAngle < 0) _currentAngle += 360f;

            compassRose.localRotation = Quaternion.Euler(0, 0, -_currentAngle);
            _lastDragPos = position;

            CheckSnaps();
        }

        private void HandleDragEnd(Vector2 _)
        {
            _dragging = false;
            CheckAllLocked();
        }

        // ── Snap logic ────────────────────────────────────────────────────────────

        private void CheckSnaps()
        {
            foreach (var arm in arms)
            {
                if (arm.locked) continue;

                float effectiveBearing = (_currentAngle + arm.targetBearing) % 360f;
                float diff = Mathf.Abs(Mathf.DeltaAngle(effectiveBearing, arm.targetBearing));
                if (diff <= snapTolerance)
                    StartCoroutine(SnapArm(arm));
            }
        }

        private IEnumerator SnapArm(CompassArm arm)
        {
            arm.locked = true;

            // Snap animation: brief scale up then settle
            float elapsed = 0;
            Vector3 startScale = arm.armTransform.localScale;
            Vector3 targetScale = startScale * 1.15f;

            while (elapsed < snapAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / snapAnimDuration;
                arm.armTransform.localScale = Vector3.Lerp(startScale, targetScale, t < 0.5f ? t * 2f : (1f - t) * 2f);
                yield return null;
            }

            arm.armTransform.localScale = startScale;
            arm.lockIndicator.color = lockedColor;

            AudioManager.Instance?.PlayMotif(MotifType.PanelRestored);
            Haptic.Play(HapticFeedback.ImpactLight);
        }

        private void CheckAllLocked()
        {
            foreach (var arm in arms)
                if (!arm.locked) return;
            Complete();
        }

        // ── Visualise unlocked state ──────────────────────────────────────────────

        private void Update()
        {
            foreach (var arm in arms)
            {
                if (!arm.locked && arm.lockIndicator != null)
                    arm.lockIndicator.color = unlockedColor;
            }
        }
    }
}
