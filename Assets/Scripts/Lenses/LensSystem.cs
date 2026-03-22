using System;
using UnityEngine;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Manages the active lens and lens unlock state.
    /// All systems that need to respond to lens switches subscribe to OnLensChanged.
    /// </summary>
    public class LensSystem : MonoBehaviour
    {
        public static LensSystem Instance { get; private set; }

        /// <summary>Fired when the active lens changes. Args: (previous, next).</summary>
        public event Action<LensType, LensType> OnLensChanged;
        /// <summary>Fired when a lens is newly unlocked.</summary>
        public event Action<LensType> OnLensUnlocked;

        public LensType ActiveLens => GameManager.Instance.State.activeLens;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── Active lens ───────────────────────────────────────────────────────────

        public void SetLens(LensType lens)
        {
            if (!IsUnlocked(lens))
            {
                Debug.LogWarning($"LensSystem: Tried to set locked lens {lens}.");
                return;
            }

            LensType previous = ActiveLens;
            if (previous == lens) return;

            GameManager.Instance.State.activeLens = lens;
            OnLensChanged?.Invoke(previous, lens);
        }

        // ── Unlock ────────────────────────────────────────────────────────────────

        public void UnlockLens(LensType lens)
        {
            var state = GameManager.Instance.State;
            if (state.unlockedLenses.Contains(lens)) return;
            state.unlockedLenses.Add(lens);
            OnLensUnlocked?.Invoke(lens);
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public bool IsUnlocked(LensType lens) =>
            GameManager.Instance.State.IsLensUnlocked(lens);

        /// <summary>
        /// Returns true if the panel's dual-lens requirement is satisfied by current state.
        /// </summary>
        public bool CanViewDualLensPanel(PanelData panel)
        {
            if (!panel.requiresDualLens) return true;
            return IsUnlocked(panel.requiredLensA)
                && IsUnlocked(panel.requiredLensB)
                && (ActiveLens == panel.requiredLensA || ActiveLens == panel.requiredLensB);
        }
    }
}
