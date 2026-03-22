using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ArchiveOfEchoes.Editor
{
    /// <summary>
    /// Top-level automation menu for Archive of Echoes.
    ///
    ///   Tools → Archive of Echoes →
    ///     1 – Create Folder Structure
    ///     2 – Build Data Assets (Issues 00–01)
    ///     3 – Build Scenes and Prefabs
    ///     4 – Register Build Settings
    ///     ──────────────────────────────
    ///     ⚡ BUILD EVERYTHING (runs all four in order)
    ///
    /// Run "⚡ BUILD EVERYTHING" once on a fresh Unity project to produce a
    /// fully wired vertical-slice ready for art / audio assignment.
    /// </summary>
    public static class ArchiveBootstrapper
    {
        // ── Menu items ────────────────────────────────────────────────────────────

        [MenuItem("Tools/Archive of Echoes/1 – Create Folder Structure")]
        public static void MenuCreateFolders() => CreateFolderStructure();

        [MenuItem("Tools/Archive of Echoes/2 – Build Data Assets (Issues 00-01)")]
        public static void MenuBuildData() => ArchiveDataBuilder.BuildDataAssets();

        [MenuItem("Tools/Archive of Echoes/3 – Build Scenes and Prefabs")]
        public static void MenuBuildScenes() => ArchiveSceneBuilder.BuildAll();

        [MenuItem("Tools/Archive of Echoes/4 – Register Build Settings")]
        public static void MenuRegisterBuild() => RegisterBuildSettings();

        [MenuItem("Tools/Archive of Echoes/5 – Import Art Assets")]
        public static void MenuImportArt() => ArchiveArtImporter.ImportAll();

        [MenuItem("Tools/Archive of Echoes/6 – Import Audio Assets")]
        public static void MenuImportAudio() => ArchiveAudioImporter.ImportAll();

        [MenuItem("Tools/Archive of Echoes/⚡ BUILD EVERYTHING")]
        public static void MenuBuildEverything()
        {
            // Prompt — this will overwrite existing Editor-generated assets
            if (!EditorUtility.DisplayDialog(
                    "Build Everything",
                    "This will create folder structure, ScriptableObject data assets, " +
                    "prefabs, scenes, and register build settings.\n\n" +
                    "Existing generated assets will be overwritten. Continue?",
                    "Build Everything", "Cancel"))
                return;

            Debug.Log("[Archive] ── Step 1: Folder structure ───────────────────");
            CreateFolderStructure();

            Debug.Log("[Archive] ── Step 2: Data assets ────────────────────────");
            ArchiveDataBuilder.BuildDataAssets();

            Debug.Log("[Archive] ── Step 3: Scenes and prefabs ─────────────────");
            ArchiveSceneBuilder.BuildAll();

            Debug.Log("[Archive] ── Step 4: Build settings ─────────────────────");
            RegisterBuildSettings();

            Debug.Log("[Archive] ── Step 5: Import art assets ──────────────────");
            ArchiveArtImporter.ImportAll();

            Debug.Log("[Archive] ── Step 6: Import audio assets ────────────────");
            ArchiveAudioImporter.ImportAll();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Archive of Echoes", "✅ BUILD COMPLETE", "Dismiss");
            Debug.Log("[Archive] ✅ BUILD EVERYTHING complete.");
        }

        // ── BatchBuildAll — dialog-free entry point for -batchmode CLI ─────────────

        /// <summary>
        /// Called by pipeline.py via Unity -batchmode -executeMethod.
        /// No EditorUtility.DisplayDialog calls — safe for headless execution.
        /// Requires generate_art.py and generate_audio.py to have run first
        /// so that the WAV / PNG files exist inside the Assets/ tree.
        /// </summary>
        public static void BatchBuildAll()
        {
            Debug.Log("[Archive] BatchBuildAll — start");
            CreateFolderStructure();
            ArchiveDataBuilder.BuildDataAssets();
            ArchiveSceneBuilder.BuildAll();
            RegisterBuildSettings();
            ArchiveArtImporter.ImportAll();
            ArchiveAudioImporter.ImportAll();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Archive] BatchBuildAll — complete ✅");
        }

        // ── Folder structure ──────────────────────────────────────────────────────

        public static void CreateFolderStructure()
        {
            string[] folders =
            {
                // Scenes
                "Assets/Scenes",

                // Prefabs
                "Assets/Prefabs",
                "Assets/Prefabs/UI",
                "Assets/Prefabs/Panels",

                // ScriptableObjects
                "Assets/ScriptableObjects",
                "Assets/ScriptableObjects/Issues",
                "Assets/ScriptableObjects/Pages",
                "Assets/ScriptableObjects/Panels",
                "Assets/ScriptableObjects/Panels/Issue00",
                "Assets/ScriptableObjects/Panels/Issue01",
                "Assets/ScriptableObjects/Pages/Issue00",
                "Assets/ScriptableObjects/Pages/Issue01",
                "Assets/ScriptableObjects/Keys",
                "Assets/ScriptableObjects/Lenses",

                // Art placeholders
                "Assets/Art",
                "Assets/Art/Panels",
                "Assets/Art/UI",
                "Assets/Art/Lenses",
                "Assets/Art/Cover",

                // Audio placeholders
                "Assets/Audio",
                "Assets/Audio/Drones",
                "Assets/Audio/Motifs",
                "Assets/Audio/SFX",

                // Editor
                "Assets/Editor",

                // Resources (for runtime addressable fallback)
                "Assets/Resources",
            };

            foreach (string folder in folders)
                EnsureFolder(folder);

            AssetDatabase.Refresh();
            Debug.Log($"[Archive] Created {folders.Length} folders.");
        }

        // ── Build settings ────────────────────────────────────────────────────────

        public static void RegisterBuildSettings()
        {
            // Authoritative scene order — matches GameManager.LoadScene(name) calls
            string[] sceneNames = { "Title", "Frame2100", "ComicReader", "IssueComplete" };
            string sceneDir = "Assets/Scenes/";

            var scenes = new EditorBuildSettingsScene[sceneNames.Length];
            for (int i = 0; i < sceneNames.Length; i++)
            {
                string path = $"{sceneDir}{sceneNames[i]}.unity";
                scenes[i] = new EditorBuildSettingsScene(path, true);
            }

            EditorBuildSettings.scenes = scenes;
            Debug.Log("[Archive] Build settings registered: " + string.Join(", ", sceneNames));
        }

        // ── Utilities ─────────────────────────────────────────────────────────────

        public static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path)?.Replace('\\', '/') ?? "Assets";
                string leaf   = Path.GetFileName(path)   ?? path;
                AssetDatabase.CreateFolder(parent, leaf);
            }
        }
    }
}
