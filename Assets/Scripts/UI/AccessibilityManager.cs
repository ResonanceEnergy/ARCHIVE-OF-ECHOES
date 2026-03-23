using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Persistent singleton that applies <see cref="AccessibilityData"/> settings
    /// to every TMP_Text and Image component in the active scene.
    ///
    /// Drop on a root GameObject marked DontDestroyOnLoad (e.g. the GameManager
    /// prefab), assign Settings in the Inspector, and call
    /// <see cref="ApplyToScene"/> after each scene transition completes.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class AccessibilityManager : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────────────

        public static AccessibilityManager Instance { get; private set; }

        [SerializeField] private AccessibilityData settings;

        // Contrast palette constants
        private static readonly Color _highBg = new Color(0f, 0f, 0f, 1f);
        private static readonly Color _highFg = new Color(1f, 1f, 0f, 1f);  // yellow
        private static readonly Color _monoBg = new Color(0f, 0f, 0f, 1f);
        private static readonly Color _monoFg = new Color(1f, 1f, 1f, 1f);

        // ── Public static accessors ───────────────────────────────────────────────

        /// <summary>Font-size multiplier from the loaded settings (1.0 if none assigned).</summary>
        public static float FontScale =>
            Instance != null && Instance.settings != null
                ? Instance.settings.fontSizeMultiplier
                : 1f;

        /// <summary>Returns true unless the user has explicitly disabled haptics.</summary>
        public static bool HapticsEnabled =>
            Instance == null ||
            Instance.settings == null ||
            Instance.settings.enableHaptics;

        /// <summary>Returns true when the reduce-motion accessibility flag is set.</summary>
        public static bool ReduceMotion =>
            Instance != null &&
            Instance.settings != null &&
            Instance.settings.reduceMotion;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() => ApplyToScene();

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Re-applies all accessibility settings to every TMP_Text and Image
        /// currently loaded.  Call this after a scene finishes loading.
        /// </summary>
        public void ApplyToScene()
        {
            if (settings == null) return;
            ApplyFontScale();
            ApplyContrastMode();
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        private void ApplyFontScale()
        {
            float scale = settings.fontSizeMultiplier;
            if (Mathf.Approximately(scale, 1f)) return;

            foreach (var tmp in FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
                tmp.fontSize *= scale;
        }

        private void ApplyContrastMode()
        {
            switch (settings.contrastMode)
            {
                case AccessibilityData.ContrastMode.High:
                    SetColorTheme(_highBg, _highFg);
                    break;
                case AccessibilityData.ContrastMode.Monochrome:
                    SetColorTheme(_monoBg, _monoFg);
                    break;
                default:
                    break;  // Normal — honour scene-authored colours
            }
        }

        private static void SetColorTheme(Color bg, Color fg)
        {
            foreach (var img in FindObjectsByType<Image>(FindObjectsSortMode.None))
                if (img.CompareTag("UIBackground")) img.color = bg;

            foreach (var tmp in FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
                tmp.color = fg;
        }
    }
}
