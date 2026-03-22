using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// D3 — Identity Node Puzzle.
    ///
    /// Two identity "nodes" float on screen — one representing the Annunaki administrator
    /// figure, one the modern-era archaeologist figure. They must be merged.
    ///
    /// Mechanic: two-finger pinch-together gesture. The player places one finger on each
    /// node and brings them together until they are within mergeDistance of each other,
    /// at which point the merge animation plays and the puzzle completes.
    ///
    /// Locked until Issue 10 ("The Living Archive"). First appearance at that issue only.
    ///
    /// On completion fires OnIdentityMerged — NarrativeState listens to set identityMerged flag.
    /// </summary>
    public class IdentityNodePuzzle : PuzzleBase
    {
        [Header("Nodes")]
        [SerializeField] private RectTransform nodeA;     // Annunaki figure
        [SerializeField] private RectTransform nodeB;     // Modern figure
        [SerializeField] private float         mergeDistance = 48f;
        [SerializeField] private float         orbitalRadius = 120f;

        [Header("Merge Visual")]
        [SerializeField] private ParticleSystem mergeParticles;
        [SerializeField] private Image          mergeFlash;
        [SerializeField] private float          flashDuration = 0.4f;

        private bool  _merging;
        private float _mergeProgress;

        protected override void Awake()
        {
            base.Awake();
            PositionNodes();
        }

        private void OnEnable()
        {
            TouchInputManager.Instance.OnPinch += HandlePinch;
        }

        private void OnDisable()
        {
            if (TouchInputManager.Instance == null) return;
            TouchInputManager.Instance.OnPinch -= HandlePinch;
        }

        // ── nodes start on opposite edges of orbital radius ──────────────────────

        private void PositionNodes()
        {
            nodeA.anchoredPosition = new Vector2(-orbitalRadius, 0);
            nodeB.anchoredPosition = new Vector2( orbitalRadius, 0);
        }

        // ── Pinch input ───────────────────────────────────────────────────────────

        private void HandlePinch(float delta)
        {
            if (_merging) return;

            // Move nodes toward each other proportional to pinch delta
            float move = -delta * 1.5f;
            Vector2 aPos = nodeA.anchoredPosition;
            Vector2 bPos = nodeB.anchoredPosition;

            aPos.x += move;
            bPos.x -= move;

            // Clamp (nodes can't cross center or go beyond starting points)
            aPos.x = Mathf.Clamp(aPos.x, -orbitalRadius, -mergeDistance * 0.5f);
            bPos.x = Mathf.Clamp(bPos.x,  mergeDistance * 0.5f, orbitalRadius);

            nodeA.anchoredPosition = aPos;
            nodeB.anchoredPosition = bPos;

            float dist = Vector2.Distance(nodeA.position, nodeB.position);
            if (dist <= mergeDistance) StartCoroutine(TriggerMerge());
        }

        // ── Merge sequence ────────────────────────────────────────────────────────

        private IEnumerator TriggerMerge()
        {
            _merging = true;

            // Snap both to center
            float t = 0;
            Vector2 aStart = nodeA.anchoredPosition;
            Vector2 bStart = nodeB.anchoredPosition;
            float dur = 0.25f;

            while (t < dur)
            {
                t += Time.deltaTime;
                float p = t / dur;
                nodeA.anchoredPosition = Vector2.Lerp(aStart, Vector2.zero, p);
                nodeB.anchoredPosition = Vector2.Lerp(bStart, Vector2.zero, p);
                yield return null;
            }

            nodeA.anchoredPosition = Vector2.zero;
            nodeB.anchoredPosition = Vector2.zero;

            mergeParticles?.Play();
            if (mergeFlash != null)
            {
                mergeFlash.gameObject.SetActive(true);
                mergeFlash.color = new Color(1, 1, 1, 0.85f);
                yield return new WaitForSeconds(flashDuration);
                mergeFlash.color = Color.clear;
                mergeFlash.gameObject.SetActive(false);
            }

            AudioManager.Instance?.PlayMotif(MotifType.T5Unlock);
            Haptic.Play(HapticFeedback.ImpactHeavy);

            NarrativeState.Instance?.SetIdentityMerged();
            Complete();
        }
    }
}
