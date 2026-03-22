using UnityEditor;
using UnityEngine;

namespace ArchiveOfEchoes.Editor
{
    /// <summary>
    /// Generates all ScriptableObject data assets for Issues 00 and 01 at edit time.
    ///
    /// Produced assets:
    ///   Assets/ScriptableObjects/Lenses/     — 5 LensDefinition SOs
    ///   Assets/ScriptableObjects/Keys/       — KnowledgeKeyData SOs
    ///   Assets/ScriptableObjects/Panels/     — PanelData SOs, one per narrative beat
    ///   Assets/ScriptableObjects/Pages/      — PageData SOs
    ///   Assets/ScriptableObjects/Issues/     — IssueData SOs
    ///
    /// All text fields are pre-populated from the GDD / issue storyboards.
    /// Sprite / AudioClip references are left null — assign in the Inspector after
    /// importing art and audio assets.
    /// </summary>
    public static class ArchiveDataBuilder
    {
        // ── Entry ─────────────────────────────────────────────────────────────────

        public static void BuildDataAssets()
        {
            BuildLensDefinitions();
            BuildKnowledgeKeys();
            BuildIssue00();
            BuildIssue01();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Archive] Data assets built.");
        }

        // ── Lens definitions ──────────────────────────────────────────────────────

        private static void BuildLensDefinitions()
        {
            string dir = "Assets/ScriptableObjects/Lenses";

            MakeLens(dir, LensType.Mythic,      "Mythic",      new Color(0.82f, 0.65f, 0.20f),
                "Gods, rituals, cosmic beings. The oldest reading of all things.");
            MakeLens(dir, LensType.Technologic,  "Technologic",  new Color(0.25f, 0.70f, 0.90f),
                "Machines, circuits, science. Sacred objects become schematics.");
            MakeLens(dir, LensType.Symbolic,    "Symbolic",    new Color(0.60f, 0.30f, 0.90f),
                "Psychology, archetypes. Characters are aspects of mind.");
            MakeLens(dir, LensType.Political,   "Political",   new Color(0.85f, 0.20f, 0.20f),
                "Power, control, bloodlines. Hierarchy made visible.");
            MakeLens(dir, LensType.Spiritual,   "Spiritual",   new Color(0.40f, 0.90f, 0.60f),
                "Consciousness, ascension. Hidden layers appear; the Gutter speaks.");
        }

        private static void MakeLens(string dir, LensType type, string displayName,
                                     Color color, string flavour)
        {
            var path = $"{dir}/Lens_{type}.asset";
            var def  = LoadOrCreate<LensDefinition>(path);
            def.lensType          = type;
            def.displayName       = displayName;
            def.lensColor         = color;
            def.unlockFlavourText = flavour;
            EditorUtility.SetDirty(def);
        }

        // ── Knowledge keys ────────────────────────────────────────────────────────

        private static void BuildKnowledgeKeys()
        {
            string dir = "Assets/ScriptableObjects/Keys";

            MakeKey(dir, "SEQUENCE",    "Sequence",
                "Panels have a correct direction. So do ages.",
                new[] { LensType.Mythic, LensType.Symbolic }, false);

            MakeKey(dir, "ALIGNMENT",   "Alignment Key",
                "Cardinal geometry locks the shaft. Turn the stars.",
                new[] { LensType.Technologic, LensType.Mythic }, false);

            MakeKey(dir, "MIRROR",      "Mirror Key",
                "As above, so below. The city repeats itself endlessly.",
                new[] { LensType.Symbolic, LensType.Political }, false);

            MakeKey(dir, "TRANSLATION", "Translation Key",
                "Glyphs evolve. Read the chain and the meaning is clear.",
                new[] { LensType.Mythic, LensType.Symbolic }, false);

            MakeKey(dir, "STABILITY",   "Stability Key",
                "The Djed pillar stands. The circuit holds.",
                new[] { LensType.Technologic, LensType.Spiritual }, true);

            MakeKey(dir, "ARTIFACT",    "Artifact Key",
                "All four components, assembled at last.",
                new[] { LensType.Mythic, LensType.Spiritual }, true);
        }

