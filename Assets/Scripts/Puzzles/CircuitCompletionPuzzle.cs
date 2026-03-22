using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// E3 — Circuit Completion Puzzle ("The Nine Nodes").
    ///
    /// A 3×3 grid of nine symbolic nodes is displayed. Each node is assigned one or more
    /// required lens types and/or Ark abilities needed to "activate" it.
    /// The player must:
    ///   1. Switch to the required lens via the lens selector.
    ///   2. Tap the node while the correct lens is active.
    ///
    /// Nodes light up in sequence (any order for first 8; central node last).
    /// When all 9 are lit, the circuit closes: lines flash white, finale chord plays.
    ///
    /// This is the boss-level puzzle of Issue 11 — the only puzzle gating access to Issue 12.
    ///
    /// Node configuration set via NodeConfig entries in Inspector.
    /// </summary>
    public class CircuitCompletionPuzzle : PuzzleBase
    {
        [System.Serializable]
        public class CircuitNode
        {
            public string    nodeId;
            public LensType  requiredLens;
            [Tooltip("Empty = no ability required")]
            public string    requiredAbility;
            public RectTransform nodeTransform;
            public Image     nodeImage;
            public Image     activationGlow;
            [HideInInspector] public bool active;
        }

        [Header("Grid")]
        [SerializeField] private CircuitNode[]      nodes;          // 9 nodes
        [SerializeField] private CircuitNode        centralNode;    // the 9th (activated last)
        [SerializeField] private LineRenderer[]     circuitLines;

        [Header("Colors / Feedback")]
        [SerializeField] private Color  lockedColor   = new(0.35f, 0.35f, 0.40f, 1f);
        [SerializeField] private Color  readyColor    = new(0.8f,  0.75f, 0.30f, 1f);
        [SerializeField] private Color  activeColor   = new(1f,    0.92f, 0.55f, 1f);
        [SerializeField] private float  tapRadius     = 32f;
        [SerializeField] private float  finaleFlashSec = 0.6f;

        private int _activeCount;

        protected override void Awake()
        {
            base.Awake();
            RefreshNodeVisuals();
        }

        private void OnEnable()
        {
            TouchInputManager.Instance.OnTap         += HandleTap;
            LensSystem.Instance.OnLensChanged        += OnLensChanged;
        }

        private void OnDisable()
        {
            if (TouchInputManager.Instance) TouchInputManager.Instance.OnTap     -= HandleTap;
            if (LensSystem.Instance)        LensSystem.Instance.OnLensChanged    -= OnLensChanged;
        }

        // ── Input ─────────────────────────────────────────────────────────────────

        private void HandleTap(Vector2 pos)
        {
            foreach (var node in nodes)
            {
                if (node.active) continue;
                if (node == centralNode && _activeCount < nodes.Length - 1) continue; // central last
                if (Vector2.Distance(pos, node.nodeTransform.position) > tapRadius) continue;

                TryActivate(node);
                return;
            }
        }

        private void OnLensChanged(LensType prev, LensType next) => RefreshNodeVisuals();

        // ── Activation ───────────────────────────────────────────────────────────

        private void TryActivate(CircuitNode node)
        {
            var lens  = LensSystem.Instance?.ActiveLens ?? LensType.Mythic;
            var state = GameManager.Instance?.State;

            bool lensOk    = lens == node.requiredLens;
            bool abilityOk = string.IsNullOrEmpty(node.requiredAbility) ||
                             (state != null && state.HasArkAbility(node.requiredAbility));

            if (!lensOk || !abilityOk)
            {
                StartCoroutine(RejectFlash(node));
                return;
            }

            node.active = true;
            _activeCount++;

            if (node.activationGlow) node.activationGlow.color = activeColor;
            if (node.nodeImage)      node.nodeImage.color       = activeColor;

            AudioManager.Instance?.PlayMotif(MotifType.PanelRestored);
            Haptic.Play(HapticFeedback.ImpactMedium);

            if (_activeCount >= nodes.Length)
                StartCoroutine(CircuitFinale());
        }

        // ── Finale ───────────────────────────────────────────────────────────────

        private IEnumerator CircuitFinale()
        {
            yield return new WaitForSeconds(0.15f);

            // Flash all circuit lines white
            foreach (var line in circuitLines)
            {
                if (line) { line.startColor = Color.white; line.endColor = Color.white; }
            }

            AudioManager.Instance?.PlayCircuitFinale();
            Haptic.Play(HapticFeedback.ImpactHeavy);

            yield return new WaitForSeconds(finaleFlashSec);
            Complete();
        }

        // ── Visual refresh ────────────────────────────────────────────────────────

        private void RefreshNodeVisuals()
        {
            LensType current = LensSystem.Instance?.ActiveLens ?? LensType.Mythic;
            foreach (var node in nodes)
            {
                if (node.active) continue;
                bool ready = node.requiredLens == current;
                if (node.nodeImage) node.nodeImage.color = ready ? readyColor : lockedColor;
            }
        }

        private IEnumerator RejectFlash(CircuitNode node)
        {
            if (node.nodeImage) node.nodeImage.color = new Color(1, 0.3f, 0.3f);
            yield return new WaitForSeconds(0.2f);
            if (!node.active) node.nodeImage.color = lockedColor;
        }
    }
}
