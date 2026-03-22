using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Title screen — displays the comic cover and the mandatory fantasy disclaimer.
    /// Player taps the disclaimer's ACKNOWLEDGE button to proceed.
    /// </summary>
    public class TitleScreenController : MonoBehaviour
    {
        [Header("Cover")]
        [SerializeField] private Image coverImage;
        [SerializeField] private CanvasGroup coverGroup;

        [Header("Disclaimer")]
        [SerializeField] private CanvasGroup disclaimerPanel;
        [SerializeField] private Button acknowledgeButton;
        [SerializeField] private Text disclaimerBodyText;

        [Header("Start Button (hidden until disclaimer acknowledged)")]
        [SerializeField] private Button startButton;
        [SerializeField] private CanvasGroup startButtonGroup;

        private const string DisclaimerAcknowledgedKey = "disclaimer_v1_ack";

        private void Start()
        {
            startButtonGroup.alpha = 0f;
            startButtonGroup.interactable = false;
            startButtonGroup.blocksRaycasts = false;

            // Disclaimer body — shown every launch per fantasy-label design rule
            disclaimerBodyText.text =
                "ARCHIVE OF ECHOES: THE ANUNNAKI LEGACY is a work of speculative myth-fantasy.\n\n" +
                "All lore, structures, technologies, and historical interpretations presented " +
                "within this archive are entirely fictional and for entertainment purposes only. " +
                "No factual, archaeological, or scientific claims are made or implied.\n\n" +
                "Names, events, and locations from history are used fictionally.";

            acknowledgeButton.onClick.AddListener(OnDisclaimerAcknowledged);
            startButton.onClick.AddListener(OnStartPressed);
        }

        private void OnDisclaimerAcknowledged()
        {
            PlayerPrefs.SetInt(DisclaimerAcknowledgedKey, 1);
            PlayerPrefs.Save();

            disclaimerPanel.interactable = false;
            disclaimerPanel.blocksRaycasts = false;
            StartCoroutine(FadePanel(disclaimerPanel, 1f, 0f, 0.3f, () =>
            {
                disclaimerPanel.gameObject.SetActive(false);
                ShowStartButton();
            }));
        }

        private void ShowStartButton()
        {
            startButtonGroup.alpha = 0f;
            startButtonGroup.interactable = true;
            startButtonGroup.blocksRaycasts = true;
            StartCoroutine(FadePanel(startButtonGroup, 0f, 1f, 0.4f));
        }

        private void OnStartPressed()
        {
            // If the player has an existing save, go directly to the saved issue
            // Otherwise begin from issue_00
            var state = ArchiveState.Load();
            bool isNewGame = state.completedIssues.Count == 0 && state.completedPanelIds.Count == 0;

            if (isNewGame)
                GameManager.Instance.GoTo2100Frame();
            else
                GameManager.Instance.BeginIssue(state.currentIssueId);
        }

        // ── Utility ───────────────────────────────────────────────────────────────

        private System.Collections.IEnumerator FadePanel(
            CanvasGroup cg, float from, float to, float duration,
            System.Action onComplete = null)
        {
            float t = 0f;
            cg.alpha = from;
            while (t < duration)
            {
                t += Time.deltaTime;
                cg.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            cg.alpha = to;
            onComplete?.Invoke();
        }
    }
}