        private static KnowledgeKeyData MakeKey(string dir, string keyId, string displayName,
                                                string description, LensType[] lenses, bool required)
        {
            var path = $"{dir}/Key_{keyId}.asset";
            var key  = LoadOrCreate<KnowledgeKeyData>(path);
            key.keyId          = keyId;
            key.displayName    = displayName;
            key.description    = description;
            key.relevantLenses = lenses;
            key.isRequiredForT5 = required;
            EditorUtility.SetDirty(key);
            return key;
        }

        // ── Issue 00 — "The Find" ─────────────────────────────────────────────────

        private static void BuildIssue00()
        {
            string panelDir = "Assets/ScriptableObjects/Panels/Issue00";
            string pageDir  = "Assets/ScriptableObjects/Pages/Issue00";

            // ── Page 1 — "The Assignment" ─────────────────────────────────────────
            var p00_p1_exterior = MakePanel(panelDir, "p00_p1_exterior", PanelType.Static,
                caption: "Sector 7-D. Demolition scheduled: 72 hours.");

            var p00_p1_interior = MakePanel(panelDir, "p00_p1_interior", PanelType.Static,
                caption: "Personal assignment: catalog and clear.");

            var p00_p1_hands = MakePanel(panelDir, "p00_p1_hands", PanelType.Static);

            var p00_p1_slot = MakePanel(panelDir, "p00_p1_slot", PanelType.Static,
                caption: "Almost all.");

            var p00_p1_taptutorial = MakePanel(panelDir, "p00_p1_taptutorial", PanelType.Interact,
                caption: "Tap to inspect.",
                archivistNote:
                    "Unlisted objects become property of the Archive-at-large. " +
                    "Standard procedure: log and contain.");

            var page00_1 = MakePage(pageDir, "page_00_1", PageLayout.Strip,
                new[] { p00_p1_exterior, p00_p1_interior, p00_p1_hands, p00_p1_slot, p00_p1_taptutorial });

            // ── Page 2 — "The Object" ─────────────────────────────────────────────
            var p00_p2_comic = MakePanel(panelDir, "p00_p2_comic", PanelType.Static);

            var p00_p2_corrupted = MakePanel(panelDir, "p00_p2_corrupted", PanelType.Stabilize,
                caption: "Something is wrong with this panel. Long-press to stabilize.",
                gutter: "Rules are for things that stay where you put them.",
                archivistNote: "Stabilization complete. Anomaly: item resists standard cataloging format.",
                startsCorrupted: true, corruptionLevel: 0.8f,
                puzzleStabilizeDuration: 2f);

            var p00_p2_eyes = MakePanel(panelDir, "p00_p2_eyes", PanelType.Static,
                gutter: "Contained? Sure. If that helps you sleep.");

            var page00_2 = MakePage(pageDir, "page_00_2", PageLayout.Strip,
                new[] { p00_p2_comic, p00_p2_corrupted, p00_p2_eyes });

            // ── Page 3 — "The Pull" ───────────────────────────────────────────────
            var p00_p3_opens = MakePanel(panelDir, "p00_p3_opens", PanelType.Static);

            var p00_p3_pinch = MakePanel(panelDir, "p00_p3_pinch", PanelType.Interact,
                caption: "This panel wants you inside. Pinch to enter.");

            var p00_p3_splash = MakePanel(panelDir, "p00_p3_splash", PanelType.Static,
                caption: "You were supposed to clear this sector.",
                gutter: "You were supposed to leave.");

            var page00_3 = MakePage(pageDir, "page_00_3", PageLayout.FullBleed,
                new[] { p00_p3_opens, p00_p3_pinch, p00_p3_splash },
                entryTransition: PageTransition.InkDive,
                isFullBleed: true);

            // ── Issue 00 SO ───────────────────────────────────────────────────────
            var issue00 = LoadOrCreate<IssueData>("Assets/ScriptableObjects/Issues/Issue_00.asset");
            issue00.issueId     = "issue_00";
            issue00.issueNumber = 0;
            issue00.title       = "The Find";
            issue00.arc         = "2100 Frame — Prologue";
            issue00.pages       = new[] { page00_1, page00_2, page00_3 };
            issue00.prerequisiteIssueIds = new string[0];
            issue00.unlocksLenses        = new LensType[0];
            issue00.unlocksKeys          = new KnowledgeKeyData[0];
            EditorUtility.SetDirty(issue00);

            Debug.Log("[Archive] Issue 00 data built.");
        }

