using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Diegetic codex owned by the Archivist character.
    /// Reveals Knowledge Keys, stores panel notes, and drives the detective board
    /// that spontaneously forms the pyramid silhouette in Issue 08.
    ///
    /// Attach to a persistent root canvas that is shown/hidden with the notebook
    /// open/close gesture (swipe-up from bottom edge).
    /// </summary>
    public class ArchiveNotebook : MonoBehaviour
    {
        public static ArchiveNotebook Instance { get; private set; }

        [Header("Key Entries — one per Knowledge Key in the game")]
        [SerializeField] private KeyEntryWidget keyEntryPrefab;
        [SerializeField] private Transform keyEntryContainer;

        [Header("Detective Board")]
        [SerializeField] private Transform detectiveBoardRoot;
        [SerializeField] private Image pyramidSilhouette;
        [SerializeField] private float pyramidRevealThreshold = 5;

        [Header("Open / Close")]
        [SerializeField] private CanvasGroup notebookGroup;
        [SerializeField] private float openDuration = 0.25f;

        private bool _isOpen;
        private readonly Dictionary<string, KeyEntryWidget> _entries = new();
        private int _revealedKeyCount;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            notebookGroup.alpha = 0f;
            notebookGroup.interactable = false;
            notebookGroup.blocksRaycasts = false;

            if (pyramidSilhouette != null)
                pyramidSilhouette.color = new Color(1f, 1f, 1f, 0f);
        }

        private void OnEnable() =>
            TouchInputManager.Instance.OnSwipeUp += HandleSwipeUp;

        private void OnDisable()
        {
            if (TouchInputManager.Instance != null)
                TouchInputManager.Instance.OnSwipeUp -= HandleSwipeUp;
        }

        // ── Open / Close ──────────────────────────────────────────────────────────

        private void HandleSwipeUp(Vector2 position)
        {
            // Only treat as notebook open if swipe started in the bottom 15% of screen
            if (position.y / Screen.height > 0.15f) return;
            ToggleOpen();
        }

        public void ToggleOpen() => StartCoroutine(AnimateNotebook(!_isOpen));

        private System.Collections.IEnumerator AnimateNotebook(bool open)
        {
            _isOpen = open;
            notebookGroup.interactable = open;
            notebookGroup.blocksRaycasts = open;

            float from = _isOpen ? 0f : 1f;
            float to   = _isOpen ? 1f : 0f;
            float t = 0f;

            while (t < openDuration)
            {
                t += Time.deltaTime;
                notebookGroup.alpha = Mathf.Lerp(from, to, t / openDuration);
                yield return null;
            }
            notebookGroup.alpha = to;
        }

        // ── Key revelation ────────────────────────────────────────────────────────

        /// <summary>
        /// Called by PanelRenderer when a Knowledge Key is collected.
        /// Safe to call multiple times with the same key (idempotent).
        /// </summary>
        public void RevealKey(KnowledgeKeyData key)
        {
            if (_entries.ContainsKey(key.keyId)) return;

            var widget = Instantiate(keyEntryPrefab, keyEntryContainer);
            widget.Populate(key);
            _entries[key.keyId] = widget;
            _revealedKeyCount++;

            // Check detective-board pyramid reveal
            RefreshPyramidSilhouette();
        }

        // ── Detective board ───────────────────────────────────────────────────────

        /// <summary>
        /// In Issue 08, once enough keys accumulate, the detective board lines up
        /// to imply a pyramid silhouette — no label, just the shape.
        /// </summary>
        private void RefreshPyramidSilhouette()
        {
            if (pyramidSilhouette == null) return;

            float progress = Mathf.Clamp01((float)_revealedKeyCount / pyramidRevealThreshold);
            pyramidSilhouette.color = new Color(1f, 1f, 1f, progress * 0.6f);
        }

        // ── Restore from save ─────────────────────────────────────────────────────

        private void Start()
        {
            // Populate notebook from existing save state on scene load
            var state = GameManager.Instance.State;
            foreach (string keyId in state.collectedKeyIds)
            {
                var key = FindKeyData(keyId);
                if (key != null) RevealKey(key);
            }
        }

        private static KnowledgeKeyData FindKeyData(string keyId)
        {
            // Editor-time: load from Resources/Keys/  
            // Production: inject a registry SO into GameManager instead
            return Resources.Load<KnowledgeKeyData>($"Keys/{keyId}");
        }
    }

}
