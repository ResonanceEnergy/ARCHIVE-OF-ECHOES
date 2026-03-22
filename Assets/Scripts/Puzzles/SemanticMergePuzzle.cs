using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// D4 — Semantic Merge Puzzle.
    ///
    /// Overlapping translucent word/glyph tiles are displayed. The player drags two tiles
    /// to overlap them, and an intersection zone reveals their merged meaning as a new symbol.
    ///
    /// Pairs are pre-defined; overlapping the wrong two tiles produces a null/question
    /// symbol and the tiles bounce apart.
    ///
    /// Issue 06 (intro): 2 pairs, each with one clear correct merge.
    /// Issue 10 (deep): 4 tiles, 2 valid merges, one null — player must find the two valid pairs.
    ///
    /// On merge completion: the merged symbol fades in at the intersection center.
    /// </summary>
    public class SemanticMergePuzzle : PuzzleBase
    {
        [System.Serializable]
        public class MergePair
        {
            public string tileIdA;
            public string tileIdB;
            public Sprite mergedSprite;
        }

        [System.Serializable]
        public class Tile
        {
            public string        tileId;
            public RectTransform tileTransform;
            public Image         tileImage;
            [HideInInspector] public Vector2 originPosition;
            [HideInInspector] public bool    merged;
        }

        [Header("Tiles & Pairs")]
        [SerializeField] private Tile[]       tiles;
        [SerializeField] private MergePair[]  validPairs;
        [SerializeField] private float        overlapCheck    = 32f;
        [SerializeField] private float        bounceDistance  = 60f;
        [SerializeField] private float        bounceDuration  = 0.2f;

        [Header("Merge Result")]
        [SerializeField] private RectTransform mergeResultContainer;
        [SerializeField] private GameObject    mergeResultPrefab;
        [SerializeField] private Color         successColor = new(1f, 0.92f, 0.55f, 1f);
        [SerializeField] private Color         nullColor    = new(0.5f, 0.5f, 0.5f, 0.7f);

        private Tile  _dragging;
        private int   _mergedCount;

        protected override void Awake()
        {
            base.Awake();
            foreach (var tile in tiles)
                tile.originPosition = tile.tileTransform.anchoredPosition;
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

        // ── Drag ─────────────────────────────────────────────────────────────────

        private void HandleDrag(Vector2 pos)
        {
            if (_dragging == null)
            {
                foreach (var tile in tiles)
                {
                    if (tile.merged) continue;
                    if (Vector2.Distance(pos, tile.tileTransform.position) <= 40f)
                    {
                        _dragging = tile;
                        return;
                    }
                }
                return;
            }
            _dragging.tileTransform.position = pos;
        }

        private void HandleDragEnd(Vector2 _)
        {
            if (_dragging == null) return;

            // Check overlap with another tile
            foreach (var other in tiles)
            {
                if (other == _dragging || other.merged) continue;
                float dist = Vector2.Distance(_dragging.tileTransform.position, other.tileTransform.position);
                if (dist <= overlapCheck)
                {
                    MergePair pair = FindPair(_dragging.tileId, other.tileId);
                    if (pair != null)
                        StartCoroutine(DoMerge(_dragging, other, pair));
                    else
                        StartCoroutine(BounceApart(_dragging, other));
                    _dragging = null;
                    return;
                }
            }

            // No overlap — return to origin
            _dragging.tileTransform.position = _dragging.tileTransform.parent.TransformPoint(_dragging.originPosition);
            _dragging = null;
        }

        // ── Merge ─────────────────────────────────────────────────────────────────

        private IEnumerator DoMerge(Tile a, Tile b, MergePair pair)
        {
            a.merged = true;
            b.merged = true;

            Vector2 mid = ((Vector2)a.tileTransform.position + (Vector2)b.tileTransform.position) * 0.5f;

            // Move both to midpoint
            float t = 0, dur = 0.18f;
            Vector2 aStart = a.tileTransform.position;
            Vector2 bStart = b.tileTransform.position;

            while (t < dur)
            {
                t += Time.deltaTime;
                float p = t / dur;
                a.tileTransform.position = Vector2.Lerp(aStart, mid, p);
                b.tileTransform.position = Vector2.Lerp(bStart, mid, p);
                yield return null;
            }

            a.tileImage.color = new Color(1, 1, 1, 0);
            b.tileImage.color = new Color(1, 1, 1, 0);

            // Spawn result
            SpawnMergeResult(mid, pair.mergedSprite, successColor);
            AudioManager.Instance?.PlayMotif(MotifType.PanelRestored);
            Haptic.Play(HapticFeedback.ImpactMedium);

            _mergedCount++;
            if (_mergedCount >= validPairs.Length) Complete();
        }

        private IEnumerator BounceApart(Tile a, Tile b)
        {
            Vector2 mid  = ((Vector2)a.tileTransform.position + (Vector2)b.tileTransform.position) * 0.5f;
            SpawnMergeResult(mid, null, nullColor);   // question symbol

            Vector2 dirA = ((Vector2)a.tileTransform.position - mid).normalized * bounceDistance;
            Vector2 dirB = ((Vector2)b.tileTransform.position - mid).normalized * bounceDistance;

            Vector2 aStart = a.tileTransform.position;
            Vector2 bStart = b.tileTransform.position;

            float t = 0;
            while (t < bounceDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Sin(t / bounceDuration * Mathf.PI);
                a.tileTransform.position = Vector2.Lerp(aStart, (Vector2)a.tileTransform.parent.TransformPoint(a.originPosition), p) + dirA * (1 - p);
                b.tileTransform.position = Vector2.Lerp(bStart, (Vector2)b.tileTransform.parent.TransformPoint(b.originPosition), p) + dirB * (1 - p);
                yield return null;
            }

            a.tileTransform.position = a.tileTransform.parent.TransformPoint(a.originPosition);
            b.tileTransform.position = b.tileTransform.parent.TransformPoint(b.originPosition);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void SpawnMergeResult(Vector2 worldPos, Sprite sprite, Color color)
        {
            if (mergeResultPrefab == null || mergeResultContainer == null) return;
            var go  = Instantiate(mergeResultPrefab, mergeResultContainer);
            go.transform.position = worldPos;
            var img = go.GetComponent<Image>() ?? go.GetComponentInChildren<Image>();
            if (img)
            {
                img.sprite = sprite;
                img.color  = color;
            }
        }

        private MergePair FindPair(string idA, string idB)
        {
            foreach (var p in validPairs)
            {
                if ((p.tileIdA == idA && p.tileIdB == idB) ||
                    (p.tileIdA == idB && p.tileIdB == idA))
                    return p;
            }
            return null;
        }
    }
}
