using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// G2 — Gutter Whisper Trace Puzzle.
    ///
    /// A symbol (e.g., the ▲ triangle) is implied in the gutter by scattered ink dots.
    /// The player traces the shape by dragging through the dots in sequence.
    ///
    /// Design notes (from GDD / issue_03.md):
    ///   - Triangle first traced here — Cities Thread → triangle = cities grid overlay
    ///   - Individual ink dots glow briefly when touched; the full shape illuminates on success
    ///   - The shape's open apex points upward — intentionally incomplete (pyramid reading)
    ///   - No fail: player can re-trace indefinitely
    /// </summary>
    public class GutterWhisperTrace : PuzzleBase
    {
        [Header("Trace nodes — assign in the order the player should touch them")]
        [SerializeField] private TraceNode[] nodes;

        [Header("Completion shape image (shown on success)")]
        [SerializeField] private Image completionShape;

        [Header("Gutter entity response")]
        [SerializeField] private CanvasGroup gutterEntityBubble;
        [SerializeField] private Text gutterEntityText;
        [SerializeField, TextArea(1, 3)] private string gutterEntityResponse;

        [Header("Line renderer for drawing the trace path")]
        [SerializeField] private LineRenderer traceLine;

        private int _nextNodeIndex;
        private readonly List<Vector3> _linePositions = new();

        public override void Initialize(PuzzleConfig config)
        {
            base.Initialize(config);

            for (int i = 0; i < nodes.Length; i++)
                nodes[i].Initialize(i, OnNodeTouched);

            if (completionShape != null)
                completionShape.color = new Color(1, 1, 1, 0);

            if (gutterEntityBubble != null)
            {
                gutterEntityBubble.alpha = 0f;
                gutterEntityBubble.interactable = false;
                gutterEntityBubble.blocksRaycasts = false;
            }

            if (traceLine != null)
                traceLine.positionCount = 0;
        }

        // ── Node touched callback ─────────────────────────────────────────────────

        private void OnNodeTouched(int index)
        {
            if (!IsActive) return;

            if (index == _nextNodeIndex)
            {
                nodes[index].SetState(TraceNode.NodeState.Lit);
                _linePositions.Add(nodes[index].transform.position);

                if (traceLine != null)
                {
                    traceLine.positionCount = _linePositions.Count;
                    traceLine.SetPositions(_linePositions.ToArray());
                }

                _nextNodeIndex++;

                if (_nextNodeIndex >= nodes.Length)
                    StartCoroutine(SolveTrace());
            }
            else if (index < _nextNodeIndex)
            {
                // Player touched an already-lit node — acceptable, ignore
            }
            else
            {
                // Wrong order — reset without penalty
                ResetTrace();
            }
        }

        // ── Solution ──────────────────────────────────────────────────────────────

        private IEnumerator SolveTrace()
        {
            IsActive = false;
            AudioManager.Instance?.OnGutterEntity();

            // Flash all nodes lit
            foreach (var node in nodes)
                node.SetState(TraceNode.NodeState.Complete);

            // Reveal completion shape
            yield return FadeImage(completionShape, 0f, 1f, 0.5f);
            yield return new WaitForSeconds(0.8f);

            // Gutter entity response
            if (gutterEntityBubble != null)
            {
                gutterEntityText.text = gutterEntityResponse;
                gutterEntityBubble.interactable = true;
                gutterEntityBubble.blocksRaycasts = true;
                yield return FadeGroup(gutterEntityBubble, 0f, 1f, 0.4f);
            }

            // Track in save
            var state = GameManager.Instance.State;
            if (!state.completedGutterDoors.Contains(name))
                state.completedGutterDoors.Add(name);

            Complete();
        }

        private void ResetTrace()
        {
            _nextNodeIndex = 0;
            _linePositions.Clear();
            if (traceLine != null) traceLine.positionCount = 0;

            foreach (var node in nodes)
                node.SetState(TraceNode.NodeState.Idle);
        }

        // ── Fade helpers ──────────────────────────────────────────────────────────

        private static IEnumerator FadeImage(Image img, float from, float to, float dur)
        {
            if (img == null) yield break;
            float t = 0f;
            Color c = img.color;
            while (t < dur)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(from, to, t / dur);
                img.color = c;
                yield return null;
            }
            c.a = to; img.color = c;
        }

        private static IEnumerator FadeGroup(CanvasGroup cg, float from, float to, float dur)
        {
            float t = 0f;
            cg.alpha = from;
            while (t < dur) { t += Time.deltaTime; cg.alpha = Mathf.Lerp(from, to, t / dur); yield return null; }
            cg.alpha = to;
        }
    }

    // ── Trace Node ────────────────────────────────────────────────────────────────

    /// <summary>
    /// A single ink dot on the gutter whisper trace path.
    /// Attach to a small circular UI image in the gutter region.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class TraceNode : MonoBehaviour
    {
        public enum NodeState { Idle, Lit, Complete }

        public int Index { get; private set; }

        private System.Action<int> _callback;
        private Image _image;

        private static readonly Color IdleColor     = new(0.85f, 0.85f, 0.85f, 0.25f);
        private static readonly Color LitColor      = new(1f, 0.95f, 0.6f, 1f);
        private static readonly Color CompleteColor = new(1f, 1f, 1f, 1f);

        public void Initialize(int index, System.Action<int> onTouched)
        {
            Index = index;
            _callback = onTouched;
            _image = GetComponent<Image>();
            _image.color = IdleColor;

            TouchInputManager.Instance.OnDrag += CheckTouch;
        }

        private void OnDestroy()
        {
            if (TouchInputManager.Instance != null)
                TouchInputManager.Instance.OnDrag -= CheckTouch;
        }

        private void CheckTouch(Vector2 screenPos)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, screenPos))
                _callback?.Invoke(Index);
        }

        public void SetState(NodeState state)
        {
            _image.color = state switch
            {
                NodeState.Lit      => LitColor,
                NodeState.Complete => CompleteColor,
                _                  => IdleColor
            };
        }
    }
}
