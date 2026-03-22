using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// One selectable slot in the LensSelectorUI radial wheel.
    /// Configure five of these (one per lens) as children of the radial root.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class LensSlot : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image     backgroundImage;
        [SerializeField] private Image     iconImage;
        [SerializeField] private Text      lensNameLabel;
        [SerializeField] private GameObject lockedOverlay;

        // ── Static tints ──────────────────────────────────────────────────────────
        private static readonly Color IdleColor    = new(0.10f, 0.08f, 0.15f, 0.90f);
        private static readonly Color HoveredColor = new(1.00f, 0.90f, 0.60f, 0.95f);
        private static readonly Color LockedAlpha  = new(1f, 1f, 1f, 0.35f);

        public  LensType LensType  { get; private set; }
        public  bool     IsUnlocked { get; private set; }
        private bool     _isHovered;
        private CanvasGroup _group;

        private void Awake() => _group = GetComponent<CanvasGroup>();

        // ── Setup ─────────────────────────────────────────────────────────────────

        /// <summary>Called by LensSelectorUI.RefreshSlots() each time the selector opens.</summary>
        public void Setup(LensType type, LensDefinition def)
        {
            LensType   = type;
            IsUnlocked = LensSystem.Instance != null && LensSystem.Instance.IsUnlocked(type);

            if (def != null)
            {
                if (iconImage != null && def.lensIcon != null)
                    iconImage.sprite = def.lensIcon;

                if (lensNameLabel != null)
                    lensNameLabel.text = def.displayName;
            }
            else if (lensNameLabel != null)
            {
                lensNameLabel.text = type.ToString();
            }

            RefreshVisuals();
        }

        // ── Hover ─────────────────────────────────────────────────────────────────

        public void SetUnlocked(bool unlocked)
        {
            IsUnlocked = unlocked;
            RefreshVisuals();
        }

        public void SetActive(bool active)
        {
            transform.localScale = active ? Vector3.one * 1.2f : Vector3.one;
        }

        public void SetHovered(bool hovered)
        {
            _isHovered = hovered;
            RefreshVisuals();
        }

        // ── Visual state ──────────────────────────────────────────────────────────

        private void RefreshVisuals()
        {
            if (_group != null)
                _group.alpha = IsUnlocked ? 1f : 0.35f;

            if (backgroundImage != null)
                backgroundImage.color = _isHovered && IsUnlocked ? HoveredColor : IdleColor;

            if (lockedOverlay != null)
                lockedOverlay.SetActive(!IsUnlocked);
        }
    }
}
