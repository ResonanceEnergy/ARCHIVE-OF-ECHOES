using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// D1 / D2 — Glyph Evolution Puzzle.
    ///
    /// A sequence of 4–6 glyph cards is displayed left-to-right, showing progressive
    /// evolution from an ancient form to a modern derivative.
    ///
    /// Step 1: The player examines the chain.
    /// Step 2: One or two "transition cards" are removed and placed in a scramble bank.
    ///         The player must drag the missing cards back to their correct positions
    ///         in the chain (the same mechanic as PanelReorder but for glyph tiles).
    /// Step 3: On correct placement, each inserted card "morphs" with a brief fade
    ///         animation before locking.
    ///
    /// D1 (Issue 03): 4-card chain, 1 card removed.
    /// D2 (Issue 05): 6-card chain, 2 cards removed; one card is deliberately ambiguous
    ///               — either placement is accepted (intentional design ambiguity beat).
    /// </summary>
    public class GlyphEvolutionPuzzle : PuzzleBase
    {
        [System.Serializable]
        public class GlyphCard
        {
            public string  glyphId;
            public Sprite  glyphSprite;
            public int     correctPosition;  // 0-based index in chain
        }

        [System.Serializable]
        public class ChainSlot
        {
            public RectTransform slotTransform;
            public Image         glyphImage;
            [HideInInspector] public string  occupantId;
            [HideInInspector] public bool    locked;
        }

        [Header("Chain")]
        [SerializeField] private ChainSlot[] chainSlots;
        [SerializeField] private GlyphCard[] allCards;          // all cards (including pre-filled)
        [SerializeField] private int[]       removedPositions;  // which slot indices are empty on start
        [SerializeField] private bool        ambiguousVariant = false;

        [Header("Scramble Bank")]
        [SerializeField] private RectTransform bankContainer;
        [SerializeField] private GameObject    glyphCardPrefab;

        [Header("Feedback")]
        [SerializeField] private float        morphDuration = 0.3f;
        [SerializeField] private Color        lockedGlow    = new(0.55f, 1f, 0.65f, 1f);
        [SerializeField] private float        snapRadius    = 36f;

        // Runtime drag state
        private GlyphCard     _dragCard;
        private RectTransform _dragTransform;
        private int           _lockedCount;

        protected override void Awake()
        {
            base.Awake();
            BuildChain();
            SpawnBankCards();
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

        // ── Build ─────────────────────────────────────────────────────────────────

        private void BuildChain()
        {
            foreach (var card in allCards)
            {
                bool removed = System.Array.IndexOf(removedPositions, card.correctPosition) >= 0;
                if (removed) continue;

                var slot = chainSlots[card.correctPosition];
                slot.glyphImage.sprite = card.glyphSprite;
                slot.occupantId = card.glyphId;
                slot.locked     = true;
            }
        }

        private void SpawnBankCards()
        {
            foreach (int pos in removedPositions)
            {
                var card = CardAtPosition(pos);
                if (card == null) continue;

                var go  = Instantiate(glyphCardPrefab, bankContainer);
                var img = go.GetComponent<Image>() ?? go.GetComponentInChildren<Image>();
                if (img) img.sprite = card.glyphSprite;

                // Store card reference
                var handle = go.AddComponent<GlyphDragHandle>();
                handle.Init(card, this);
            }
        }

        // ── Drag ─────────────────────────────────────────────────────────────────

        public void BeginDrag(GlyphCard card, RectTransform rt)
        {
            _dragCard      = card;
            _dragTransform = rt;
        }

        private void HandleDrag(Vector2 pos)
        {
            if (_dragTransform == null) return;
            _dragTransform.position = pos;
        }

        private void HandleDragEnd(Vector2 _)
        {
            if (_dragCard == null) return;

            Vector2 dropPos = TouchInputManager.Instance.LastPosition;
            ChainSlot target = FindNearestEmptySlot(dropPos);

            if (target != null)
            {
                bool correct = (target.occupantId == null) &&
                               (ambiguousVariant || target == chainSlots[_dragCard.correctPosition]);

                if (correct)
                {
                    target.occupantId = _dragCard.glyphId;
                    StartCoroutine(MorphInto(target, _dragCard.glyphSprite));
                    Destroy(_dragTransform.gameObject);
                }
                else
                {
                    // Return to bank (snap back animation could go here)
                    _dragTransform.position = _dragTransform.parent.position;
                }
            }
            else
            {
                _dragTransform.position = _dragTransform.parent.position;
            }

            _dragCard      = null;
            _dragTransform = null;
        }

        // ── Morph ────────────────────────────────────────────────────────────────

        private IEnumerator MorphInto(ChainSlot slot, Sprite sprite)
        {
            var img = slot.glyphImage;
            float t = 0;
            Color start = img.color;
            Color fade  = new(start.r, start.g, start.b, 0);

            while (t < morphDuration * 0.5f)
            {
                img.color = Color.Lerp(start, fade, t / (morphDuration * 0.5f));
                t += Time.deltaTime;
                yield return null;
            }

            img.sprite = sprite;
            t = 0;

            while (t < morphDuration * 0.5f)
            {
                img.color = Color.Lerp(fade, lockedGlow, t / (morphDuration * 0.5f));
                t += Time.deltaTime;
                yield return null;
            }

            img.color = lockedGlow;
            slot.locked = true;
            _lockedCount++;

            AudioManager.Instance?.PlayMotif(MotifType.PanelRestored);

            if (_lockedCount >= removedPositions.Length) Complete();
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private GlyphCard CardAtPosition(int pos)
        {
            foreach (var c in allCards)
                if (c.correctPosition == pos) return c;
            return null;
        }

        private ChainSlot FindNearestEmptySlot(Vector2 pos)
        {
            ChainSlot best = null;
            float bestDist = snapRadius;
            foreach (var slot in chainSlots)
            {
                if (slot.locked) continue;
                float d = Vector2.Distance(pos, slot.slotTransform.position);
                if (d < bestDist) { bestDist = d; best = slot; }
            }
            return best;
        }
    }

    // ── Drag handle helper component ─────────────────────────────────────────────

    internal class GlyphDragHandle : MonoBehaviour
    {
        private GlyphEvolutionPuzzle _puzzle;
        private GlyphEvolutionPuzzle.GlyphCard _card;

        public void Init(GlyphEvolutionPuzzle.GlyphCard card, GlyphEvolutionPuzzle puzzle)
        {
            _card   = card;
            _puzzle = puzzle;
        }

        private void OnEnable()
        {
            if (TouchInputManager.Instance)
                TouchInputManager.Instance.OnDrag += CheckStart;
        }

        private void OnDisable()
        {
            if (TouchInputManager.Instance)
                TouchInputManager.Instance.OnDrag -= CheckStart;
        }

        private void CheckStart(Vector2 pos)
        {
            if (Vector2.Distance(pos, transform.position) > 40f) return;
            _puzzle.BeginDrag(_card, (RectTransform)transform);
            TouchInputManager.Instance.OnDrag -= CheckStart;
        }
    }
}
