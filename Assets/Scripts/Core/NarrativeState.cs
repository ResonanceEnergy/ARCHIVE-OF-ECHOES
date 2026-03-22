using System;
using UnityEngine;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Tracks high-level narrative state: T5 unlock conditions, Scribe escalation,
    /// and which ending variant will trigger at Issue 12.
    /// </summary>
    public class NarrativeState : MonoBehaviour
    {
        public static NarrativeState Instance { get; private set; }

        public event Action OnT5Unlocked;
        public event Action<EndingVariant> OnEndingTriggered;
        public event Action<int> OnScribeEscalationChanged;
        public event Action<int> OnDjedBarChanged;

        // Five Knowledge Keys required to unlock T5 Convergence (from GDD)
        private static readonly string[] T5RequiredKeys =
        {
            "MANDATE",
            "CUSTODIAN",
            "CORRESPONDENCE",
            "MIRROR",
            "CENTER"
        };

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── T5 Convergence ────────────────────────────────────────────────────────

        public bool IsT5Unlocked()
        {
            var state = GameManager.Instance.State;
            foreach (string key in T5RequiredKeys)
                if (!state.HasKey(key)) return false;
            return true;
        }

        /// <summary>
        /// Call this whenever a Knowledge Key is collected.
        /// Fires OnT5Unlocked the first time all five required keys are present.
        /// </summary>
        public void CheckT5Unlock()
        {
            if (IsT5Unlocked())
                OnT5Unlocked?.Invoke();
        }

        // ── Endings ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Called by Issue 12 capstone placement with the active lens or pre-determined EndingVariant.
        /// </summary>
        public void TriggerEnding(LensType lensAtPlacement)
        {
            EndingVariant ending = GameManager.Instance.State.DetermineEnding(lensAtPlacement);
            OnEndingTriggered?.Invoke(ending);
        }

        /// <summary>Overload used by CapstonePlacementPuzzle when ending is already resolved.</summary>
        public void TriggerEnding(EndingVariant ending) => OnEndingTriggered?.Invoke(ending);

        // ── Scribe escalation ─────────────────────────────────────────────────────

        public int GetScribeLevel() => GameManager.Instance.State.scribeEscalationLevel;

        public void IncrementScribeLevel()
        {
            var state = GameManager.Instance.State;
            state.scribeEscalationLevel = Mathf.Min(state.scribeEscalationLevel + 1, 5);
            OnScribeEscalationChanged?.Invoke(state.scribeEscalationLevel);
        }

        public void DecrementScribeEscalation()
        {
            var state = GameManager.Instance.State;
            state.scribeEscalationLevel = Mathf.Max(state.scribeEscalationLevel - 1, 0);
            OnScribeEscalationChanged?.Invoke(state.scribeEscalationLevel);
        }

        public int ScribeEscalationLevel => GameManager.Instance?.State?.scribeEscalationLevel ?? 0;

        // ── Djed progress ─────────────────────────────────────────────────────────

        public int GetDjedBarCount() => GameManager.Instance?.State?.djedBarCount ?? 0;
        public int DjedBarCount       => GameManager.Instance?.State?.djedBarCount ?? 0;

        public void ActivateDjedBar()
        {
            var state = GameManager.Instance.State;
            if (state.djedBarCount < 4)
            {
                state.djedBarCount++;
                OnDjedBarChanged?.Invoke(state.djedBarCount);
            }
        }

        public void SetIdentityMerged()
        {
            var state = GameManager.Instance?.State;
            if (state != null) state.identityMerged = true;
        }

        public void SetResonantProtection(bool active)
        {
            var state = GameManager.Instance?.State;
            if (state != null) state.resonantProtectionActive = active;
        }
    }
}
