using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Scribe Corruption Behaviour — attached to any panel that carries Scribe-faction presence.
    ///
    /// Manages visual escalation of Scribe interference across 5 levels:
    ///   Level 0 — Panel is clean.
    ///   Level 1 — Faint ink bleed on panel edges (B1 Stabilize puzzle unlocked).
    ///   Level 2 — Center ink blot; caption corrupted (partial text replaced with glyphs).
    ///   Level 3 — Panel art desaturates; secondary "false restoration" overlay appears.
    ///   Level 4 — Inverted panel flash every 4 seconds; fake "issue complete" UI ghost shown.
    ///   Level 5 — Cascade: all panels in current issue affected; Resonant Protection must activate.
    ///
    /// Escalation source: NarrativeState.ScribeEscalationLevel.
    /// This component listens to NarrativeState and updates visual state accordingly.
    ///
    /// De-escalation: Completing a B-series puzzle on this panel drops escalation by 1.
    /// </summary>
    public class ScribeCorruptionBehaviour : MonoBehaviour
    {
        [Header("Overlays (assign per panel)")]
        [SerializeField] private CanvasGroup  edgeBleedOverlay;
        [SerializeField] private CanvasGroup  centerBlotOverlay;
        [SerializeField] private Image        panelArtImage;
        [SerializeField] private GameObject   falseRestorationOverlay;
        [SerializeField] private CanvasGroup  cascadeVignette;
        [SerializeField] private Text         captionText;

        [Header("Level 2 caption corruption")]
        [SerializeField] private string       originalCaption;
        [SerializeField] private string       corruptedCaption;   // inspector-set mix of glyphs

        [Header("Level 4 flash")]
        [SerializeField] private float        flashInterval = 4f;

        private int   _lastLevel = -1;
        private float _flashTimer;

        private void OnEnable()
        {
            if (NarrativeState.Instance)
                NarrativeState.Instance.OnScribeEscalationChanged += ApplyEscalationLevel;
        }

        private void OnDisable()
        {
            if (NarrativeState.Instance)
                NarrativeState.Instance.OnScribeEscalationChanged -= ApplyEscalationLevel;
        }

        private void Start()
        {
            int level = NarrativeState.Instance?.ScribeEscalationLevel ?? 0;
            ApplyEscalationLevel(level);
        }

        private void Update()
        {
            if (_lastLevel < 4) return;

            _flashTimer += Time.deltaTime;
            if (_flashTimer >= flashInterval)
            {
                _flashTimer = 0;
                StartCoroutine(InvertFlash());
            }
        }

        // ── Apply level ───────────────────────────────────────────────────────────

        private void ApplyEscalationLevel(int level)
        {
            if (level == _lastLevel) return;
            _lastLevel = level;

            SetAlpha(edgeBleedOverlay,    level >= 1 ? Mathf.Lerp(0, 0.6f, (level - 1) / 4f) : 0f);
            SetAlpha(centerBlotOverlay,   level >= 2 ? Mathf.Lerp(0, 0.8f, (level - 2) / 3f) : 0f);
            SetAlpha(cascadeVignette,     level >= 5 ? 0.9f : 0f);

            if (panelArtImage)
                panelArtImage.color = level >= 3
                    ? Color.Lerp(Color.white, Color.grey, (level - 3) / 2f)
                    : Color.white;

            if (falseRestorationOverlay)
                falseRestorationOverlay.SetActive(level >= 3);

            if (captionText)
                captionText.text = level >= 2 ? corruptedCaption : originalCaption;
        }

        // ── Level 4 invert flash ──────────────────────────────────────────────────

        private IEnumerator InvertFlash()
        {
            if (panelArtImage == null) yield break;

            Color orig = panelArtImage.color;
            panelArtImage.color = new Color(1 - orig.r, 1 - orig.g, 1 - orig.b, orig.a);
            yield return new WaitForSeconds(0.08f);
            panelArtImage.color = orig;
        }

        // ── De-escalate (called by B-series puzzle on Complete) ───────────────────

        public void DeEscalate()
        {
            NarrativeState.Instance?.DecrementScribeEscalation();
        }

        // ── Helper ────────────────────────────────────────────────────────────────

        private static void SetAlpha(CanvasGroup cg, float alpha)
        {
            if (cg) cg.alpha = alpha;
        }
    }
}