        // ── Issue 01 — "Broken Page" ──────────────────────────────────────────────

        private static void BuildIssue01()
        {
            string panelDir = "Assets/ScriptableObjects/Panels/Issue01";
            string pageDir  = "Assets/ScriptableObjects/Pages/Issue01";

            var keySequence = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_SEQUENCE.asset");

            // ── Page 1 — "The Gutter Between" ────────────────────────────────────
            var p01_p1_dark = MakePanel(panelDir, "p01_p1_darkness", PanelType.Static,
                caption: "Where are you?");

            var p01_p1_frags = MakePanel(panelDir, "p01_p1_fragments", PanelType.Static);

            var p01_p1_voice = MakePanel(panelDir, "p01_p1_voice", PanelType.Static,
                archivistNote:
                    "Record 01 — Panel-Space is real. I theorized it. " +
                    "A zone of potential between fixed narrative states. " +
                    "The page can be any order until an observer fixes it. " +
                    "Welcome to the experiment.");

            var p01_p1_frag = MakePanel(panelDir, "p01_p1_fragment", PanelType.Interact,
                caption: "This page is wrong. It was right once.",
                gutter: "This page has been broken. Can you fix it?");

            var page01_1 = MakePage(pageDir, "page_01_1", PageLayout.FourPanel,
                new[] { p01_p1_dark, p01_p1_frags, p01_p1_voice, p01_p1_frag });

            // ── Page 2 — "Reorder" (A1 Panel Reorder puzzle) ─────────────────────
            var p01_p2_reorder = MakePanel(panelDir, "p01_p2_reorder", PanelType.Reorder,
                caption: "Drag the panels into their correct order.",
                revealsKeys: keySequence != null ? new[] { keySequence } : new KnowledgeKeyData[0],
                puzzleReorderCount: 5);

            var page01_2 = MakePage(pageDir, "page_01_2", PageLayout.FullBleed,
                new[] { p01_p2_reorder },
                isFullBleed: true);

            // ── Page 3 — "The Gutter Speaks" ─────────────────────────────────────
            var p01_p3_restored = MakePanel(panelDir, "p01_p3_restored", PanelType.Static,
                archivistNote:
                    "That page was a memory. Someone's memory. " +
                    "You ordered it. Now you're part of it.");

            var p01_p3_solid = MakePanel(panelDir, "p01_p3_solid", PanelType.Static,
                caption: "There are many ways forward.");

            var p01_p3_gutter = MakePanel(panelDir, "p01_p3_gutterdoor", PanelType.Gutter,
                caption: "Something hides between panels. Swipe along the gutter to feel for it.",
                gutter: "They put me here to lead you away. I've decided to lead you further in instead.");

            var p01_p3_vision = MakePanel(panelDir, "p01_p3_pyramid", PanelType.Static,
                gutter: "PYRAMID");

            var page01_3 = MakePage(pageDir, "page_01_3", PageLayout.Strip,
                new[] { p01_p3_restored, p01_p3_solid, p01_p3_gutter, p01_p3_vision },
                entryTransition: PageTransition.GutterEntity);

            // ── Page 4 — "The First Branch" ───────────────────────────────────────
            var p01_p4_t1 = MakePanel(panelDir, "p01_p4_t1", PanelType.Interact,
                caption: "T1 thread: Origins. The beginning of the beginning. Oldest path. Most complete.",
                archivistNote: "There is no wrong branch. The Archive observes all paths.",
                isBranchPoint: true,
                branchOptions: new[]
                {
                    new BranchOption
                    {
                        label         = "T1 Origins",
                        targetIssueId = "issue_02",
                        targetPageIndex = 0
                    }
                });

            var p01_p4_t2 = MakePanel(panelDir, "p01_p4_t2", PanelType.Interact,
                caption: "T2 thread: Cities. The beginning of civilisation. A later entry point. Less stable.",
                isBranchPoint: true,
                branchOptions: new[]
                {
                    new BranchOption
                    {
                        label         = "T2 Cities",
                        targetIssueId = "issue_02",
                        targetPageIndex = 0
                    }
                });

            var page01_4 = MakePage(pageDir, "page_01_4", PageLayout.TwoPanel,
                new[] { p01_p4_t1, p01_p4_t2 });

            // ── Page 5 — "Into the Thread" (full bleed splash) ────────────────────
            var p01_p5 = MakePanel(panelDir, "p01_p5_thread", PanelType.Static,
                caption: "The archive opens.",
                gutter: "You can't read both at once. You can only live one at a time. But you'll be back.");

            var page01_5 = MakePage(pageDir, "page_01_5", PageLayout.FullBleed,
                new[] { p01_p5 },
                entryTransition: PageTransition.InkDive,
                isFullBleed: true);

            // ── Issue 01 SO ───────────────────────────────────────────────────────
            var issue01 = LoadOrCreate<IssueData>("Assets/ScriptableObjects/Issues/Issue_01.asset");
            issue01.issueId              = "issue_01";
            issue01.issueNumber          = 1;
            issue01.title                = "Broken Page";
            issue01.arc                  = "Tutorial Completion — Transition Shard";
            issue01.pages                = new[] { page01_1, page01_2, page01_3, page01_4, page01_5 };
            issue01.prerequisiteIssueIds = new[] { "issue_00" };
            issue01.unlocksLenses        = new LensType[0];
            issue01.unlocksKeys          = keySequence != null ? new[] { keySequence } : new KnowledgeKeyData[0];
            EditorUtility.SetDirty(issue01);

            Debug.Log("[Archive] Issue 01 data built.");
        }

