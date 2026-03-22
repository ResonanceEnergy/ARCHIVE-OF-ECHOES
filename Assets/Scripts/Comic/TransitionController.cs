using System.Collections;
using UnityEngine;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Handles all page-level transitions: fade, ink-dive, lens switch, corruption flash.
    /// Attach to a full-screen CanvasGroup overlay that sits above all comic content.
    /// </summary>
    public class TransitionController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup overlay;
        [SerializeField] private UnityEngine.UI.Image overlayImage;
        [SerializeField] private float defaultDuration = 0.3f;

        // Carefully chosen colours that feel like ink, not just black
        private static readonly Color InkColor       = new(0.05f, 0.03f, 0.08f, 1f);
        private static readonly Color CorruptionColor = new(0.75f, 0.08f, 0.08f, 1f);
        private static readonly Color GutterColor    = Color.black;

        public IEnumerator Play(PageTransition transition)
        {
            float duration = DurationFor(transition);
            overlayImage.color = ColorFor(transition);

            yield return Fade(0f, 1f, duration * 0.4f);

            // Micro-hold at peak — longer for the cinematic InkDive
            float hold = transition == PageTransition.InkDive ? 0.15f : 0.04f;
            yield return new WaitForSeconds(hold);

            yield return Fade(1f, 0f, duration * 0.6f);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                overlay.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            overlay.alpha = to;
        }

        private float DurationFor(PageTransition t) => t switch
        {
            PageTransition.InkDive      => 0.5f,
            PageTransition.LensSwitch   => 0.22f,
            PageTransition.Corruption   => 0.35f,
            PageTransition.GutterEntity => 0.4f,
            _                           => defaultDuration
        };

        private Color ColorFor(PageTransition t) => t switch
        {
            PageTransition.Corruption   => CorruptionColor,
            PageTransition.GutterEntity => GutterColor,
            _                           => InkColor
        };
    }
}
