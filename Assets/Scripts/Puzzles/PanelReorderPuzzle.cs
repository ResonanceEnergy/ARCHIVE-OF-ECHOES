using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// A-series Panel Reorder Puzzle.
    /// Panels are shuffled; player drags them to the correct sequence.
    /// Puzzle completes when every slot holds its correct panel.
    ///
    /// Scene setup: add DraggablePanel components to the panel child objects
    /// and assign CorrectIndex on each. Add ReorderSlot components to the slot
    /// GameObjects. Wire slots[] and draggablePanels[] in the Inspector.
    /// </summary>
    public class PanelReorderPuzzle : PuzzleBase
    {
        [SerializeField] private ReorderSlot[] slots;
        [SerializeField] private DraggablePanel[] draggablePanels;

        private DraggablePanel _dragging;

        // ── Init ──────────────────────────────────────────────────────────────────

        public override void Initialize(PuzzleConfig config)
        {
            base.Initialize(config);
            Shuffle();

            TouchInputManager.Instance.OnDrag    += OnDrag;
            TouchInputManager.Instance.OnDragEnd += OnDragEnd;
        }

        private void OnDestroy()
        {
            if (TouchInputManager.Instance == null) return;
            TouchInputManager.Instance.OnDrag    -= OnDrag;
            TouchInputManager.Instance.OnDragEnd -= OnDragEnd;
        }

        // ── Shuffle ───────────────────────────────────────────────────────────────

        private void Shuffle()
        {
            // Guarantee at least one panel is out of place
            var shuffled = draggablePanels.OrderBy(_ => Random.value).ToArray();
            while (IsAlreadySolved(shuffled))
                shuffled = draggablePanels.OrderBy(_ => Random.value).ToArray();

            for (int i = 0; i < slots.Length && i < shuffled.Length; i++)
                slots[i].Place(shuffled[i]);
        }

        private bool IsAlreadySolved(DraggablePanel[] order)
        {
            for (int i = 0; i < order.Length; i++)
                if (order[i].CorrectIndex != i) return false;
            return true;
        }

        // ── Drag callbacks ────────────────────────────────────────────────────────

        private void OnDrag(Vector2 screenPosition)
        {
            if (!IsActive) return;

            if (_dragging == null)
            {
                _dragging = PanelAt(screenPosition);
                _dragging?.PickUp();
            }
            else
            {
                _dragging.MoveTo(screenPosition);
            }
        }

        private void OnDragEnd(Vector2 screenPosition)
        {
            if (_dragging == null) return;

            ReorderSlot target = SlotAt(screenPosition);

            if (target != null)
            {
                ReorderSlot origin = _dragging.CurrentSlot;
                DraggablePanel swapped = target.CurrentPanel;

                target.Place(_dragging);

                if (swapped != null && origin != null) origin.Place(swapped);
                else origin?.Clear();
            }
            else
            {
                _dragging.ReturnToSlot();
            }

            _dragging.PutDown();
            _dragging = null;

            CheckSolution();
        }

        // ── Solution check ────────────────────────────────────────────────────────

        private void CheckSolution()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].CurrentPanel == null) return;
                if (slots[i].CurrentPanel.CorrectIndex != i) return;
            }
            Complete();
        }

        // ── Hit-testing ───────────────────────────────────────────────────────────

        private DraggablePanel PanelAt(Vector2 screen)
        {
            foreach (var p in draggablePanels)
            {
                var rt = p.GetComponent<RectTransform>();
                if (rt && RectTransformUtility.RectangleContainsScreenPoint(rt, screen))
                    return p;
            }
            return null;
        }

        private ReorderSlot SlotAt(Vector2 screen)
        {
            foreach (var s in slots)
            {
                var rt = s.GetComponent<RectTransform>();
                if (rt && RectTransformUtility.RectangleContainsScreenPoint(rt, screen))
                    return s;
            }
            return null;
        }
    }

    // ── Supporting MonoBehaviours ─────────────────────────────────────────────────

    /// <summary>Panel that can be picked up and dragged by the player.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggablePanel : MonoBehaviour
    {
        [Tooltip("Zero-based index of the correct slot for this panel")]
        public int CorrectIndex;
        public ReorderSlot CurrentSlot { get; set; }

        private Vector3 _restPosition;

        public void PickUp()
        {
            _restPosition = transform.position;
            GetComponent<CanvasGroup>().alpha = 0.75f;
            transform.SetAsLastSibling();
        }

        public void MoveTo(Vector2 screenPosition)
        {
            var parentRect = transform.parent as RectTransform;
            if (parentRect != null &&
                RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    parentRect, screenPosition, null, out Vector3 world))
            {
                transform.position = world;
            }
        }

        public void PutDown()                  => GetComponent<CanvasGroup>().alpha = 1f;
        public void ReturnToSlot()             => transform.position = _restPosition;
        public void SnapTo(Vector3 position)   => transform.position = position;
    }

    /// <summary>An empty slot that accepts a DraggablePanel.</summary>
    public class ReorderSlot : MonoBehaviour
    {
        public DraggablePanel CurrentPanel { get; private set; }

        public void Place(DraggablePanel panel)
        {
            CurrentPanel = panel;
            if (panel == null) return;
            panel.CurrentSlot = this;
            panel.SnapTo(transform.position);
        }

        public void Clear()
        {
            if (CurrentPanel != null) CurrentPanel.CurrentSlot = null;
            CurrentPanel = null;
        }
    }
}
