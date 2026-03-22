using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Resonant Protection Behaviour — attached to panels in Issue 11 that carry
    /// Resonant faction interference that blocks the Scribe's final escalation move.
    ///
    /// When activated (Issue 11 threshold reached), a golden Resonant shield overlay
    /// fades in and NarrativeState.resonantProtectionActive is set to true.
    ///
    /// This component can also briefly amplify a panel's color saturation when the
    /// player is under the Symbolic or Spiritual lens — a hint that these lenses
    /// align with the Resonant order.
    ///
    /// Resonant activation trigger:
    ///   • Issue 10 complete AND Djed bar count == 4.
    ///
    /// Once active, it cannot be cancelled by the Scribe.
    /// </summary>
    public class ResonantProtectionBehaviour : MonoBehaviour
    {
        [Header("Shield Visual")]
        [SerializeField] private CanvasGroup  shieldOverlay;
        [SerializeField] private float        fadeInDuration = 0.8f;

        [Header("Lens Resonance Amplification")]
        [SerializeField] private Image        panelArtImage;
        [SerializeField] private float        amplifyAmount = 0.15f;   // color saturation boost
        [SerializeField] private float        amplifySpeed  = 2.0f;

        private bool _active;
        private bool _amplifying;

        private void OnEnable()
        {
            if (LensSystem.Instance)
                LensSystem.Instance.OnLensChanged += OnLensChanged;
            if (NarrativeState.Instance)
                NarrativeState.Instance.OnDjedBarChanged += CheckActivation;
        }

        private void OnDisable()
        {
            if (LensSystem.Instance)
                LensSystem.Instance.OnLensChanged -= OnLensChanged;
            if (NarrativeState.Instance)
                NarrativeState.Instance.OnDjedBarChanged -= CheckActivation;
        }

        private void Start()
        {
            int djed = NarrativeState.Instance?.DjedBarCount ?? 0;
            bool issue10Complete = GameManager.Instance?.State?.IsIssueComplete("issue_10") ?? false;
            if (issue10Complete && djed >= 4) Activate();

            UpdateAmplification(LensSystem.Instance?.ActiveLens ?? LensType.Mythic);
        }

        // ── Activation ────────────────────────────────────────────────────────────

        private void CheckActivation(int djedCount)
        {
            if (_active) return;
            bool issue10Complete = GameManager.Instance?.State?.IsIssueComplete("issue_10") ?? false;
            if (issue10Complete && djedCount >= 4) Activate();
        }

        private void Activate()
        {
            _active = true;
            NarrativeState.Instance?.SetResonantProtection(true);
            StartCoroutine(FadeInShield());
            AudioManager.Instance?.PlayMotif(MotifType.LensUnlock);
        }

        private IEnumerator FadeInShield()
        {
            if (shieldOverlay == null) yield break;
            shieldOverlay.gameObject.SetActive(true);
            shieldOverlay.alpha = 0;
            float t = 0;
            while (t < fadeInDuration)
            {
                t += Time.deltaTime;
                shieldOverlay.alpha = Mathf.Lerp(0, 0.85f, t / fadeInDuration);
                yield return null;
            }
            shieldOverlay.alpha = 0.85f;
        }

        // ── Lens resonance amplification ──────────────────────────────────────────

        private void OnLensChanged(LensType prev, LensType next) => UpdateAmplification(next);

        private void UpdateAmplification(LensType lens)
        {
            bool resonant = lens == LensType.Symbolic || lens == LensType.Spiritual;
            if (resonant != _amplifying)
            {
                _amplifying = resonant;
                StopCoroutine(nameof(AmplifyCoroutine));
                StartCoroutine(AmplifyCoroutine(resonant));
            }
        }

        private IEnumerator AmplifyCoroutine(bool amplify)
        {
            if (panelArtImage == null) yield break;

            Color start  = panelArtImage.color;
            Color target = amplify
                ? new Color(
                    Mathf.Clamp01(start.r + amplifyAmount),
                    Mathf.Clamp01(start.g + amplifyAmount * 0.5f),
                    Mathf.Clamp01(start.b),
                    start.a)
                : Color.white;

            float t = 0, dur = 1f / amplifySpeed;
            while (t < dur)
            {
                t += Time.deltaTime;
                panelArtImage.color = Color.Lerp(start, target, t / dur);
                yield return null;
            }
            panelArtImage.color = target;
        }
    }
}
