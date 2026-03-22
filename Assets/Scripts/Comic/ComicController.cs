using System.Collections;
using UnityEngine;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Root controller for the ComicReader scene.
    /// Orchestrates page loading, transitions, lens unlocks, and issue completion.
    /// </summary>
    public class ComicController : MonoBehaviour
    {
        [Header("Prefab & Container")]
        [SerializeField] private GameObject pageViewPrefab;
        [SerializeField] private Transform pageContainer;

        [Header("Systems")]
        [SerializeField] private TransitionController transitions;
        [SerializeField] private LensSelectorUI lensSelector;
        [SerializeField] private ArchiveNotebook archiveNotebook;

        private IssueData _issue;
        private PageViewController _activePage;
        private int _pageIndex;

        private void Start()
        {
            var state = GameManager.Instance.State;
            _issue = GameManager.Instance.GetIssue(state.currentIssueId);

            if (_issue == null)
            {
                Debug.LogError($"ComicController: Could not find issue '{state.currentIssueId}'.");
                return;
            }

            LoadPage(state.currentPageIndex);
        }

        // ── Page loading ──────────────────────────────────────────────────────────

        private void LoadPage(int index)
        {
            if (index < 0 || index >= _issue.pages.Length) return;

            if (_activePage != null)
                Destroy(_activePage.gameObject);

            _pageIndex = index;
            GameManager.Instance.State.currentPageIndex = index;

            var go = Instantiate(pageViewPrefab, pageContainer);
            _activePage = go.GetComponent<PageViewController>();
            _activePage.Initialize(_issue.pages[index]);
            _activePage.OnRequestNextPage     += NavigateForward;
            _activePage.OnRequestPreviousPage += NavigateBack;
            _activePage.OnPageComplete        += HandlePageComplete;

            // Unlock lenses and keys granted by this issue on every page load
            // (idempotent — LensSystem and notebook guard against duplicates)
            foreach (var lens in _issue.unlocksLenses)
                LensSystem.Instance.UnlockLens(lens);
            foreach (var key in _issue.unlocksKeys)
                archiveNotebook?.RevealKey(key);
        }

        // ── Navigation ────────────────────────────────────────────────────────────

        private void NavigateForward()
        {
            int next = _pageIndex + 1;
            if (next >= _issue.pages.Length) { CompleteIssue(); return; }
            StartCoroutine(TransitionTo(next, PageTransition.PageTurn));
        }

        private void NavigateBack()
        {
            if (_pageIndex <= 0) return;
            StartCoroutine(TransitionTo(_pageIndex - 1, PageTransition.PageTurn));
        }

        private IEnumerator TransitionTo(int index, PageTransition transition)
        {
            yield return transitions.Play(transition);
            LoadPage(index);
        }

        // ── Issue complete ────────────────────────────────────────────────────────

        private void HandlePageComplete()
        {
            // Nothing to do per-page beyond what LoadPage already handled
        }

        private void CompleteIssue()
        {
            GameManager.Instance.CompleteIssue(_issue.issueId);
            GameManager.Instance.LoadScene("IssueComplete");
        }
    }
}
