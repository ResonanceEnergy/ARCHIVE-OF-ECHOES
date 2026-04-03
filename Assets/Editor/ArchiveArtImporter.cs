using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArchiveOfEchoes.Editor
{
    /// <summary>
    /// Reads Assets/Art/art_manifest.json (written by generate_art.py) and
    /// assigns each generated texture to the matching ScriptableObject field:
    ///
    ///   Panel ID  p00_* / p01_*  →  PanelData.panelArtwork
    ///                               (p00_p2_corrupted also → corruptedOverlay)
    ///   lens_*                  →  LensDefinition.lensIcon
    ///   key_*                   →  KnowledgeKeyData.icon
    ///   cover_issue_00/01       →  IssueData.coverArt
    ///   ui_*                    →  stored in Art/UI; no SO assignment needed
    ///
    /// Run via:  Tools → Archive of Echoes → 5 – Import Art Assets
    /// or call ArchiveArtImporter.ImportAll() from ArchiveBootstrapper.
    /// </summary>
    public static class ArchiveArtImporter
    {
        private const string ManifestPath = "Assets/Art/art_manifest.json";

        // ── Entry ─────────────────────────────────────────────────────────────────

        public static void ImportAll()
        {
            if (!File.Exists(ManifestPath))
            {
                Debug.LogWarning(
                    $"[ArtImporter] Manifest not found at {ManifestPath}. " +
                    "Run generate_art.py first, then reimport to Unity.");
                return;
            }

            string json = File.ReadAllText(ManifestPath);
            var manifest = ParseManifest(json);
            if (manifest == null || manifest.Count == 0)
            {
                Debug.LogWarning("[ArtImporter] Manifest is empty or could not be parsed.");
                return;
            }

            // Force Unity to recognise any newly written PNG files
            AssetDatabase.Refresh();

            int assigned = 0;
            int skipped  = 0;

            foreach (var kv in manifest)
            {
                string assetId   = kv.Key;
                string imagePath = kv.Value;   // e.g. "Assets/Art/Panels/Issue00/p00_p1_exterior.png"

                // Ensure import settings are set to Sprite before loading
                EnsureSpriteImport(imagePath);

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
                if (sprite == null)
                {
                    Debug.LogWarning($"[ArtImporter] Sprite not found at {imagePath} — skipping {assetId}");
                    skipped++;
                    continue;
                }

                bool ok = AssignSprite(assetId, sprite);
                if (ok) assigned++;
                else    skipped++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ArtImporter] Done — {assigned} assigned, {skipped} skipped / not matched.");
        }

        // ── Assignment routing ────────────────────────────────────────────────────

        private static bool AssignSprite(string assetId, Sprite sprite)
        {
            // Panel — Issue 00/01 short prefix: p00_*, p01_*
            if (assetId.StartsWith("p00_") || assetId.StartsWith("p01_"))
                return AssignPanelSprite(assetId, sprite);

            // Panel — Issue 02-12 long prefix: i02_*, i03_*, … i12_*
            // Pattern: i{NN}_p{page}_{name}
            if (assetId.Length >= 4 && assetId[0] == 'i' && char.IsDigit(assetId[1]) && char.IsDigit(assetId[2]) && assetId[3] == '_')
                return AssignIssuePanelSprite(assetId, sprite);

            // Lens icon: lens_mythic → Lens_Mythic.asset
            if (assetId.StartsWith("lens_"))
                return AssignLens(assetId, sprite);

            // Knowledge key icon: key_sequence → Key_SEQUENCE.asset
            if (assetId.StartsWith("key_"))
                return AssignKey(assetId, sprite);

            // Issue cover: cover_issue_00 / cover_issue_01
            if (assetId.StartsWith("cover_issue_"))
                return AssignCover(assetId, sprite);

            // UI assets — no SO target, just image in Art/UI folder (no-op)
            if (assetId.StartsWith("ui_"))
            {
                Debug.Log($"[ArtImporter] UI asset '{assetId}' imported (no SO target).");
                return true;
            }

            Debug.LogWarning($"[ArtImporter] Unknown asset id pattern: {assetId}");
            return false;
        }

        // ── Panel ─────────────────────────────────────────────────────────────────

        private static bool AssignPanelSprite(string assetId, Sprite sprite)
        {
            // Determine issue folder from prefix
            string issueFolder = assetId.StartsWith("p00_") ? "Issue00" : "Issue01";
            string soPath      = $"Assets/ScriptableObjects/Panels/{issueFolder}/{assetId}.asset";

            var panel = AssetDatabase.LoadAssetAtPath<PanelData>(soPath);
            if (panel == null)
            {
                Debug.LogWarning($"[ArtImporter] PanelData SO not found: {soPath}");
                return false;
            }

            var so = new SerializedObject(panel);

            // panelArtwork is always set
            SetObjectRef(so, "panelArtwork", sprite);

            // The corrupted panel's artwork doubles as corruptedOverlay
            // until a clean resolved version is available
            if (assetId.EndsWith("_corrupted"))
                SetObjectRef(so, "corruptedOverlay", sprite);

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(panel);
            Debug.Log($"[ArtImporter] ✓ Panel {assetId}");
            return true;
        }

        // ── Lens ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Routes i{NN}_* art IDs (Issues 02–12) to their PanelData SO.
        /// Pattern: i02_p3_edin → Assets/ScriptableObjects/Panels/Issue02/i02_p3_edin.asset
        /// Logs at Debug level (not Warning) when the SO does not yet exist —
        /// these are expected for issues whose data isn't yet built.
        /// </summary>
        private static bool AssignIssuePanelSprite(string assetId, Sprite sprite)
        {
            // Extract the two-digit issue number from "i{NN}_..."
            string issueNumStr  = assetId.Substring(1, 2);   // "02", "03", …
            string issueFolder  = $"Issue{issueNumStr}";
            string soPath       = $"Assets/ScriptableObjects/Panels/{issueFolder}/{assetId}.asset";

            var panel = AssetDatabase.LoadAssetAtPath<PanelData>(soPath);
            if (panel == null)
            {
                // Not a warning — SOs for later issues are built on demand.
                Debug.Log($"[ArtImporter] {assetId}: no SO at {soPath} (skipped).");
                return false;
            }

            var so = new SerializedObject(panel);
            SetObjectRef(so, "panelArtwork", sprite);

            if (assetId.EndsWith("_corrupted"))
                SetObjectRef(so, "corruptedOverlay", sprite);

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(panel);
            Debug.Log($"[ArtImporter] ✓ Panel {assetId}");
            return true;
        }

        private static bool AssignLens(string assetId, Sprite sprite)
        {
            // "lens_mythic" → "Mythic" → "Lens_Mythic.asset"
            string typeName = Capitalise(assetId.Substring("lens_".Length));
            string soPath   = $"Assets/ScriptableObjects/Lenses/Lens_{typeName}.asset";

            var def = AssetDatabase.LoadAssetAtPath<LensDefinition>(soPath);
            if (def == null)
            {
                Debug.LogWarning($"[ArtImporter] LensDefinition SO not found: {soPath}");
                return false;
            }

            var so = new SerializedObject(def);
            SetObjectRef(so, "lensIcon", sprite);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(def);
            Debug.Log($"[ArtImporter] ✓ Lens {assetId}");
            return true;
        }

        // ── Knowledge key ─────────────────────────────────────────────────────────

        private static bool AssignKey(string assetId, Sprite sprite)
        {
            // "key_sequence" → "SEQUENCE" → "Key_SEQUENCE.asset"
            string keyId  = assetId.Substring("key_".Length).ToUpperInvariant();
            string soPath = $"Assets/ScriptableObjects/Keys/Key_{keyId}.asset";

            var key = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(soPath);
            if (key == null)
            {
                Debug.LogWarning($"[ArtImporter] KnowledgeKeyData SO not found: {soPath}");
                return false;
            }

            var so = new SerializedObject(key);
            SetObjectRef(so, "icon", sprite);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(key);
            Debug.Log($"[ArtImporter] ✓ Key {assetId}");
            return true;
        }

        // ── Issue cover ───────────────────────────────────────────────────────────

        private static bool AssignCover(string assetId, Sprite sprite)
        {
            // "cover_issue_00" → "Issue_00.asset"
            // Strip "cover_" prefix → "issue_00" → title-case → "Issue_00"
            string suffix = assetId.Substring("cover_".Length);            // "issue_00"
            string soName = Capitalise(suffix.Replace("issue_", "Issue_")); // "Issue_00"
            string soPath = $"Assets/ScriptableObjects/Issues/{soName}.asset";

            var issue = AssetDatabase.LoadAssetAtPath<IssueData>(soPath);
            if (issue == null)
            {
                Debug.LogWarning($"[ArtImporter] IssueData SO not found: {soPath}");
                return false;
            }

            var so = new SerializedObject(issue);
            SetObjectRef(so, "coverArt", sprite);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(issue);
            Debug.Log($"[ArtImporter] ✓ Cover {assetId}");
            return true;
        }

        // ── Sprite import settings ────────────────────────────────────────────────

        private static void EnsureSpriteImport(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;  // not yet imported or wrong type

            bool dirty = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                dirty = true;
            }
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                dirty = true;
            }
            if (importer.maxTextureSize < 2048)
            {
                importer.maxTextureSize = 2048;
                dirty = true;
            }
            if (dirty)
            {
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }

        // ── Utilities ─────────────────────────────────────────────────────────────

        private static void SetObjectRef(SerializedObject so, string field, Object value)
        {
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogWarning($"[ArtImporter] Field '{field}' not found on {so.targetObject.name}");
                return;
            }
            prop.objectReferenceValue = value;
        }

        /// <summary>Capitalise first letter, leave rest unchanged.</summary>
        private static string Capitalise(string s) =>
            string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);

        /// <summary>Minimal JSON object parser (no external deps).</summary>
        private static Dictionary<string, string> ParseManifest(string json)
        {
            var dict = new Dictionary<string, string>();
            // Strip outer braces
            json = json.Trim().TrimStart('{').TrimEnd('}');
            foreach (var entry in json.Split(','))
            {
                var parts = entry.Split(new[] { ':' }, 2);
                if (parts.Length != 2) continue;
                string key   = parts[0].Trim().Trim('"');
                string value = parts[1].Trim().Trim('"');
                if (!string.IsNullOrEmpty(key))
                    dict[key] = value;
            }
            return dict;
        }
    }
}
