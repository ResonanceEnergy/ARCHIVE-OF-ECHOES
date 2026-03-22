using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ArchiveOfEchoes.Editor
{
    /// <summary>
    /// Reads Assets/Audio/audio_manifest.json (produced by generate_audio.py)
    /// and wires every WAV file into the appropriate UnityEngine.Object:
    ///
    ///   drone_*        → LensDefinition.ambientNote
    ///                    (Assets/ScriptableObjects/Lenses/Lens_{X}.asset)
    ///
    ///   sfx_*          → AudioManager serialized field on
    ///                    Assets/Prefabs/AudioManagerPrefab.prefab
    ///
    /// Mapping follows the exact field names registered in AudioManager.cs.
    /// Invoke via: Tools → Archive of Echoes → 6 – Import Audio Assets
    /// or call ArchiveAudioImporter.ImportAll() from code / batchmode.
    /// </summary>
    public static class ArchiveAudioImporter
    {
        private const string ManifestPath   = "Assets/Audio/audio_manifest.json";
        private const string LensesFolder   = "Assets/ScriptableObjects/Lenses";
        private const string PrefabPath     = "Assets/Prefabs/AudioManagerPrefab.prefab";

        // ── SFX id → AudioManager field name ─────────────────────────────────────

        private static readonly Dictionary<string, string> SfxFieldMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "sfx_panel_restored",           "panelRestoredClip" },
            { "sfx_lens_unlock",              "lensUnlockClip" },
            { "sfx_page_flip",                "pageFlipClip" },
            { "sfx_corruption_flash",         "corruptionFlashClip" },
            { "sfx_gutter_entity",            "gutterEntityClip" },
            { "sfx_knowledge_key_collected",  "knowledgeKeyCollectedClip" },
            { "sfx_t5_unlock",                "t5UnlockClip" },
            { "sfx_djed_bar_activated",       "djedBarActivatedClip" },
            { "sfx_circuit_close",            "circuitCloseClip" },
        };

        // ── Drone id → Lens SO name (Lens_{lensName}.asset) ──────────────────────

        private static readonly Dictionary<string, string> DroneToLens =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "drone_mythic",       "Lens_Mythic" },
            { "drone_technologic",  "Lens_Technologic" },
            { "drone_symbolic",     "Lens_Symbolic" },
            { "drone_political",    "Lens_Political" },
            { "drone_spiritual",    "Lens_Spiritual" },
        };

        // ── Public entry point ────────────────────────────────────────────────────

        public static void ImportAll()
        {
            if (!File.Exists(ManifestPath))
            {
                Debug.LogWarning($"[AudioImporter] Manifest not found: {ManifestPath}\n" +
                                  "Run  python generate_audio.py  first.");
                return;
            }

            var manifest = ParseManifest(File.ReadAllText(ManifestPath));
            int ok = 0, skip = 0, fail = 0;

            foreach (var kvp in manifest)
            {
                string id   = kvp.Key;
                string path = kvp.Value;

                // Skip non-audio entries or missing files
                if (!path.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) &&
                    !path.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) &&
                    !path.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
                {
                    skip++;
                    continue;
                }

                EnsureAudioImportSettings(path);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null)
                {
                    Debug.LogWarning($"[AudioImporter] Clip not found at {path} — skipping {id}");
                    fail++;
                    continue;
                }

                bool assigned = false;

                if (id.StartsWith("drone_", StringComparison.OrdinalIgnoreCase))
                    assigned = AssignDroneNote(id, clip);
                else if (id.StartsWith("sfx_", StringComparison.OrdinalIgnoreCase))
                    assigned = AssignSfxClip(id, clip);
                else
                {
                    // motif_* or unknown — just ensure it's imported
                    Debug.Log($"[AudioImporter] {id}: imported (no SO target)");
                    skip++;
                    continue;
                }

                if (assigned) ok++; else fail++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[AudioImporter] Done — ✓ {ok}  skipped {skip}  failed {fail}");
        }

        // ── Drone → LensDefinition.ambientNote ───────────────────────────────────

        private static bool AssignDroneNote(string id, AudioClip clip)
        {
            if (!DroneToLens.TryGetValue(id, out string soName))
            {
                Debug.LogWarning($"[AudioImporter] No lens mapping for drone id '{id}'");
                return false;
            }

            string soPath = $"{LensesFolder}/{soName}.asset";
            var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(soPath);
            if (so == null)
            {
                Debug.LogWarning($"[AudioImporter] LensDefinition SO not found: {soPath}");
                return false;
            }

            var ser   = new SerializedObject(so);
            var field = ser.FindProperty("ambientNote");
            if (field == null)
            {
                Debug.LogWarning($"[AudioImporter] '{soPath}' has no 'ambientNote' field.");
                return false;
            }

            field.objectReferenceValue = clip;
            ser.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(so);
            Debug.Log($"[AudioImporter] {id} → {soPath} .ambientNote");
            return true;
        }

        // ── SFX → AudioManagerPrefab ──────────────────────────────────────────────

        private static bool AssignSfxClip(string id, AudioClip clip)
        {
            if (!SfxFieldMap.TryGetValue(id, out string fieldName))
            {
                Debug.LogWarning($"[AudioImporter] No AudioManager field mapping for '{id}'");
                return false;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[AudioImporter] Prefab not found: {PrefabPath}");
                return false;
            }

            // Work on the prefab via SerializedObject to persist changes
            var am = prefab.GetComponent<global::ArchiveOfEchoes.AudioManager>();
            if (am == null)
            {
                Debug.LogWarning($"[AudioImporter] AudioManager component not found on {PrefabPath}");
                return false;
            }

            var ser   = new SerializedObject(am);
            var field = ser.FindProperty(fieldName);
            if (field == null)
            {
                Debug.LogWarning($"[AudioImporter] Field '{fieldName}' not found on AudioManager");
                return false;
            }

            field.objectReferenceValue = clip;
            ser.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(prefab);
            PrefabUtility.SavePrefabAsset(prefab);
            Debug.Log($"[AudioImporter] {id} → AudioManagerPrefab.{fieldName}");
            return true;
        }

        // ── Audio import settings ─────────────────────────────────────────────────

        private static void EnsureAudioImportSettings(string assetPath)
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(assetPath) as AudioImporter;
            if (importer == null) return;

            bool changed = false;
            if (importer.forceToMono != true)  { importer.forceToMono  = true;  changed = true; }

            var settings = importer.defaultSampleSettings;
            if (settings.loadType != AudioClipLoadType.DecompressOnLoad)
            {
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                changed = true;
            }
            if (settings.compressionFormat != AudioCompressionFormat.Vorbis)
            {
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                changed = true;
            }
            if (changed)
            {
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
            }
        }

        // ── Minimal JSON parser ───────────────────────────────────────────────────
        // Handles flat { "key": "value" } JSON produced by generate_audio.py.

        private static Dictionary<string, string> ParseManifest(string json)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var rx     = new Regex(@"""([^""]+)""\s*:\s*""([^""]+)""");
            foreach (Match m in rx.Matches(json))
                result[m.Groups[1].Value] = m.Groups[2].Value;
            return result;
        }
    }
}
