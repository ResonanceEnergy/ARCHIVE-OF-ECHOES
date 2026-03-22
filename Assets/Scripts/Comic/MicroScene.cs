using UnityEngine;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Attach to any PanelRenderer that has an interactive micro-scene.
    /// PanelEntryController discovers this component on pinch-in and calls
    /// Activate/Deactivate to show the correct micro-scene content.
    /// </summary>
    public class MicroScene : MonoBehaviour
    {
        [Tooltip("Matches the prefab name / addressable key for this scene's content")]
        [SerializeField] private string microSceneId;

        private GameObject _microRoot;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Called by PanelEntryController after an InkDive transition completes.
        /// Sets microSceneRoot active and stamps this scene's content into it.
        /// </summary>
        public void Activate(GameObject microSceneRoot)
        {
            _microRoot = microSceneRoot;
            microSceneRoot.SetActive(true);

            // Placeholder: production code will load an Addressable sub-scene here.
            // For now, just mark the root as belonging to this panel in the editor.
            Debug.Log($"[MicroScene] Entering '{microSceneId}'");
        }

        /// <summary>Called by PanelEntryController when the player pinches out.</summary>
        public void Deactivate()
        {
            if (_microRoot != null)
                _microRoot.SetActive(false);

            Debug.Log($"[MicroScene] Exiting '{microSceneId}'");
            _microRoot = null;
        }

        public string MicroSceneId => microSceneId;
    }
}
