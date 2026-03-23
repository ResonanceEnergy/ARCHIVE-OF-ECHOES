using UnityEditor;
using UnityEngine;

namespace ArchiveOfEchoes.Editor
{
    /// <summary>
    /// Generates <c>Assets/ScriptableObjects/Accessibility/AccessibilityDefaults.asset</c>
    /// with sensible default values.  Idempotent: re-running never overwrites an
    /// existing asset, so designer edits are always preserved.
    ///
    ///   Tools → Archive of Echoes → 7 – Build Accessibility Assets
    ///
    /// Also called headlessly from <see cref="ArchiveBootstrapper.BatchBuildAll"/>.
    /// </summary>
    public static class ArchiveAccessibilityBuilder
    {
        private const string AssetDir  = "Assets/ScriptableObjects/Accessibility";
        private const string AssetPath = AssetDir + "/AccessibilityDefaults.asset";

        [MenuItem("Tools/Archive of Echoes/7 \u2013 Build Accessibility Assets")]
        public static void BuildAccessibilityDefaults()
        {
            ArchiveBootstrapper.EnsureFolder(AssetDir);

            // Idempotent: skip if the asset already exists
            if (AssetDatabase.LoadAssetAtPath<AccessibilityData>(AssetPath) != null)
            {
                Debug.Log("[Archive] AccessibilityDefaults.asset already exists — skipped.");
                return;
            }

            var data = ScriptableObject.CreateInstance<AccessibilityData>();
            data.fontSizeMultiplier = 1f;
            data.contrastMode       = AccessibilityData.ContrastMode.Normal;
            data.enableHaptics      = true;
            data.reduceMotion       = false;

            AssetDatabase.CreateAsset(data, AssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Archive] Created {AssetPath}");
        }
    }
}
