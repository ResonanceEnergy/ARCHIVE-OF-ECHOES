using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Between-issue summary screen.
    /// Shows the completed issue title, collected Knowledge Keys, and a
    /// preview of the next issue (title + arc label only — no spoilers).
    ///
    /// Also triggers lens-unlock fanfare if this issue granted new lenses.
    /// Navigation: tap CONTINUE to load the next issue, or tap ARCHIVE NOTEBOOK
    /// to review before continuing.
    /// </summary>
    public class IssueCompleteController : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Text issueNumberLabel;
        [SerializeField] private Text issueTitleLabel;
        [SerializeField] private Text arcLabel;

        [Header("Key Collected Notification")]
        [SerializeField] private Transform keyListContainer;
        [SerializeField] private GameObject keyBadgePrefab;

        [Header("Lens Unlock Banner")]
        [SerializeField] private CanvasGroup lensUnlockBanner;
        [SerializeField] private Text lensUnlockLabel;

        [Header("Next Issue Preview")]
        [SerializeField] private Text nextIssueTitleLabel;
        [SerializeField] private Text nextIssueArcLabel;

        [Header("Buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button notebookButton;

        private IssueData _currentIssue;
        private IssueData _nextIssue;

        private void Start()
        {
            var state = GameManager.Instance.State;
            _currentIssue = GameManager.Instance.GetIssue(state.currentIssueId);

            if (_currentIssue == null) return;

            issueNumberLabel.text = $"ISSUE {_currentIssue.issueNumber:00}";
            issueTitleLabel.text  = _currentIssue.title.ToUpper();
            arcLabel.text         = _currentIssue.arc;

            PopulateKeyBadges(state);
            DeterminNextIssue(state);
            StartCoroutine(PlaySequence());

            continueButton.onClick.AddListener(OnContinue);
            notebookButton.onClick.AddListener(() => FindAnyObjectByType<ArchiveNotebook>(FindObjectsInactive.Exclude)?.ToggleOpen());

            lensUnlockBanner.alpha = 0f;
        }

        // ── Sequence ──────────────────────────────────────────────────────────────

        private IEnumerator PlaySequence()
        {
            yield return new WaitForSeconds(0.5f);

            // Lens unlock fanfare (if any)
            foreach (var lens in _currentIssue.unlocksLenses)
            {
                if (!GameManager.Instance.State.IsLensUnlocked(lens)) continue;
                // Already unlocked this session — show banner
                var def = GameManager.Instance.GetLensDefinition(lens);
                lensUnlockLabel.text = $"{def?.displayName.ToUpper() ?? lens.ToString()} LENS UNLOCKED";
                yield return FadeGroup(lensUnlockBanner, 0f, 1f, 0.5f);
                yield return new WaitForSeconds(2f);
                yield return FadeGroup(lensUnlockBanner, 1f, 0f, 0.5f);
            }
        }

        // ── Keys ──────────────────────────────────────────────────────────────────

        private void PopulateKeyBadges(ArchiveState state)
        {
            foreach (var key in _currentIssue.unlocksKeys)
            {
                if (!state.HasKey(key.keyId)) continue;
                var badge = Instantiate(keyBadgePrefab, keyListContainer);
                var lbl = badge.GetComponentInChildren<Text>();
                if (lbl != null) lbl.text = key.displayName.ToUpper();
            }
        }

        // ── Next issue ────────────────────────────────────────────────────────────

        private void DeterminNextIssue(ArchiveState state)
        {
            int next = _currentIssue.issueNumber + 1;
            string nextId = $"issue_{next:00}";
            _nextIssue = GameManager.Instance.GetIssue(nextId);

            if (_nextIssue != null)
            {
                nextIssueTitleLabel.text = _nextIssue.title.ToUpper();
                nextIssueArcLabel.text   = _nextIssue.arc;
            }
            else
            {
                nextIssueTitleLabel.text = "END OF SEASON ONE";
                nextIssueArcLabel.text   = string.Empty;
            }
        }

        // ── Navigation ────────────────────────────────────────────────────────────

        private void OnContinue()
        {
            if (_nextIssue != null)
                GameManager.Instance.BeginIssue(_nextIssue.issueId);
            else
                GameManager.Instance.GoTo2100Frame();
        }

        // ── Fade helper ───────────────────────────────────────────────────────────

        private static IEnumerator FadeGroup(CanvasGroup cg, float from, float to, float dur)
        {
            float t = 0f;
            cg.alpha = from;
            while (t < dur) { t += Time.deltaTime; cg.alpha = Mathf.Lerp(from, to, t / dur); yield return null; }
            cg.alpha = to;
        }
    }
}
