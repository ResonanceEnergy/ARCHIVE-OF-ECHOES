using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// E1 — Ark Assembly Puzzle.
    ///
    /// Four Ark component pieces are scattered across the panel. Each must be dragged
    /// to its correct assembly slot. When all four are placed, a synthesis chord plays
    /// and the ArkInventoryUI triggers the assembled state.
    ///
    /// Each component piece emits its individual note when dropped correctly:
    ///   Carrying Pole   → C4
    ///   West Shard      → E4
    ///   East Shard      → G4
    ///   Mercy Seat      → B4
    /// All four placed → C4+E4+G4+B4 chord.
    ///
    /// Issue 10 ("The Assembly"). Puzzle is gated: only activates after Issues 07–09.
    /// </summary>
    public class ArkAssemblyPuzzle : PuzzleBase
    {
        [System.Serializable]
        public class ArkPiece
        {
            public string        pieceId;
            public RectTransform pieceTransform;
            public Image         pieceImage;
            public AudioClip     pieceNote;
        }

        [System.Serializable]
        public class AssemblySlot
        {
            public string        acceptsPieceId;
            public RectTransform slotTransform;
            public Image         slotHighlight;
            [HideInInspector] public bool filled;
        }

        [Header("Pieces & Slots")]
        [SerializeField] private ArkPiece[]     pieces;
        [SerializeField] private AssemblySlot[] slots;
        [SerializeField] private float          snapRadius = 44f;

        [Header("Feedback")]
        [SerializeField] private Color         correctColor   = new(1f, 0.92f, 0.55f, 1f);
        [SerializeField] private Color         wrongColor     = new(1f, 0.35f, 0.35f, 1f);
        [SerializeField] private AudioClip     chordComplete;
        [SerializeField] private ArkInventoryUI arkInventory;

        private ArkPiece _dragging;
        private Vector2  _dragOrigin;
        private int      _placedCount;

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
                foreach (var piece in pieces)
                {
                    if (!piece.pieceTransform.gameObject.activeSelf) continue;
                    if (Vector2.Distance(pos, piece.pieceTransform.position) <= 44f)
                    {
                        _dragging   = piece;
                        _dragOrigin = piece.pieceTransform.position;
                        return;
                    }
                }
                return;
            }
            _dragging.pieceTransform.position = pos;
        }

        private void HandleDragEnd(Vector2 _)
        {
            if (_dragging == null) return;

            Vector2 dropPos = TouchInputManager.Instance.LastPosition;
            AssemblySlot target = FindSlot(dropPos, _dragging.pieceId);

            if (target != null && !target.filled)
            {
                PlacePiece(_dragging, target);
            }
            else
            {
                // Wrong slot — flash and return
                AssemblySlot wrongTarget = FindNearestSlot(dropPos);
                if (wrongTarget != null && !wrongTarget.filled)
                    StartCoroutine(FlashWrong(wrongTarget));
                _dragging.pieceTransform.position = _dragOrigin;
            }

            _dragging = null;
        }

        // ── Placement ────────────────────────────────────────────────────────────

        private void PlacePiece(ArkPiece piece, AssemblySlot slot)
        {
            slot.filled = true;
            piece.pieceTransform.position = slot.slotTransform.position;
            piece.pieceTransform.SetParent(slot.slotTransform, true);
            piece.pieceImage.color = correctColor;
            if (slot.slotHighlight) slot.slotHighlight.color = correctColor;

            // Individual note
            if (piece.pieceNote)
                AudioManager.Instance?.PlayOneShot(piece.pieceNote);

            Haptic.Play(HapticFeedback.ImpactMedium);

            // Record component in save state
            GameManager.Instance?.State?.AddArkComponent(piece.pieceId);

            _placedCount++;
            if (_placedCount >= pieces.Length)
            {
                StartCoroutine(FinaleChord());
            }
        }

        private IEnumerator FinaleChord()
        {
            yield return new WaitForSeconds(0.1f);

            if (chordComplete)
                AudioManager.Instance?.PlayOneShot(chordComplete);

            AudioManager.Instance?.PlayMotif(MotifType.CircuitClose);
            Haptic.Play(HapticFeedback.ImpactHeavy);

            arkInventory?.TriggerAssembly();
            Complete();
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private AssemblySlot FindSlot(Vector2 pos, string pieceId)
        {
            AssemblySlot best = null;
            float bestDist = snapRadius;
            foreach (var slot in slots)
            {
                if (slot.filled || slot.acceptsPieceId != pieceId) continue;
                float d = Vector2.Distance(pos, slot.slotTransform.position);
                if (d < bestDist) { bestDist = d; best = slot; }
            }
            return best;
        }

        private AssemblySlot FindNearestSlot(Vector2 pos)
        {
            AssemblySlot best = null;
            float bestDist = snapRadius;
            foreach (var slot in slots)
            {
                if (slot.filled) continue;
                float d = Vector2.Distance(pos, slot.slotTransform.position);
                if (d < bestDist) { bestDist = d; best = slot; }
            }
            return best;
        }

        private IEnumerator FlashWrong(AssemblySlot slot)
        {
            if (slot.slotHighlight == null) yield break;
            slot.slotHighlight.color = wrongColor;
            yield return new WaitForSeconds(0.25f);
            slot.slotHighlight.color = Color.white;
        }
    }
}
