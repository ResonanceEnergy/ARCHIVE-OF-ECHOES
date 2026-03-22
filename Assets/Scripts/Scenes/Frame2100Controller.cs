using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// The 2100 framing scene — plays at the start of a new game and is returned
    /// to after Issue 12's ending.
    ///
    /// Opening: desaturated dystopian archive, player-character finds the comic.
    ///          The comic is the only full-colour object in the scene.
    ///          A tap on the comic begins Issue 00.
    ///
    /// Closing (epilogue): called by Issue 12 ComicController after the ending cinematic.
    ///                     Shows the sector classification change and the archive light.
    /// </summary>
    public class Frame2100Controller : MonoBehaviour
    {
        public enum Mode { Opening, Closing }

        [Header("Mode")]
        [SerializeField] private Mode mode = Mode.Opening;

        [Header("Scene objects")]
        [SerializeField] private CanvasGroup sceneGroup;
        [SerializeField] private Image comicCover;
        [SerializeField] private Button tapComic;

        [Header("Caption")]
        [SerializeField] private Text captionLabel;

        [Header("Epilogue UI (Mode.Closing only)")]
        [SerializeField] private GameObject classificationReadout;
        [SerializeField] private Text classificationText;
        [SerializeField] private GameObject archiveWindowLight;
        [SerializeField] private Button returnToTitleButton;

        private static readonly Color DesaturatedTint = new(0.72f, 0.72f, 0.72f, 1f);
        private static readonly Color FinalLine_1 = new(0.9f, 0.9f, 0.9f, 1f);

        private void Start()
        {
            if (mode == Mode.Opening)
                StartCoroutine(PlayOpening());
            else
                StartCoroutine(PlayEpilogue());
        }

        // ── Opening ───────────────────────────────────────────────────────────────

        private IEnumerator PlayOpening()
        {
            sceneGroup.alpha = 0f;
            captionLabel.text = string.Empty;

            yield return Fade(sceneGroup, 0f, 1f, 1.5f);

            captionLabel.text = "Sector 7-D. Archive District. 2100 CE.\n" +
                                 "Classification: IRRECOVERABLE.";
            yield return new WaitForSeconds(2.4f);

            captionLabel.text = "You found it between two condemned shelving units.\n" +
                                 "A comic book. The only colour left in the room.";
            yield return new WaitForSeconds(2.8f);

            captionLabel.text = string.Empty;

            // Pulse the comic cover to invite a tap
            StartCoroutine(PulseComicCover());
            tapComic.interactable = true;
            tapComic.onClick.AddListener(OnComicTapped);
        }

        private void OnComicTapped()
        {
            tapComic.interactable = false;
            StopAllCoroutines();
            StartCoroutine(DiveIntoComic());
        }

        private IEnumerator DiveIntoComic()
        {
            // Zoom the cover to fill the screen, then transition into Issue 00
            float t = 0f;
            Vector3 startScale = comicCover.transform.localScale;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(1f, 4f, t / 0.6f);
                comicCover.transform.localScale = startScale * s;
                yield return null;
            }
            GameManager.Instance.BeginIssue("issue_00");
        }

        private IEnumerator PulseComicCover()
        {
            while (true)
            {
                yield return ScaleOverTime(comicCover.transform, 1f, 1.05f, 0.6f);
                yield return ScaleOverTime(comicCover.transform, 1.05f, 1f, 0.6f);
            }
        }

        // ── Epilogue ──────────────────────────────────────────────────────────────

        private IEnumerator PlayEpilogue()
        {
            sceneGroup.alpha = 0f;
            if (classificationReadout != null) classificationReadout.SetActive(false);
            if (archiveWindowLight != null) archiveWindowLight.SetActive(false);
            if (returnToTitleButton != null) returnToTitleButton.gameObject.SetActive(false);

            yield return Fade(sceneGroup, 0f, 1f, 1.5f);
            yield return new WaitForSeconds(1.2f);

            // Classification change
            if (classificationReadout != null)
            {
                classificationReadout.SetActive(true);
                classificationText.text = "CLASSIFICATION: IRRECOVERABLE";
                yield return new WaitForSeconds(1.8f);
                classificationText.text = "CLASSIFICATION: ESSENTIAL RECORD. DO NOT DEMOLISH.";
                yield return new WaitForSeconds(1.2f);
            }

            // Archive light turns on
            if (archiveWindowLight != null)
            {
                archiveWindowLight.SetActive(true);
                yield return new WaitForSeconds(2f);
            }

            // Final caption
            captionLabel.text =
                "Whether what happened inside was real, remembered, or invented\n" +
                "— the Archive thinks it happened.\n\nThat's what archives are for.";
            yield return new WaitForSeconds(3.5f);

            // Final line (from issue_12)
            captionLabel.text = "\"Completion does not mean certainty.\nIt means the story can continue.\"";
            yield return new WaitForSeconds(3f);

            if (returnToTitleButton != null)
            {
                returnToTitleButton.gameObject.SetActive(true);
                returnToTitleButton.onClick.AddListener(() => GameManager.Instance.ReturnToTitle());
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static IEnumerator Fade(CanvasGroup cg, float from, float to, float duration)
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
        }

        private static IEnumerator ScaleOverTime(Transform target, float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(from, to, t / duration);
                target.localScale = Vector3.one * s;
                yield return null;
            }
            target.localScale = Vector3.one * to;
        }
    }
}
