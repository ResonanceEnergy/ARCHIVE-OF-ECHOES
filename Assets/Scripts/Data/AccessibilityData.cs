using UnityEngine;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// ScriptableObject carrying all runtime accessibility preferences.
    /// Create via the menu: Assets → Create → Archive of Echoes → Accessibility Data.
    /// The default asset is generated automatically by ArchiveAccessibilityBuilder.
    /// </summary>
    [CreateAssetMenu(
        fileName = "AccessibilityDefaults",
        menuName  = "Archive of Echoes/Accessibility Data")]
    public class AccessibilityData : ScriptableObject
    {
        /// <summary>Colour contrast theme applied to UI elements at runtime.</summary>
        public enum ContrastMode
        {
            Normal,      // scene-authored colours, no override
            High,        // black background, bright-yellow foreground
            Monochrome,  // black background, white foreground
        }

        [Tooltip("Multiplier applied to all TMP font sizes at scene load. Range 1.0–2.0.")]
        [Range(1f, 2f)]
        public float fontSizeMultiplier = 1f;

        [Tooltip("Colour contrast theme applied to tagged UI elements.")]
        public ContrastMode contrastMode = ContrastMode.Normal;

        [Tooltip("Whether iOS haptic feedback is enabled.")]
        public bool enableHaptics = true;

        [Tooltip("Suppresses non-critical animated transitions (lens-switch radial, panel snap).")]
        public bool reduceMotion = false;
    }
}
