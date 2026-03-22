using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// C2 — Shaft Labyrinth Puzzle.
    ///
    /// A branching tunnel cross-section is drawn as UI walls. The player traces a path
    /// from the entrance node to the exit node by holding and dragging their finger.
    /// Touching a wall resets progress (no failure — reset is immediate, no extra penalty).
    ///
    /// Issue 04 variant: single-branch shaft (tutorial-weight difficulty).
    /// Issue 09 variant: 3-level branching shaft; two false exits; choice of Resonant detour.
    ///
    /// Walls are represented as RectTransform hit-boxes. The path tracer uses continuous
    /// raycasting against these rects; any collision triggers a reset.
    /// </summary>
    public class ShaftLabyrinthPuzzle : PuzzleBase
    {
        [System.Serializable]
        public class WallRect
        {
            public RectTransform rect;
        }

        [Header("Structure")]
        [SerializeField] private WallRect[]   walls;
        [SerializeField] private RectTransform startNode;
        [SerializeField] private RectTransform exitNode;
        [SerializeField] private float         exitRadius = 24f;

        [Header("Path Tracer")]
        [SerializeField] private LineRenderer  pathLine;
        [SerializeField] private int           maxPoints = 256;

        [Header("Feedback")]
        [SerializeField] private GameObject    resetFlash;        // brief red overlay
        [SerializeField] private float         flashDuration = 0.08f;

        private bool             _tracing;
        private int              _pointIdx;
        private Vector3[]        _points;
        private Canvas           _canvas;

        protected override void Awake()
        {
            base.Awake();
            _points = new Vector3[maxPoints];
            _canvas = GetComponentInParent<Canvas>();
        }

        private void OnEnable()
        {
            TouchInputManager.Instance.OnDrag    += HandleDrag;
            TouchInputManager.Instance.OnDragEnd += HandleDragEnd;
        }

        private void OnDisable()
        {
            if (TouchInputManager.Instance == null) return;
            TouchInputManager.Instance.OnDrag    -= HandleDrag;
            TouchInputManager.Instance.OnDragEnd -= HandleDragEnd;
        }

        // ── Input ─────────────────────────────────────────────────────────────────

        private void HandleDrag(Vector2 pos)
        {
            if (!_tracing)
            {
                // Start trace only if finger begins inside start node area
                float startDist = Vector2.Distance(pos, (Vector2)startNode.position);
                if (startDist > 32f) return;
                _tracing = true;
                _pointIdx = 0;
                if (pathLine) { pathLine.positionCount = 0; }
            }

            // Wall collision check
            if (IsCollidingWithWall(pos))
            {
                ResetTrace();
                return;
            }

            // Record point
            if (_pointIdx < maxPoints)
            {
                _points[_pointIdx] = pos;
                _pointIdx++;
                if (pathLine)
                {
                    pathLine.positionCount = _pointIdx;
                    pathLine.SetPosition(_pointIdx - 1, pos);
                }
            }

            // Exit check
            float exitDist = Vector2.Distance(pos, (Vector2)exitNode.position);
            if (exitDist <= exitRadius)
            {
                _tracing = false;
                Complete();
            }
        }

        private void HandleDragEnd(Vector2 _)
        {
            if (_tracing) ResetTrace();
        }

        // ── Wall collision ────────────────────────────────────────────────────────

        private bool IsCollidingWithWall(Vector2 screenPos)
        {
            Vector2 local;
            foreach (var wall in walls)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    wall.rect, screenPos, _canvas.worldCamera, out local);
                if (wall.rect.rect.Contains(local)) return true;
            }
            return false;
        }

        // ── Reset ────────────────────────────────────────────────────────────────

        private void ResetTrace()
        {
            _tracing  = false;
            _pointIdx = 0;
            if (pathLine) pathLine.positionCount = 0;
            StartCoroutine(FlashReset());
        }

        private IEnumerator FlashReset()
        {
            if (resetFlash != null) resetFlash.SetActive(true);
            yield return new WaitForSeconds(flashDuration);
            if (resetFlash != null) resetFlash.SetActive(false);
        }
    }
}
