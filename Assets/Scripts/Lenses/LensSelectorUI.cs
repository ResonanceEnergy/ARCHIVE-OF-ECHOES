using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Radial lens selector overlay.
    /// Opens on two-finger tap-and-drag; closes / applies on finger lift.
    /// Five slots arranged in a circle.  Unlocked slots are fully opaque;
    /// locked slots are dimmed and non-selectable.
    /// </summary>
    public class LensSelectorUI : MonoBehaviour
    {
        [Header("Canvas")]
        [SerializeField] private CanvasGroup selectorGroup;
        [SerializeField] private RectTransform selectorRoot;

        [Header("Slots — assign in order: Mythic, Technologic, Symbolic, Political, Spiritual")]
        [SerializeField] private LensSlot[] slots;

        [Header("Animation")]
        [SerializeField] private float openDuration = 0.18f;
        [SerializeField] private float closeDuration = 0.12f;

        private bool _isOpen;
        private LensType _hoveredLens;
        private bool _hoverDirty;

        private void Awake()
        {
            selectorGroup.alpha = 0f;
            selectorGroup.interactable = false;
            selectorGroup.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            TouchInputManager.Instance.OnTwoFingerDrag += HandleTwoFingerDrag;
            // Finger lift ends a two-finger gesture — we close on the next single-touch begin
            TouchInputManager.Instance.OnTap += _ => TryClose();
        }

        private void OnDisable()
        {
            if (TouchInputManager.Instance == null) return;
            TouchInputManager.Instance.OnTwoFingerDrag -= HandleTwoFingerDrag;
            TouchInputManager.Instance.OnTap -= _ => TryClose();
        }

        private void HandleTwoFingerDrag(Vector2 midpoint, Vector2 delta)
        {
            if (!_isOpen)
                OpenAt(midpoint);
            else
                UpdateHover(midpoint + delta);
        }

        private void OpenAt(Vector2 screenPosition)
        {
            if (_isOpen) return;
            _isOpen = true;

            // Position selector root at the touch midpoint
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                selectorRoot.parent as RectTransform,
                screenPosition,
                null,
                out Vector2 local))
            {
                selectorRoot.anchoredPosition = local;
            }

            RefreshSlots();
            StopAllCoroutines();
            StartCoroutine(AnimateScale(0f, 1f, openDuration));
        }

        private void TryClose()
        {
            if (!_isOpen) return;
            _isOpen = false;

            StopAllCoroutines();
            StartCoroutine(AnimateScale(1f, 0f, closeDuration, onComplete: ApplyHoveredLens));
        }

        private void ApplyHoveredLens()
        {
            if (_hoverDirty)
                LensSystem.Instance.SetLens(_hoveredLens);
            _hoverDirty = false;
        }

        private void UpdateHover(Vector2 screenPosition)
        {
            float minDist = float.MaxValue;
            LensType nearest = _hoveredLens;

            foreach (var slot in slots)
            {
                if (!slot.IsUnlocked) continue;
                Vector2 slotScreen = RectTransformUtility.WorldToScreenPoint(null, slot.transform.position);
                float dist = Vector2.Distance(screenPosition, slotScreen);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = slot.LensType;
                }
            }

            if (nearest != _hoveredLens)
            {
                _hoveredLens = nearest;
                _hoverDirty = true;
                foreach (var slot in slots)
                    slot.SetHovered(slot.LensType == _hoveredLens);
            }
        }

        private void RefreshSlots()
        {
            _hoveredLens = LensSystem.Instance.ActiveLens;
            _hoverDirty = false;

            foreach (var slot in slots)
            {
                slot.SetUnlocked(LensSystem.Instance.IsUnlocked(slot.LensType));
                slot.SetActive(slot.LensType == LensSystem.Instance.ActiveLens);
                slot.SetHovered(slot.LensType == _hoveredLens);
            }
        }

        private IEnumerator AnimateScale(float from, float to, float duration, System.Action onComplete = null)
        {
            selectorGroup.interactable = to > 0f;
            selectorGroup.blocksRaycasts = to > 0f;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(from, to, t / duration);
                selectorRoot.localScale = Vector3.one * s;
                selectorGroup.alpha = s;
                yield return null;
            }

            selectorRoot.localScale = Vector3.one * to;
            selectorGroup.alpha = to;
            onComplete?.Invoke();
        }
    }

}
