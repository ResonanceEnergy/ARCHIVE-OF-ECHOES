using System;
using UnityEngine;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Owns a single comic page: instantiates its panel renderers, handles
    /// swipe navigation events, and reports when all required panels are complete.
    /// </summary>
    public class PageViewController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform panelContainer;
        [SerializeField] private GameObject panelRendererPrefab;

        public PageData Data { get; private set; }

        public event Action OnPageComplete;
        public event Action OnRequestNextPage;
        public event Action OnRequestPreviousPage;

        private PanelRenderer[] _renderers;
        private int _completedCount;
        private int _requiredCount;

        // ── Init ──────────────────────────────────────────────────────────────────

        public void Initialize(PageData data)
        {
            Data = data;
            BuildPanels(data);

            TouchInputManager.Instance.OnSwipeLeft  += _ => OnRequestNextPage?.Invoke();
            TouchInputManager.Instance.OnSwipeRight += _ => OnRequestPreviousPage?.Invoke();
        }

        private void OnDestroy()
        {
            if (TouchInputManager.Instance == null) return;
            TouchInputManager.Instance.OnSwipeLeft  -= _ => OnRequestNextPage?.Invoke();
            TouchInputManager.Instance.OnSwipeRight -= _ => OnRequestPreviousPage?.Invoke();
        }

        // ── Panel construction ────────────────────────────────────────────────────

        private void BuildPanels(PageData data)
        {
            _renderers = new PanelRenderer[data.panels.Length];
            _requiredCount = 0;
            _completedCount = 0;

            var state = GameManager.Instance.State;

            for (int i = 0; i < data.panels.Length; i++)
            {
                PanelData pd = data.panels[i];
                var go = Instantiate(panelRendererPrefab, panelContainer);
                var renderer = go.GetComponent<PanelRenderer>();
                renderer.Initialize(pd);
                renderer.OnPanelRestored += HandlePanelRestored;
                _renderers[i] = renderer;

                if (pd.panelType != PanelType.Static)
                {
                    _requiredCount++;
                    if (state.completedPanelIds.Contains(pd.panelId))
                        _completedCount++;
                }
            }

            // Page might already be complete in a resumed save
            CheckCompletion();
        }

        // ── Completion ────────────────────────────────────────────────────────────

        private void HandlePanelRestored(PanelRenderer _)
        {
            _completedCount++;
            CheckCompletion();
        }

        private void CheckCompletion()
        {
            if (_requiredCount == 0 || _completedCount >= _requiredCount)
                OnPageComplete?.Invoke();
        }
    }
}
