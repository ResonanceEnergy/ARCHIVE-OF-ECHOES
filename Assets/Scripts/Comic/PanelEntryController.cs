using System.Collections;
using UnityEngine;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Handles the "Ink Dive" — pinch-in to enter a panel micro-scene,
    /// pinch-out to exit back to the page.
    /// Attach to the root ComicReader canvas alongside ComicController.
    /// </summary>
    public class PanelEntryController : MonoBehaviour
    {
        [Header("Zoom thresholds (normalised to screen height)")]
        [SerializeField] private float entryThreshold = -0.12f;   // pinch-in
        [SerializeField] private float exitThreshold  =  0.12f;   // pinch-out

        [Header("References")]
        [SerializeField] private TransitionController transitions;
        [SerializeField] private GameObject microSceneRoot;

        private bool _inMicroScene;
        private PanelRenderer _enteredPanel;

        private void OnEnable()  => TouchInputManager.Instance.OnPinch += HandlePinch;
        private void OnDisable()
        {
            if (TouchInputManager.Instance != null)
                TouchInputManager.Instance.OnPinch -= HandlePinch;
        }

        private void HandlePinch(float delta)
        {
            if (!_inMicroScene && delta <= entryThreshold)
                TryEnter();
            else if (_inMicroScene && delta >= exitThreshold)
                StartCoroutine(Exit());
        }

        private void TryEnter()
        {
            // Find the topmost panel under the current touch centroid
            var panel = FindActiveInteractablePanel();
            if (panel == null) return;
            _enteredPanel = panel;
            StartCoroutine(Enter(panel));
        }

        private IEnumerator Enter(PanelRenderer panel)
        {
            _inMicroScene = true;
            yield return transitions.Play(PageTransition.InkDive);
            microSceneRoot.SetActive(true);
            panel.GetComponent<MicroScene>()?.Activate(microSceneRoot);
        }

        private IEnumerator Exit()
        {
            yield return transitions.Play(PageTransition.InkDive);
            _enteredPanel?.GetComponent<MicroScene>()?.Deactivate();
            microSceneRoot.SetActive(false);
            _enteredPanel = null;
            _inMicroScene = false;
        }

        private static PanelRenderer FindActiveInteractablePanel()
        {
            // Returns the first Interact-type panel in the current page.
            // A more precise version would raycast to the touch midpoint.
            foreach (var p in FindObjectsOfType<PanelRenderer>())
                if (p.Data.panelType == PanelType.Interact) return p;
            return null;
        }
    }

}
