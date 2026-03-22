using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// B-series Stabilize Puzzle.
    /// Player long-presses the corrupted panel for a configurable duration.
    /// A progress ring fills as they hold. Releasing early resets to zero.
    /// </summary>
    public class StabilizePuzzle : PuzzleBase
    {
        [Header("UI (auto-found or assign in prefab)")]
        [SerializeField] private Image progressRing;
        [SerializeField] private ParticleSystem stabilizeParticles;

        private float _requiredDuration;
        private bool _isHolding;
        private float _holdElapsed;

        // ── Init ──────────────────────────────────────────────────────────────────

        public override void Initialize(PuzzleConfig config)
        {
            base.Initialize(config);
            _requiredDuration = config.stabilizeDuration > 0f ? config.stabilizeDuration : 2f;

            if (progressRing != null)
            {
                progressRing.fillAmount = 0f;
                progressRing.gameObject.SetActive(false);
            }

            TouchInputManager.Instance.OnLongPressUpdate   += OnHoldUpdate;
            TouchInputManager.Instance.OnLongPressComplete += OnHoldReleased;
            TouchInputManager.Instance.OnLongPressCancel   += OnHoldCancelled;
        }

        private void OnDestroy()
        {
            if (TouchInputManager.Instance == null) return;
            TouchInputManager.Instance.OnLongPressUpdate   -= OnHoldUpdate;
            TouchInputManager.Instance.OnLongPressComplete -= OnHoldReleased;
            TouchInputManager.Instance.OnLongPressCancel   -= OnHoldCancelled;
        }

        // ── Touch callbacks ───────────────────────────────────────────────────────

        private void OnHoldUpdate(Vector2 position, float elapsed)
        {
            if (!IsActive || !IsOnThisPanel(position)) return;

            _isHolding = true;
            _holdElapsed = elapsed;

            float progress = Mathf.Clamp01(elapsed / _requiredDuration);

            if (progressRing != null)
            {
                progressRing.gameObject.SetActive(true);
                progressRing.fillAmount = progress;
            }

            stabilizeParticles?.Play();

            if (progress >= 1f)
            {
                CleanUp();
                Complete();
            }
        }

        private void OnHoldReleased(Vector2 _)
        {
            if (!_isHolding) return;

            // Released too early — reset
            ResetProgress();
        }

        private void OnHoldCancelled(Vector2 _) => ResetProgress();

        // ── Helpers ───────────────────────────────────────────────────────────────

        private void ResetProgress()
        {
            _isHolding = false;
            _holdElapsed = 0f;
            if (progressRing != null) progressRing.fillAmount = 0f;
            stabilizeParticles?.Stop();
        }

        private void CleanUp()
        {
            if (progressRing != null) progressRing.gameObject.SetActive(false);
            stabilizeParticles?.Stop();
        }

        private bool IsOnThisPanel(Vector2 screenPosition)
        {
            var rt = transform as RectTransform;
            return rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, screenPosition);
        }
    }
}
