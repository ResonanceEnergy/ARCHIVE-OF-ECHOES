using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Singleton root. Persists across scenes.
    /// Owns the ArchiveState and provides access to SO registries.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Issue Registry — assign all IssueData SOs here")]
        [SerializeField] private IssueData[] allIssues;

        [Header("Lens Registry — assign all LensDefinition SOs here")]
        [SerializeField] private LensDefinition[] allLensDefinitions;

        public ArchiveState State { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            State = ArchiveState.Load();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) State.Save();
        }

        private void OnApplicationQuit()
        {
            State.Save();
        }

        // ── Registry lookups ──────────────────────────────────────────────────────

        public IssueData GetIssue(string issueId)
        {
            foreach (var issue in allIssues)
                if (issue.issueId == issueId) return issue;
            Debug.LogWarning($"GameManager: Issue '{issueId}' not found in registry.");
            return null;
        }

        public LensDefinition GetLensDefinition(LensType lens)
        {
            foreach (var def in allLensDefinitions)
                if (def.lensType == lens) return def;
            return null;
        }

        // ── Scene flow ────────────────────────────────────────────────────────────

        public void LoadScene(string sceneName)
        {
            State.Save();
            SceneManager.LoadScene(sceneName);
        }

        public void BeginIssue(string issueId)
        {
            State.currentIssueId = issueId;
            State.currentPageIndex = 0;
            LoadScene("ComicReader");
        }

        public void CompleteIssue(string issueId)
        {
            if (!State.completedIssues.Contains(issueId))
                State.completedIssues.Add(issueId);
            State.Save();
        }

        public void ReturnToTitle() => LoadScene("Title");
        public void GoTo2100Frame() => LoadScene("Frame2100");
    }
}