        // ── Panel / Page factory helpers ──────────────────────────────────────────

        private static PanelData MakePanel(
            string dir,
            string id,
            PanelType type,
            string caption          = "",
            string gutter           = "",
            string archivistNote    = "",
            bool   startsCorrupted  = false,
            float  corruptionLevel  = 0f,
            float  puzzleStabilizeDuration = 2f,
            int    puzzleReorderCount      = 4,
            KnowledgeKeyData[] revealsKeys = null,
            bool   isBranchPoint    = false,
            BranchOption[] branchOptions = null)
        {
            var path  = $"{dir}/Panel_{id}.asset";
            var panel = LoadOrCreate<PanelData>(path);

            panel.panelId        = id;
            panel.panelType      = type;
            panel.captionText    = caption;
            panel.gutterText     = gutter;
            panel.archivistNote  = archivistNote;
            panel.startsCorrupted = startsCorrupted;
            panel.corruptionLevel = Mathf.Clamp01(corruptionLevel);
            panel.isBranchPoint  = isBranchPoint;
            panel.branchOptions  = branchOptions ?? new BranchOption[0];
            panel.revealsKeys    = revealsKeys   ?? new KnowledgeKeyData[0];

            if (type == PanelType.Stabilize || type == PanelType.Reorder)
            {
                panel.puzzleConfig = new PuzzleConfig
                {
                    puzzleType            = type,
                    stabilizeDuration     = puzzleStabilizeDuration,
                    reorderPanelCount     = puzzleReorderCount,
                    isOptional            = false
                };
            }

            EditorUtility.SetDirty(panel);
            return panel;
        }

        private static PageData MakePage(
            string dir,
            string id,
            PageLayout layout,
            PanelData[] panels,
            PageTransition entryTransition = PageTransition.PageTurn,
            bool isFullBleed = false)
        {
            var path = $"{dir}/Page_{id}.asset";
            var page = LoadOrCreate<PageData>(path);

            page.pageId          = id;
            page.layout          = layout;
            page.panels          = panels;
            page.entryTransition = entryTransition;
            page.isFullBleed     = isFullBleed;

            EditorUtility.SetDirty(page);
            return page;
        }

        // ── Asset I/O ─────────────────────────────────────────────────────────────

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;

            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
