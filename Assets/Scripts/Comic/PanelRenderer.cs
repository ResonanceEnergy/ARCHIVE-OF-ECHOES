using System;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Renders and manages a single comic panel.
    /// Handles lens variant switching, corruption overlay, dual-lens visibility,
    /// and delegates puzzle interaction to an attached PuzzleBase component.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class PanelRenderer : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image panelImage;
        [SerializeField] private Image corruptionMask;
        [SerializeField] private Text captionLabel;
        [SerializeField] private Text gutterLabel;

        public PanelData Data { get; private set; }
        public CorruptionState CorruptionState { get; private set; } = CorruptionState.Clean;

        public event Action<PanelRenderer> OnPanelRestored;
        public event Action<PanelRenderer> OnPanelTapped;

        private CanvasGroup _group;
        private PuzzleBase _puzzle;

        // ── Initialisation ────────────────────────────────────────────────────────

        public void Initialize(PanelData data)
        {
            Data = data;
            _group = GetComponent<CanvasGroup>();

            panelImage.sprite = data.panelArtwork;
            captionLabel.text = data.captionText;
            gutterLabel.text = data.gutterText;

            if (data.startsCorrupted)
                ShowCorruption(data.corruptionLevel);
            else
                corruptionMask.gameObject.SetActive(false);

            ApplyLensFilter(LensSystem.Instance.ActiveLens);
            LensSystem.Instance.OnLensChanged += OnLensChanged;

            TouchInputManager.Instance.OnTap += HandleTap;

            AttachPuzzle(data);
        }

        private void OnDestroy()
        {
            if (LensSystem.Instance != null)
                LensSystem.Instance.OnLensChanged -= OnLensChanged;
            if (TouchInputManager.Instance != null)
                TouchInputManager.Instance.OnTap -= HandleTap;
        }

        // ── Lens handling ─────────────────────────────────────────────────────────

        private void OnLensChanged(LensType previous, LensType next) => ApplyLensFilter(next);

        private void ApplyLensFilter(LensType lens)
        {
            // Apply post-process material for visual lens tint
            var def = GameManager.Instance.GetLensDefinition(lens);
            if (def?.lensPostProcessMaterial != null)
                panelImage.material = def.lensPostProcessMaterial;
            else
                panelImage.material = null;

            // Swap to lens-specific variant art if one exists
            bool variantFound = false;
            foreach (var v in Data.lensVariants)
            {
                if (v.lens != lens) continue;
                if (v.altArtwork != null) panelImage.sprite = v.altArtwork;
                if (!string.IsNullOrEmpty(v.altCaption)) captionLabel.text = v.altCaption;
                variantFound = true;
                break;
            }

            if (!variantFound)
            {
                panelImage.sprite = Data.panelArtwork;
                captionLabel.text = Data.captionText;
            }

            // Dual-lens gating
            if (Data.requiresDualLens)
                _group.alpha = LensSystem.Instance.CanViewDualLensPanel(Data) ? 1f : 0f;
        }

        // ── Corruption ────────────────────────────────────────────────────────────

        private void ShowCorruption(float level)
        {
            CorruptionState = CorruptionState.Corrupted;
            corruptionMask.gameObject.SetActive(true);
            corruptionMask.color = new Color(0f, 0f, 0f, Mathf.Clamp01(level));
        }

        // ── Restoration (called by puzzle on completion) ───────────────────────────

        public void MarkRestored()
        {
            CorruptionState = CorruptionState.Restored;
            corruptionMask.gameObject.SetActive(false);

            var state = GameManager.Instance.State;
            if (!state.completedPanelIds.Contains(Data.panelId))
                state.completedPanelIds.Add(Data.panelId);

            foreach (var key in Data.revealsKeys)
                CollectKey(key);

            OnPanelRestored?.Invoke(this);
        }

        private void CollectKey(KnowledgeKeyData key)
        {
            var state = GameManager.Instance.State;
            if (state.HasKey(key.keyId)) return;
            state.collectedKeyIds.Add(key.keyId);
            ArchiveNotebook.Instance?.RevealKey(key);
            NarrativeState.Instance?.CheckT5Unlock();
        }

        // ── Tap ───────────────────────────────────────────────────────────────────

        private void HandleTap(Vector2 screenPosition)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                    transform as RectTransform, screenPosition)) return;
            OnPanelTapped?.Invoke(this);
        }

        // ── Puzzle attachment ─────────────────────────────────────────────────────

        private void AttachPuzzle(PanelData data)
        {
            if (data.panelType == PanelType.Static) return;

            _puzzle = data.panelType switch
            {
                PanelType.Stabilize => gameObject.AddComponent<StabilizePuzzle>(),
                PanelType.Reorder   => gameObject.AddComponent<PanelReorderPuzzle>(),
                _                   => null
            };

            if (_puzzle == null) return;
            _puzzle.Initialize(data.puzzleConfig);
            _puzzle.OnPuzzleComplete += MarkRestored;
        }
    }
}
