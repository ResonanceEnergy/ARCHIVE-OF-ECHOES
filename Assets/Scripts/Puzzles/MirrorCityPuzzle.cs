using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// C4 — Mirror-City Puzzle.
    ///
    /// A city-grid panel and a star-chart panel are displayed side by side (or overlaid
    /// at 50% opacity). Several grid nodes in the star chart are highlighted;
    /// the player must tap the corresponding mirrored coordinates in the city grid.
    ///
    /// Coordinate mapping: star-chart node (col, row) → city-grid mirror (mirrorCols - col, row).
    /// The mirror axis is the vertical center line between the two panels.
    ///
    /// Issue 04 (intro): 3 nodes, straightforward axial mirror.
    /// Issue 06 (full): 6 nodes, 45° rotational mirror (mirror axis is diagonal).
    /// </summary>
    public class MirrorCityPuzzle : PuzzleBase
    {
        [System.Serializable]
        public class GridNode
        {
            [Tooltip("Column index, 0-based")]
            public int col;
            [Tooltip("Row index, 0-based")]
            public int row;
            public RectTransform nodeTransform;
            [HideInInspector] public bool tapped;
        }

        public enum MirrorMode { AxialVertical, Diagonal45 }

        [Header("Grid")]
        [SerializeField] private GridNode[] starChartNodes;     // highlighted in star chart
        [SerializeField] private GridNode[] cityGridNodes;      // whole city grid (player taps these)
        [SerializeField] private int        gridColumns = 6;
        [SerializeField] private int        gridRows    = 6;
        [SerializeField] private MirrorMode mirrorMode  = MirrorMode.AxialVertical;
        [SerializeField] private float      tapRadius   = 30f;

        [Header("Highlight")]
        [SerializeField] private Color highlightColor = new(0.9f, 0.7f, 0.1f, 1f);
        [SerializeField] private Color correctColor   = new(0.55f, 1f, 0.65f, 1f);
        [SerializeField] private Color wrongColor     = new(1f, 0.35f, 0.35f, 1f);
        [SerializeField] private float wrongFlashSec  = 0.25f;

        private int _confirmedCount;

        protected override void Awake()
        {
            base.Awake();
            HighlightStarNodes();
        }

        private void OnEnable()  => TouchInputManager.Instance.OnTap += HandleTap;
        private void OnDisable()
        {
            if (TouchInputManager.Instance == null) return;
            TouchInputManager.Instance.OnTap -= HandleTap;
        }

        // ── Input ─────────────────────────────────────────────────────────────────

        private void HandleTap(Vector2 pos)
        {
            foreach (var cityNode in cityGridNodes)
            {
                if (Vector2.Distance(pos, cityNode.nodeTransform.position) > tapRadius) continue;
                if (cityNode.tapped) return;

                // Find matching star node target
                GridNode expected = FindExpectedCityNode(cityNode.col, cityNode.row);
                if (expected != null)
                {
                    cityNode.tapped = true;
                    SetNodeColor(cityNode, correctColor);

                    AudioManager.Instance?.PlayMotif(MotifType.PanelRestored);
                    Haptic.Play(HapticFeedback.ImpactLight);
                    _confirmedCount++;
                    if (_confirmedCount >= starChartNodes.Length) Complete();
                }
                else
                {
                    StartCoroutine(FlashWrong(cityNode));
                }
                return;
            }
        }

        // ── Mirror math ───────────────────────────────────────────────────────────

        private GridNode FindExpectedCityNode(int cityCol, int cityRow)
        {
            foreach (var starNode in starChartNodes)
            {
                (int mc, int mr) = MirrorCoord(starNode.col, starNode.row);
                if (mc == cityCol && mr == cityRow) return starNode;
            }
            return null;
        }

        private (int col, int row) MirrorCoord(int starCol, int starRow)
        {
            return mirrorMode switch
            {
                MirrorMode.AxialVertical => (gridColumns - 1 - starCol, starRow),
                MirrorMode.Diagonal45   => (gridRows - 1 - starRow, gridColumns - 1 - starCol),
                _ => (starCol, starRow)
            };
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void HighlightStarNodes()
        {
            foreach (var n in starChartNodes)
                SetNodeColor(n, highlightColor);
        }

        private static void SetNodeColor(GridNode node, Color c)
        {
            var img = node.nodeTransform.GetComponent<Image>();
            if (img != null) img.color = c;
        }

        private IEnumerator FlashWrong(GridNode node)
        {
            SetNodeColor(node, wrongColor);
            yield return new WaitForSeconds(wrongFlashSec);
            SetNodeColor(node, Color.white);
        }
    }
}
