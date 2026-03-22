using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Ark Inventory UI — Archive Notebook tab.
    ///
    /// Shows the four recovered Ark components (carrying pole, two shards, mercy seat cover)
    /// and their assembly state. Each component reveals an ability icon when collected;
    /// the fifth "Capstone Lock" ability slot glows-locked until Issue 12 capstone placement.
    ///
    /// Unlocked in the Notebook after Issue 07 ("The Vault") first component collected.
    /// All four assembled in Issue 10 ("The Assembly").
    /// </summary>
    public class ArkInventoryUI : MonoBehaviour
    {
        [System.Serializable]
        public class ComponentSlot
        {
            public string componentId;
            public string displayName;
            public GameObject lockedOverlay;
            public Image componentImage;
            public Text componentLabel;
        }

        [System.Serializable]
        public class AbilitySlot
        {
            public string abilityId;
            public string displayName;
            public Image abilityIcon;
            public Image lockIcon;
            public GameObject glowLockedOverlay;
        }

        [SerializeField] private ComponentSlot[] componentSlots;   // 4 component slots
        [SerializeField] private AbilitySlot[]   abilitySlots;     // 5 ability slots (4 active + Capstone Lock)
        [SerializeField] private GameObject      arkAssembledBanner;
        [SerializeField] private Image           assemblyProgressFill;

        [Header("Colors")]
        [SerializeField] private Color lockedTint    = new(0.4f, 0.4f, 0.45f, 1f);
        [SerializeField] private Color unlockedTint  = new(1f, 0.96f, 0.78f, 1f);
        [SerializeField] private Color capstoneColor = new(1f, 0.85f, 0.2f, 1f);

        public static event Action OnArkAssembled;

        private void OnEnable() => Refresh();

        // ── Refresh ───────────────────────────────────────────────────────────────

        public void Refresh()
        {
            if (GameManager.Instance == null) return;
            var state = GameManager.Instance.State;

            int collectedCount = 0;

            // Components
            foreach (var slot in componentSlots)
            {
                bool have = state.HasArkComponent(slot.componentId);
                if (have) collectedCount++;

                SetSlotState(slot, have);
            }

            // Assembly progress fill
            if (assemblyProgressFill != null)
                assemblyProgressFill.fillAmount = collectedCount / (float)componentSlots.Length;

            // Assembled banner
            if (arkAssembledBanner != null)
                arkAssembledBanner.SetActive(state.arkAssembled);

            // Abilities
            for (int i = 0; i < abilitySlots.Length; i++)
            {
                var slot  = abilitySlots[i];
                bool isCapstone = i == abilitySlots.Length - 1;

                if (isCapstone)
                {
                    // Capstone Lock: glowing-locked until Issue 12
                    bool capstoneReady = state.IsIssueComplete("issue_11");
                    slot.glowLockedOverlay?.SetActive(!capstoneReady);
                    if (slot.abilityIcon != null) slot.abilityIcon.color = capstoneReady ? capstoneColor : lockedTint;
                    if (slot.lockIcon    != null) slot.lockIcon.enabled  = !capstoneReady;
                }
                else
                {
                    bool abilityUnlocked = state.HasArkAbility(slot.abilityId);
                    slot.glowLockedOverlay?.SetActive(false);
                    if (slot.abilityIcon != null) slot.abilityIcon.color = abilityUnlocked ? unlockedTint : lockedTint;
                    if (slot.lockIcon    != null) slot.lockIcon.enabled  = !abilityUnlocked;
                }
            }
        }

        // ── Assembly trigger (called from E1 ArkAssemblyPuzzle) ──────────────────

        public void TriggerAssembly()
        {
            if (GameManager.Instance == null) return;
            var state = GameManager.Instance.State;
            if (state.arkAssembled) return;

            state.arkAssembled = true;
            GameManager.Instance.State.Save();
            OnArkAssembled?.Invoke();
            Refresh();
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void SetSlotState(ComponentSlot slot, bool unlocked)
        {
            if (slot.lockedOverlay   != null) slot.lockedOverlay.SetActive(!unlocked);
            if (slot.componentImage  != null) slot.componentImage.color = unlocked ? unlockedTint : lockedTint;
            if (slot.componentLabel  != null) slot.componentLabel.text  = unlocked ? slot.displayName : "???";
        }
    }
}
