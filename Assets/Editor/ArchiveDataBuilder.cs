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
            BuildIssue02();
            BuildIssue03();
            BuildIssue04();
            BuildIssue05();
            BuildIssue06();
            BuildIssue07();
            BuildIssue08();
            BuildIssue09();
            BuildIssue10();

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

            // Phase 2 keys ────────────────────────────────────────────────────
            MakeKey(dir, "MANDATE",        "Mandate Key",
                "The Council Decree: a civilisation built on a plan.",
                new[] { LensType.Mythic, LensType.Political }, false);

            MakeKey(dir, "CUSTODIAN",      "Custodian Key",
                "Isis holds the garden. The garden holds the record.",
                new[] { LensType.Mythic, LensType.Spiritual }, false);

            MakeKey(dir, "MODIFICATION",   "Modification Key",
                "Two strands where there should be more. The Limiter.",
                new[] { LensType.Technologic, LensType.Political }, false);

            MakeKey(dir, "GRID",           "Grid Key",
                "The blueprint existed before the first brick was laid.",
                new[] { LensType.Technologic, LensType.Symbolic }, false);

            MakeKey(dir, "CORRESPONDENCE", "Correspondence Key",
                "Two cities, one grid. The pattern repeats at every scale.",
                new[] { LensType.Symbolic, LensType.Technologic }, false);

            MakeKey(dir, "RESONANCE",      "Resonance Key",
                "A third city. A third argument. The myth that became a warning.",
                new[] { LensType.Mythic, LensType.Symbolic }, false);

            MakeKey(dir, "RESTORATION",    "Restoration Key",
                "The record was defaced. You are restoring it.",
                new[] { LensType.Political, LensType.Symbolic }, false);

            MakeKey(dir, "PASSAGE",        "Passage Key",
                "The geometry of the shaft points to a specific star. Follow the angle.",
                new[] { LensType.Technologic, LensType.Spiritual }, false);

            MakeKey(dir, "DESCENT",        "Descent Key",
                "The alternate shaft leads down before it leads forward.",
                new[] { LensType.Spiritual }, false);

            MakeKey(dir, "REVELATION",     "Revelation Key",
                "Some objects only appear under both lenses simultaneously.",
                new[] { LensType.Spiritual, LensType.Technologic }, true);

            MakeKey(dir, "BLUEPRINT",      "Blueprint Key",
                "Every measurement corresponds to a specification. It was received, not invented.",
                new[] { LensType.Technologic, LensType.Mythic }, true);

            // Phase 3 keys ────────────────────────────────────────────────────
            MakeKey(dir, "THE_FIVE",    "The Five Key",
                "Seal Burn, Scan, Circuit Link, Gutter Step, Capstone Lock. The Ark is an instrument.",
                new[] { LensType.Technologic, LensType.Spiritual }, false);

            MakeKey(dir, "CENTER",      "Center Key",
                "T5: The Missing Center. Not forgotten — placed. Everything else was built around it.",
                new[] { LensType.Mythic, LensType.Spiritual }, true);

            MakeKey(dir, "FOUNDATION",  "Foundation Key",
                "The subterranean chamber precedes the structure above it.",
                new[] { LensType.Technologic, LensType.Mythic }, false);

            MakeKey(dir, "ALIGNED",     "Aligned Key",
                "All four shafts point to their stellar targets. The chamber was built for this moment.",
                new[] { LensType.Technologic, LensType.Symbolic }, false);

            MakeKey(dir, "SAME",        "Same Key",
                "Two stories, one person. The exile carried what they sought. The carrier was the Ark.",
                new[] { LensType.Symbolic, LensType.Political }, false);

            MakeKey(dir, "COMPLETE_INSTRUMENT", "Complete Instrument Key",
                "All four components assembled. All five abilities active. The Ark is whole.",
                new[] { LensType.Mythic, LensType.Spiritual, LensType.Technologic }, true);

            MakeKey(dir, "CARRIED",     "Carried Key",
                "The Ark must be carried. Two poles. Two hands. One destination.",
                new[] { LensType.Spiritual, LensType.Political }, false);
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

        // ── Issue 02 — "Origins Thread" ───────────────────────────────────────────

        private static void BuildIssue02()
        {
            string panelDir = "Assets/ScriptableObjects/Panels/Issue02";
            string pageDir  = "Assets/ScriptableObjects/Pages/Issue02";

            var keyMandate      = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_MANDATE.asset");
            var keyCustodian    = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_CUSTODIAN.asset");
            var keyModification = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_MODIFICATION.asset");

            // ── Page 1 — "T1-A Entry" ─────────────────────────────────────────
            var p02_p1_splash = MakePanel(panelDir, "p02_p1_splash", PanelType.Static,
                caption: "T1-A: Origins. Before the first word was written.",
                gutter: "They did not arrive. They returned.");

            var p02_p1_approach = MakePanel(panelDir, "p02_p1_approach", PanelType.Static,
                caption: "The craft: older than the word for sky.");

            var p02_p1_arrival = MakePanel(panelDir, "p02_p1_arrival", PanelType.Static,
                caption: "Eridu. The first city. The first decision.",
                archivistNote:
                    "Record T1-A — Sumerian king list: 'Before the flood, kingship descended from heaven.' " +
                    "The Archive treats this as a technical description, not a metaphor.");

            var page02_1 = MakePage(pageDir, "page_02_1", PageLayout.Strip,
                new[] { p02_p1_splash, p02_p1_approach, p02_p1_arrival },
                entryTransition: PageTransition.InkDive);

            // ── Page 2 — "Council Decree" (Puzzle: D2 Reorder) ───────────────
            var p02_p2_decree = MakePanel(panelDir, "p02_p2_decree", PanelType.Reorder,
                caption: "The decree has five components. They belong in order.",
                gutter: "Law is sequence. Sequence is control. Control is the first technology.",
                archivistNote: "Assembled in correct order: cause, effect, rule, duration, consequence.",
                revealsKeys: keyMandate != null ? new[] { keyMandate } : new KnowledgeKeyData[0],
                puzzleReorderCount: 5);

            var page02_2 = MakePage(pageDir, "page_02_2", PageLayout.FullBleed,
                new[] { p02_p2_decree },
                isFullBleed: true);

            // ── Page 3 — "E.D.I.N." (Stabilize: B1) ─────────────────────────
            var p02_p3_edin = MakePanel(panelDir, "p02_p3_edin", PanelType.Static,
                caption: "E.D.I.N.: the garden enclosure. Managed. Intentional. Perfect.");

            var p02_p3_isis = MakePanel(panelDir, "p02_p3_isis", PanelType.Static,
                caption: "She tends the record, not the garden. The garden is the record.",
                revealsKeys: keyCustodian != null ? new[] { keyCustodian } : new KnowledgeKeyData[0]);

            var p02_p3_corrupt = MakePanel(panelDir, "p02_p3_corrupt", PanelType.Stabilize,
                caption: "This panel is destabilising. Hold to restore the E.D.I.N. grid.",
                gutter: "Some memories resist being seen clearly.",
                startsCorrupted: true, corruptionLevel: 0.7f,
                puzzleStabilizeDuration: 2.5f);

            var p02_p3_restored = MakePanel(panelDir, "p02_p3_restored", PanelType.Static,
                caption: "The enclosure is recorded. The custodian is named.",
                archivistNote:
                    "If the E.D.I.N. is a managed habitat, " +
                    "then the expulsion is a change in administrative access.");

            var page02_3 = MakePage(pageDir, "page_02_3", PageLayout.FourPanel,
                new[] { p02_p3_edin, p02_p3_isis, p02_p3_corrupt, p02_p3_restored });

            // ── Page 4 — "The Bloodline" (A2 Reorder) ────────────────────────
            var p02_p4_adamu = MakePanel(panelDir, "p02_p4_adamu", PanelType.Static,
                caption: "The first designed recipient. The bloodline begins with a specification.");

            var p02_p4_rotate = MakePanel(panelDir, "p02_p4_rotate", PanelType.Reorder,
                caption: "The lineage panels are out of sequence. Restore the correct descent order.",
                archivistNote:
                    "The Adamic lineage in the Sumerian record is a list of reigns, not births. " +
                    "The durations suggest something else entirely.",
                puzzleReorderCount: 4);

            var p02_p4_djed1 = MakePanel(panelDir, "p02_p4_djed1", PanelType.Static,
                caption: "Bar 1 of 4. The pillar is incomplete. This is the first section.",
                gutter: "They built it in pieces. That was intentional.");

            var page02_4 = MakePage(pageDir, "page_02_4", PageLayout.Strip,
                new[] { p02_p4_adamu, p02_p4_rotate, p02_p4_djed1 });

            // ── Page 5 — "Hidden Panel" ───────────────────────────────────────
            var p02_p5_surface = MakePanel(panelDir, "p02_p5_surface", PanelType.Static,
                caption: "The panel shows a standard genealogy. Nothing unusual.");

            var p02_p5_hidden = MakePanel(panelDir, "p02_p5_hidden", PanelType.Interact,
                caption: "Beneath this panel: something else. Hold to look closer.",
                archivistNote:
                    "Below-panel space: a partial diagram. Biological. " +
                    "The structure depicted does not match current documentation.");

            var p02_p5_reveal = MakePanel(panelDir, "p02_p5_reveal", PanelType.Static,
                caption: "A partial record. Deliberately incomplete.",
                gutter: "What you cannot read is the most important part.");

            var page02_5 = MakePage(pageDir, "page_02_5", PageLayout.Strip,
                new[] { p02_p5_surface, p02_p5_hidden, p02_p5_reveal });

            // ── Page 6 — "Lens Switch + Limiter" (D1 partial glyph) ──────────
            var p02_p6_lensswitch = MakePanel(panelDir, "p02_p6_lensswitch", PanelType.Interact,
                caption: "The Technologic Lens is now available. Some panels only resolve under its reading.",
                archivistNote:
                    "The Technologic Lens reads sacred objects as functional schematics. " +
                    "This lens may be unsettling. The Archive recommends it regardless.");

            var p02_p6_limiter = MakePanel(panelDir, "p02_p6_limiter", PanelType.Gutter,
                caption: "Under the Technologic Lens: the biological diagram resolves.",
                gutter: "Two strands. The specification calls for more. Someone reduced it. The reduction was deliberate.",
                archivistNote:
                    "D1 Glyph chain (partial): the glyph for MODIFICATION appears at the reduction point. " +
                    "Full chain deferred to Issue 04.",
                revealsKeys: keyModification != null ? new[] { keyModification } : new KnowledgeKeyData[0]);

            var page02_6 = MakePage(pageDir, "page_02_6", PageLayout.TwoPanel,
                new[] { p02_p6_lensswitch, p02_p6_limiter },
                entryTransition: PageTransition.GutterEntity);

            // ── Page 7 — "Thread Complete" ────────────────────────────────────
            var p02_p7_sky = MakePanel(panelDir, "p02_p7_sky", PanelType.Static,
                caption: "The custodian looks upward. So does the constellation map.");

            var p02_p7_djed = MakePanel(panelDir, "p02_p7_djed", PanelType.Static,
                caption: "Bar 1 of 4 is standing. Three remain.",
                gutter: "The Djed is a stability column. You will understand this later.");

            var p02_p7_constellation = MakePanel(panelDir, "p02_p7_constellation", PanelType.Static,
                caption: "T1 Origin thread: first nodes lit. The path continues to T2.",
                archivistNote:
                    "Issue 02 complete. Technologic Lens active. " +
                    "The Limiter is documented. The garden is recorded. The bloodline begins.");

            var page02_7 = MakePage(pageDir, "page_02_7", PageLayout.FullBleed,
                new[] { p02_p7_sky, p02_p7_djed, p02_p7_constellation },
                entryTransition: PageTransition.InkDive,
                isFullBleed: true);

            // ── Issue 02 SO ───────────────────────────────────────────────────
            var issue02 = LoadOrCreate<IssueData>("Assets/ScriptableObjects/Issues/Issue_02.asset");
            issue02.issueId              = "issue_02";
            issue02.issueNumber          = 2;
            issue02.title                = "Origins Thread";
            issue02.arc                  = "T1-A — The First Decision";
            issue02.pages                = new[]
            {
                page02_1, page02_2, page02_3, page02_4, page02_5, page02_6, page02_7
            };
            issue02.prerequisiteIssueIds = new[] { "issue_01" };
            issue02.unlocksLenses        = new[] { LensType.Technologic };
            issue02.unlocksKeys          = new KnowledgeKeyData[0];
            EditorUtility.SetDirty(issue02);

            Debug.Log("[Archive] Issue 02 data built.");
        }

        // ── Issue 03 — "Cities Thread" ────────────────────────────────────────────

        private static void BuildIssue03()
        {
            string panelDir = "Assets/ScriptableObjects/Panels/Issue03";
            string pageDir  = "Assets/ScriptableObjects/Pages/Issue03";

            var keyGrid           = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_GRID.asset");
            var keyCorrespondence = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_CORRESPONDENCE.asset");
            var keyResonance      = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_RESONANCE.asset");

            // ── Page 1 — "Before the City" (T2-A Entry) ──────────────────────
            var p03_p1_floodplain = MakePanel(panelDir, "p03_p1_floodplain", PanelType.Static,
                caption: "T2-A: The floodplain, after the water receded. Before anyone counted anything.",
                archivistNote:
                    "Record T2-A — The first cities appear simultaneously across geography. " +
                    "The Archive notes this does not match a diffusion model.");

            var p03_p1_survey = MakePanel(panelDir, "p03_p1_survey", PanelType.Static,
                caption: "They are measuring before they are building.");

            var p03_p1_gridlines = MakePanel(panelDir, "p03_p1_gridlines", PanelType.Static,
                caption: "The grid exists on the ground before any structure stands on it.",
                gutter: "The plan precedes the city. Someone planned this who would never live here.");

            var p03_p1_stabilize = MakePanel(panelDir, "p03_p1_stabilize", PanelType.Stabilize,
                caption: "This panel is destabilising. Restore the survey grid.",
                startsCorrupted: true, corruptionLevel: 0.5f,
                puzzleStabilizeDuration: 2f);

            var page03_1 = MakePage(pageDir, "page_03_1", PageLayout.FourPanel,
                new[] { p03_p1_floodplain, p03_p1_survey, p03_p1_gridlines, p03_p1_stabilize });

            // ── Page 2 — "The Migration Reorder" (Puzzle: A1) ────────────────
            var p03_p2_reorder = MakePanel(panelDir, "p03_p2_reorder", PanelType.Reorder,
                caption: "Six stages of city construction. They are out of order. Survey requires stakes, not the other way around.",
                gutter: "Look at where the surveyors are looking. They look toward the stakes.",
                archivistNote: "Correct order: Plain → Survey → Stakes → Foundation → Structures → City-from-above.",
                revealsKeys: keyGrid != null ? new[] { keyGrid } : new KnowledgeKeyData[0],
                puzzleReorderCount: 6);

            var page03_2 = MakePage(pageDir, "page_03_2", PageLayout.FullBleed,
                new[] { p03_p2_reorder },
                isFullBleed: true);

            // ── Page 3 — "The Pattern" (T2-B Kemet + A4 Overlay intro) ───────
            var p03_p3_kemet = MakePanel(panelDir, "p03_p3_kemet", PanelType.Static,
                caption: "T2-B: Kemet. After the water receded. Before the names hardened.");

            var p03_p3_grid_again = MakePanel(panelDir, "p03_p3_grid_again", PanelType.Static,
                caption: "The grid again. Same alignment. Same scale. Different geography.",
                archivistNote:
                    "The design specifications are identical. Across geography. Across generations. " +
                    "Whoever drafted them was either everywhere, or nowhere at all.");

            var p03_p3_overlay_intro = MakePanel(panelDir, "p03_p3_overlay_intro", PanelType.Interact,
                caption: "These pages want to be placed together. Drag one onto the other.",
                gutter: "Pattern recognition is how you know you are awake.");

            var page03_3 = MakePage(pageDir, "page_03_3", PageLayout.Strip,
                new[] { p03_p3_kemet, p03_p3_grid_again, p03_p3_overlay_intro });

            // ── Page 4 — "Overlay Puzzle" (A4) ───────────────────────────────
            var p03_p4_overlay = MakePanel(panelDir, "p03_p4_overlay", PanelType.Interact,
                caption: "Align the Sumer grid over the Kemet grid. Match the orientation marks.",
                gutter: "The triangle at center. Both cities. Same position. Same proportions. I have no explanation.",
                archivistNote:
                    "A4 Overlay complete: a triangle with an open apex aligns at exact center of both cities. " +
                    "The shape is present in the grid, not drawn on it.",
                revealsKeys: keyCorrespondence != null ? new[] { keyCorrespondence } : new KnowledgeKeyData[0]);

            var page03_4 = MakePage(pageDir, "page_03_4", PageLayout.FullBleed,
                new[] { p03_p4_overlay },
                isFullBleed: true);

            // ── Page 5 — "Symbolic Lens Unlocked + Triangle Seed" ────────────
            var p03_p5_archivist = MakePanel(panelDir, "p03_p5_archivist", PanelType.Static,
                caption: "You are starting to see something that is not in any single panel. It is between them.",
                archivistNote:
                    "The Symbolic Lens is now available. " +
                    "This lens shows what structures mean, not what they are.");

            var p03_p5_lensswitch = MakePanel(panelDir, "p03_p5_lensswitch", PanelType.Interact,
                caption: "The Symbolic Lens activates. Both Symbolic and Technologic lenses work for the rest of this issue.");

            var p03_p5_triangle = MakePanel(panelDir, "p03_p5_triangle", PanelType.Static,
                caption: "The symbol precedes the structure. The structure is the argument for the symbol.",
                gutter: "The triangle was here before the city. You are only now able to see it.");

            var page03_5 = MakePage(pageDir, "page_03_5", PageLayout.Strip,
                new[] { p03_p5_archivist, p03_p5_lensswitch, p03_p5_triangle });

            // ── Page 6 — "The Gutter Whisper + Atlantis Branch" (G2) ─────────
            var p03_p6_coastline = MakePanel(panelDir, "p03_p6_coastline", PanelType.Static,
                caption: "T2-C: Atlantis (reconstructed). This thread is fragmentary. I cannot verify it.",
                archivistNote:
                    "The Archive notes the fragmentary status. " +
                    "Atlantis as depicted is a myth-reference, not a claim.");

            var p03_p6_rings = MakePanel(panelDir, "p03_p6_rings", PanelType.Static,
                caption: "Concentric rings. The triangle at the exact center. Again.");

            var p03_p6_whisper = MakePanel(panelDir, "p03_p6_whisper", PanelType.Gutter,
                caption: "A whisper lives here. Trace the shape with your finger to complete it.",
                gutter: "\u25b2 \u2014 they put it everywhere. You will need to know why. Later.",
                revealsKeys: keyResonance != null ? new[] { keyResonance } : new KnowledgeKeyData[0]);

            var p03_p6_atlantis = MakePanel(panelDir, "p03_p6_atlantis", PanelType.Interact,
                caption: "Optional: enter the Atlantis thread for additional lore, or continue to Issue 04.",
                isBranchPoint: true,
                branchOptions: new[]
                {
                    new BranchOption
                    {
                        label           = "T2-C Atlantis Deep",
                        targetIssueId   = "issue_03",
                        targetPageIndex = 7
                    }
                });

            var page03_6 = MakePage(pageDir, "page_03_6", PageLayout.FourPanel,
                new[] { p03_p6_coastline, p03_p6_rings, p03_p6_whisper, p03_p6_atlantis },
                entryTransition: PageTransition.GutterEntity);

            // ── Page 7 — "Cities Thread Complete" ────────────────────────────
            var p03_p7_threecities = MakePanel(panelDir, "p03_p7_threecities", PanelType.Static,
                caption: "Sumer. Kemet. The third. All three in a single glance.");

            var p03_p7_triangle = MakePanel(panelDir, "p03_p7_triangle", PanelType.Static,
                caption: "The pattern is the message. The cities are the proof. But proof of what?",
                archivistNote:
                    "The triangle symbol (\u25b2) is now in the Archive Notebook \u2014 unlabelled, just a shape. " +
                    "Full naming deferred to Issue 09.");

            var p03_p7_constellation = MakePanel(panelDir, "p03_p7_constellation", PanelType.Static,
                caption: "Cities are drawings. This drawing has an author.",
                gutter: "Three cities. One triangle. The archive grows uneasy.");

            var page03_7 = MakePage(pageDir, "page_03_7", PageLayout.FullBleed,
                new[] { p03_p7_threecities, p03_p7_triangle, p03_p7_constellation },
                entryTransition: PageTransition.InkDive,
                isFullBleed: true);

            // ── Issue 03 SO ───────────────────────────────────────────────────
            var issue03 = LoadOrCreate<IssueData>("Assets/ScriptableObjects/Issues/Issue_03.asset");
            issue03.issueId              = "issue_03";
            issue03.issueNumber          = 3;
            issue03.title                = "Cities Thread";
            issue03.arc                  = "T2 \u2014 The Grid Beneath";
            issue03.pages                = new[]
            {
                page03_1, page03_2, page03_3, page03_4, page03_5, page03_6, page03_7
            };
            issue03.prerequisiteIssueIds = new[] { "issue_02" };
            issue03.unlocksLenses        = new[] { LensType.Symbolic };
            issue03.unlocksKeys          = new KnowledgeKeyData[0];
            EditorUtility.SetDirty(issue03);

            Debug.Log("[Archive] Issue 03 data built.");
        }

        // ── Issue 04 — "The Architect Thread" ────────────────────────────────────

        private static void BuildIssue04()
        {
            string panelDir = "Assets/ScriptableObjects/Panels/Issue04";
            string pageDir  = "Assets/ScriptableObjects/Pages/Issue04";

            var keyMirror = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_MIRROR.asset");

            // ── Page 1 — "T3-A Entry: The Scriptorium" ────────────────────────
            var p04_p1_scriptorium = MakePanel(panelDir, "p04_p1_scriptorium", PanelType.Static,
                caption: "T3-A: The Architect's workshop. Older documents than any institution admits.",
                archivistNote: "Record T3-A \u2014 Thoth's Blueprint. The translation is not metaphorical.");

            var p04_p1_architect = MakePanel(panelDir, "p04_p1_architect", PanelType.Static,
                caption: "A cloaked figure, back turned. The Architect. Not the inventor \u2014 the translator.",
                gutter: "He left room in the blueprint. He left room for you.");

            var p04_p1_diagram = MakePanel(panelDir, "p04_p1_diagram", PanelType.Static,
                caption: "The diagram stretches further than this page. It has always stretched further than any page.");

            var page04_1 = MakePage(pageDir, "page_04_1", PageLayout.Strip,
                new[] { p04_p1_scriptorium, p04_p1_architect, p04_p1_diagram },
                entryTransition: PageTransition.InkDive);

            // ── Page 2 — "C3 Correspondence Puzzle" ───────────────────────────
            var p04_p2_starmap = MakePanel(panelDir, "p04_p2_starmap", PanelType.Static,
                caption: "The sky at a specific date. Specific time. Specific place.",
                archivistNote: "As above, so below. The Architect's blueprint phrase.");

            var p04_p2_groundgrid = MakePanel(panelDir, "p04_p2_groundgrid", PanelType.Static,
                caption: "The ground plan of the structure. The same date. The same time. Directly below.");

            var p04_p2_correspondence = MakePanel(panelDir, "p04_p2_correspondence", PanelType.Interact,
                caption: "Align the star map to the ground plan. The geometry will complete itself.",
                gutter: "The layout is an argument. The argument is: the sky was consulted before the first stone.",
                archivistNote:
                    "C3 Correspondence complete: three star positions map exactly to three major structures.");

            var page04_2 = MakePage(pageDir, "page_04_2", PageLayout.Strip,
                new[] { p04_p2_starmap, p04_p2_groundgrid, p04_p2_correspondence });

            // ── Page 3 — "D1 Glyph Evolution" (full Reorder) ─────────────────
            var p04_p3_glyphs = MakePanel(panelDir, "p04_p3_glyphs", PanelType.Reorder,
                caption: "The glyph chain spans twelve stages. Each stage is a translation of the previous.",
                gutter: "Read the chain and the meaning is clear. Ignore the chain and the glyph is decoration.",
                archivistNote:
                    "D1 Glyph Evolution (full): the MODIFICATION glyph is stage 7 of 12. " +
                    "The TRANSLATION key applies here.",
                puzzleReorderCount: 6);

            var page04_3 = MakePage(pageDir, "page_04_3", PageLayout.FullBleed,
                new[] { p04_p3_glyphs },
                isFullBleed: true);

            // ── Page 4 — "Mystery Schools" (T3-B) ────────────────────────────
            var p04_p4_school = MakePanel(panelDir, "p04_p4_school", PanelType.Static,
                caption: "T3-B: Mystery Schools. The knowledge went underground. It did not disappear.");

            var p04_p4_chain = MakePanel(panelDir, "p04_p4_chain", PanelType.Static,
                caption: "Pythagoras. Hermes Trismegistus. The unnamed custodians before them.",
                archivistNote:
                    "The chain is verifiable through documentary overlap. " +
                    "The Archive does not require you to believe this. It does require that you note it.");

            var p04_p4_fragment = MakePanel(panelDir, "p04_p4_fragment", PanelType.Static,
                caption: "A fragment: 'The work of the Architect is never finished until the last reader completes it.'",
                gutter: "You are the latest reader. Or the last one.");

            var page04_4 = MakePage(pageDir, "page_04_4", PageLayout.Strip,
                new[] { p04_p4_school, p04_p4_chain, p04_p4_fragment });

            // ── Page 5 — "C4 Mirror-City Intro" ──────────────────────────────
            var p04_p5_mirrorA = MakePanel(panelDir, "p04_p5_mirrorA", PanelType.Static,
                caption: "The city above. The city the diagram describes.",
                archivistNote:
                    "C4 Mirror-City introduced here. " +
                    "The concept: the city is two drawings that complete each other.");

            var p04_p5_mirrorB = MakePanel(panelDir, "p04_p5_mirrorB", PanelType.Interact,
                caption: "There is a city below this one. Drag the reflection into alignment.",
                gutter: "As above, so below is not metaphor. It is a construction method.",
                revealsKeys: keyMirror != null ? new[] { keyMirror } : new KnowledgeKeyData[0]);

            var page04_5 = MakePage(pageDir, "page_04_5", PageLayout.TwoPanel,
                new[] { p04_p5_mirrorA, p04_p5_mirrorB });

            // ── Page 6 — "All Five Lenses Active" ────────────────────────────
            var p04_p6_alllenses = MakePanel(panelDir, "p04_p6_alllenses", PanelType.Interact,
                caption: "The Political Lens and Spiritual Lens are now available. All five lenses active.",
                archivistNote:
                    "The Architect's work requires all five readings simultaneously. " +
                    "You now have the complete set.");

            var page04_6 = MakePage(pageDir, "page_04_6", PageLayout.FullBleed,
                new[] { p04_p6_alllenses },
                entryTransition: PageTransition.GutterEntity,
                isFullBleed: true);

            // ── Page 7 — "Constellation Update" ──────────────────────────────
            var p04_p7_constA = MakePanel(panelDir, "p04_p7_constA", PanelType.Static,
                caption: "T3 nodes: fully lit. The Architect Thread complete.",
                gutter: "The translator is done. The translation is not.");

            var p04_p7_constB = MakePanel(panelDir, "p04_p7_constB", PanelType.Static,
                caption: "Five threads illuminate. The sixth thread \u2014 the Break \u2014 is visible ahead.",
                archivistNote: "The line to T4 is now visible in the Constellation Map.");

            var page04_7 = MakePage(pageDir, "page_04_7", PageLayout.TwoPanel,
                new[] { p04_p7_constA, p04_p7_constB });

            // ── Page 8 — "Closing Splash" ─────────────────────────────────────
            var p04_p8_closing = MakePanel(panelDir, "p04_p8_closing", PanelType.Static,
                caption: "The Architect left the blueprint. You are holding a copy.",
                gutter: "The blueprint always had your name in it. You were the missing measurement.",
                archivistNote:
                    "Issue 04 complete. Five lenses active. " +
                    "The Architect is named but not yet met.");

            var page04_8 = MakePage(pageDir, "page_04_8", PageLayout.FullBleed,
                new[] { p04_p8_closing },
                entryTransition: PageTransition.InkDive,
                isFullBleed: true);

            // ── Issue 04 SO ───────────────────────────────────────────────────
            var issue04 = LoadOrCreate<IssueData>("Assets/ScriptableObjects/Issues/Issue_04.asset");
            issue04.issueId              = "issue_04";
            issue04.issueNumber          = 4;
            issue04.title                = "The Architect Thread";
            issue04.arc                  = "T3 \u2014 The Blueprint Beneath";
            issue04.pages                = new[]
            {
                page04_1, page04_2, page04_3, page04_4, page04_5, page04_6, page04_7, page04_8
            };
            issue04.prerequisiteIssueIds = new[] { "issue_03" };
            issue04.unlocksLenses        = new[] { LensType.Political, LensType.Spiritual };
            issue04.unlocksKeys          = new KnowledgeKeyData[0];
            EditorUtility.SetDirty(issue04);

            Debug.Log("[Archive] Issue 04 data built.");
        }

        // ── Issue 05 — "The Break" ────────────────────────────────────────────────

        private static void BuildIssue05()
        {
            string panelDir = "Assets/ScriptableObjects/Panels/Issue05";
            string pageDir  = "Assets/ScriptableObjects/Pages/Issue05";

            var keyRestoration = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_RESTORATION.asset");

            // ── Page 1 — "T4-A Entry: The Defacement" ────────────────────────
            var p05_p1_wall = MakePanel(panelDir, "p05_p1_wall", PanelType.Static,
                caption: "T4-A: A wall of glyphs. Some are missing. The absences are not accidental.",
                archivistNote:
                    "Record T4-A \u2014 The era of erasure begins here. " +
                    "The Archive treats deliberate removal of records as a category of record.");

            var p05_p1_chisel = MakePanel(panelDir, "p05_p1_chisel", PanelType.Static,
                caption: "They worked systematically. This was not vandalism. This was policy.",
                gutter: "The permanent record is whatever survives. Survival is sometimes chosen.");

            var p05_p1_gap = MakePanel(panelDir, "p05_p1_gap", PanelType.Static,
                caption: "The gap where a glyph was. Its outline still pressed in stone.",
                archivistNote: "Defacement Undo (B3) becomes available. The ghost glyphs are the key.");

            var page05_1 = MakePage(pageDir, "page_05_1", PageLayout.Strip,
                new[] { p05_p1_wall, p05_p1_chisel, p05_p1_gap },
                entryTransition: PageTransition.InkDive);

            // ── Page 2 — "Scribe Faction" ─────────────────────────────────────
            var p05_p2_scribeA = MakePanel(panelDir, "p05_p2_scribeA", PanelType.Static,
                caption: "The Scribes: an institutional force. They are not characters. They are a process.");

            var p05_p2_scribeB = MakePanel(panelDir, "p05_p2_scribeB", PanelType.Static,
                caption: "They had keys. The remove order was issued from inside.",
                gutter: "Whoever holds the keys decides what the record says.");

            var p05_p2_corrupt = MakePanel(panelDir, "p05_p2_corrupt", PanelType.Stabilize,
                caption: "Active Scribe corruption. The record is being altered. Hold to stabilise.",
                startsCorrupted: true, corruptionLevel: 0.85f,
                puzzleStabilizeDuration: 3.5f);

            var p05_p2_revealed = MakePanel(panelDir, "p05_p2_revealed", PanelType.Static,
                caption: "What they were hiding: the record shows the same event the official account denies.",
                archivistNote:
                    "Political Lens: the Scribes are now visible as a faction entry in the Archive.");

            var page05_2 = MakePage(pageDir, "page_05_2", PageLayout.FourPanel,
                new[] { p05_p2_scribeA, p05_p2_scribeB, p05_p2_corrupt, p05_p2_revealed });

            // ── Page 3 — "Defacement Undo" (Puzzle: B3) ──────────────────────
            var p05_p3_defacement = MakePanel(panelDir, "p05_p3_defacement", PanelType.Interact,
                caption: "Six glyphs removed. Their ghosted shapes remain. Match each ghost to its missing glyph.",
                gutter: "The sixth glyph has no ghost. Reconstruct it from context. The apex is always the same shape.",
                archivistNote:
                    "B3 Defacement Undo: 5 of 6 glyphs restorable from ghosts. " +
                    "The 6th glyph (apex symbol from T2) must be inferred from the surrounding chain. " +
                    "Lock deferred to Issue 08 if missed.",
                revealsKeys: keyRestoration != null ? new[] { keyRestoration } : new KnowledgeKeyData[0]);

            var page05_3 = MakePage(pageDir, "page_05_3", PageLayout.FullBleed,
                new[] { p05_p3_defacement },
                isFullBleed: true);

            // ── Page 4 — "The Identity Node" (D3 partial) ────────────────────
            var p05_p4_figure1 = MakePanel(panelDir, "p05_p4_figure1", PanelType.Static,
                caption: "The figure at the edge of two stories.",
                archivistNote: "T4-B transition begins.");

            var p05_p4_figure2 = MakePanel(panelDir, "p05_p4_figure2", PanelType.Static,
                caption: "The same figure? In travel clothes. Walking away from a city. The face is the same.");

            var p05_p4_carried = MakePanel(panelDir, "p05_p4_carried", PanelType.Static,
                caption: "A rectangular void. The symbol for CARRIED. Nearby: this figure. Always.",
                gutter: "Whoever carried it, it was never supposed to stay carried.");

            var p05_p4_glyphs = MakePanel(panelDir, "p05_p4_glyphs", PanelType.Static,
                caption: "Two role glyphs. Different. But contain the same core shape.");

            var p05_p4_node = MakePanel(panelDir, "p05_p4_node", PanelType.Interact,
                caption: "These two figures share an origin. Connect the symbol they share.",
                archivistNote:
                    "D3 Identity Node: partial connection made. " +
                    "LOCKED: node will not resolve until the carried object is found. Issue 10.");

            var page05_4 = MakePage(pageDir, "page_05_4", PageLayout.Strip,
                new[] { p05_p4_figure1, p05_p4_figure2, p05_p4_carried, p05_p4_glyphs, p05_p4_node });

            // ── Page 5 — "The Exile" ──────────────────────────────────────────
            var p05_p5_walking = MakePanel(panelDir, "p05_p5_walking", PanelType.Static,
                caption: "The figure walking away from the city. The triangle structure visible behind them, incomplete.");

            var p05_p5_cloth = MakePanel(panelDir, "p05_p5_cloth", PanelType.Static,
                caption: "They carry something wrapped in cloth. Rectangular. Heavy.",
                gutter: "The carrying poles are included in the construction. This was planned.");

            var p05_p5_desert = MakePanel(panelDir, "p05_p5_desert", PanelType.Static,
                caption: "The exile was not banishment. It was evacuation.",
                archivistNote:
                    "Spiritual Lens: the wrapped object pulses once. " +
                    "T5 symbol visible on the constellation map.");

            var page05_5 = MakePage(pageDir, "page_05_5", PageLayout.Strip,
                new[] { p05_p5_walking, p05_p5_cloth, p05_p5_desert });

            // ── Page 6 — "Scribe Lock" (Hard Stabilize) ───────────────────────
            var p05_p6_heavy = MakePanel(panelDir, "p05_p6_heavy", PanelType.Stabilize,
                caption: "Active Scribe corruption \u2014 heavy. What they are hiding is here. Hold to stabilise.",
                gutter: "The redacted word is the most important word in the sentence.",
                startsCorrupted: true, corruptionLevel: 0.95f,
                puzzleStabilizeDuration: 4f);

            var p05_p6_same = MakePanel(panelDir, "p05_p6_same", PanelType.Static,
                caption: "After stabilisation: both versions of the figure as the same person. The fork was inserted later.");

            var p05_p6_edit = MakePanel(panelDir, "p05_p6_edit", PanelType.Static,
                caption: "Someone very well-resourced wanted two stories where there was one.",
                archivistNote:
                    "Identity Node in Archive Notebook updates: bridge visible but locked. " +
                    "Lock remains until the carried object is confirmed in Issue 10.");

            var p05_p6_bridge = MakePanel(panelDir, "p05_p6_bridge", PanelType.Static,
                caption: "You know who. You do not know what they carried. Until you do: the node stays open.",
                gutter: "Open nodes are not failures. They are questions the archive is still asking.");

            var page05_6 = MakePage(pageDir, "page_05_6", PageLayout.FourPanel,
                new[] { p05_p6_heavy, p05_p6_same, p05_p6_edit, p05_p6_bridge });

            // ── Page 7 — "Thread Complete" ────────────────────────────────────
            var p05_p7_wall_restored = MakePanel(panelDir, "p05_p7_wall_restored", PanelType.Static,
                caption: "Five of six. The apex will wait.",
                archivistNote:
                    "The sixth glyph position remains blank in the Notebook until Issue 08.");

            var p05_p7_constellation = MakePanel(panelDir, "p05_p7_constellation", PanelType.Static,
                caption: "T4 partially lit. T5 locked. The lock is the same symbol on the wrapped cloth.",
                gutter:
                    "They erased the record. You are restoring it. " +
                    "You think this is archaeology. It might be something more urgent than that.");

            var page05_7 = MakePage(pageDir, "page_05_7", PageLayout.TwoPanel,
                new[] { p05_p7_wall_restored, p05_p7_constellation },
                entryTransition: PageTransition.InkDive);

            // ── Issue 05 SO ───────────────────────────────────────────────────
            var issue05 = LoadOrCreate<IssueData>("Assets/ScriptableObjects/Issues/Issue_05.asset");
            issue05.issueId              = "issue_05";
            issue05.issueNumber          = 5;
            issue05.title                = "The Break";
            issue05.arc                  = "T4-A/B \u2014 The Era of Erasure";
            issue05.pages                = new[]
            {
                page05_1, page05_2, page05_3, page05_4, page05_5, page05_6, page05_7
            };
            issue05.prerequisiteIssueIds = new[] { "issue_04" };
            issue05.unlocksLenses        = new LensType[0];
            issue05.unlocksKeys          = new KnowledgeKeyData[0];
            EditorUtility.SetDirty(issue05);

            Debug.Log("[Archive] Issue 05 data built.");
        }

        // ── Issue 06 — "Mirror Site" ──────────────────────────────────────────────

        private static void BuildIssue06()
        {
            string panelDir = "Assets/ScriptableObjects/Panels/Issue06";
            string pageDir  = "Assets/ScriptableObjects/Pages/Issue06";

            var keyAlignment = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_ALIGNMENT.asset");

            // ── Page 1 — "The Angular Problem" ───────────────────────────────
            var p06_p1_grid = MakePanel(panelDir, "p06_p1_grid", PanelType.Static,
                caption: "The city grid from above. A 15.5\u00b0 offset from true north. Every major structure.",
                archivistNote:
                    "Record T5-A \u2014 The Mirror Site. " +
                    "The angular correspondence requires specific coordinates.");

            var p06_p1_compass = MakePanel(panelDir, "p06_p1_compass", PanelType.Static,
                caption: "The compass indicates true north. The city disagrees by exactly 15.5\u00b0.",
                gutter: "This is not subsidence. This is not gradual drift. This is a deliberate 15.5\u00b0 decision.");

            var p06_p1_starmap = MakePanel(panelDir, "p06_p1_starmap", PanelType.Static,
                caption: "The star Thuban. Pole star at 2500 BCE. Offset from current north: 15.5\u00b0.",
                archivistNote: "C1 Cardinal Alignment (advanced) begins here.");

            var p06_p1_rotation = MakePanel(panelDir, "p06_p1_rotation", PanelType.Interact,
                caption: "Rotate the star map to 2500 BCE. The city grid will align itself.",
                gutter: "The city was not built facing where we are. It was built facing when we were.");

            var page06_1 = MakePage(pageDir, "page_06_1", PageLayout.FourPanel,
                new[] { p06_p1_grid, p06_p1_compass, p06_p1_starmap, p06_p1_rotation },
                entryTransition: PageTransition.InkDive);

            // ── Page 2 — "C1 Cardinal Alignment (full)" ───────────────────────
            var p06_p2_aligned = MakePanel(panelDir, "p06_p2_aligned", PanelType.Static,
                caption: "Aligned. All three major structures on a diagonal. The diagonal faces Thuban.",
                archivistNote:
                    "C1 complete: the diagonal is the Orion's Belt / Thuban correspondence confirmed.");

            var p06_p2_locked = MakePanel(panelDir, "p06_p2_locked", PanelType.Stabilize,
                caption: "The alignment panel is destabilising. Scribe interference. Hold to lock it.",
                startsCorrupted: true, corruptionLevel: 0.6f,
                puzzleStabilizeDuration: 2.5f);

            var p06_p2_alignkey = MakePanel(panelDir, "p06_p2_alignkey", PanelType.Static,
                caption: "The alignment is now recorded. The Notebook preserves it.",
                revealsKeys: keyAlignment != null ? new[] { keyAlignment } : new KnowledgeKeyData[0]);

            var page06_2 = MakePage(pageDir, "page_06_2", PageLayout.Strip,
                new[] { p06_p2_aligned, p06_p2_locked, p06_p2_alignkey });

            // ── Page 3 — "C4 Mirror-City: Full Puzzle" ────────────────────────
            var p06_p3_mirrorcity = MakePanel(panelDir, "p06_p3_mirrorcity", PanelType.Interact,
                caption: "The full Mirror-City puzzle. The city above aligns with the city implied below.",
                gutter: "There is a city beneath this city. There always has been.",
                archivistNote:
                    "C4 Mirror-City (full): align six orientation pairs. " +
                    "The reflection axis is the Nile \u2014 the city mirrors across water, not land.");

            var page06_3 = MakePage(pageDir, "page_06_3", PageLayout.FullBleed,
                new[] { p06_p3_mirrorcity },
                isFullBleed: true);

            // ── Page 4 — "A4 Overlay: Page 1 of 3" ───────────────────────────
            var p06_p4_overlayA1 = MakePanel(panelDir, "p06_p4_overlayA1", PanelType.Static,
                caption: "Ground plan A: the plateau.");
            var p06_p4_overlayA2 = MakePanel(panelDir, "p06_p4_overlayA2", PanelType.Static,
                caption: "Ground plan B: the star field at the correct date.");
            var p06_p4_overlayA3 = MakePanel(panelDir, "p06_p4_overlayA3", PanelType.Interact,
                caption: "Overlay the star field onto the ground plan. Hold the orientation.",
                gutter: "The stars were drawn first. Then the plateau was marked.");
            var p06_p4_overlayA4 = MakePanel(panelDir, "p06_p4_overlayA4", PanelType.Static,
                caption: "Three points align. The overlay requires two more pages to complete.");

            var page06_4 = MakePage(pageDir, "page_06_4", PageLayout.FourPanel,
                new[] { p06_p4_overlayA1, p06_p4_overlayA2, p06_p4_overlayA3, p06_p4_overlayA4 });

            // ── Page 5 — "A4 Overlay: Page 2 of 3" ───────────────────────────
            var p06_p5_overlayB1 = MakePanel(panelDir, "p06_p5_overlayB1", PanelType.Static,
                caption: "The plateau again, with additional survey points marked.");
            var p06_p5_overlayB2 = MakePanel(panelDir, "p06_p5_overlayB2", PanelType.Static,
                caption: "The shaft angles from the structure \u2014 each pointing to a specific star.");
            var p06_p5_overlayB3 = MakePanel(panelDir, "p06_p5_overlayB3", PanelType.Interact,
                caption: "Add the shaft-direction vectors to the overlay. They extend beyond the plateau.",
                gutter: "The structure points at the sky in four specific directions. None are accidents.");
            var p06_p5_overlayB4 = MakePanel(panelDir, "p06_p5_overlayB4", PanelType.Static,
                caption: "Six points aligned. One remains. The apex.");

            var page06_5 = MakePage(pageDir, "page_06_5", PageLayout.FourPanel,
                new[] { p06_p5_overlayB1, p06_p5_overlayB2, p06_p5_overlayB3, p06_p5_overlayB4 });

            // ── Page 6 — "A4 Overlay: Page 3 of 3 + Pyramid First Glimpse" ───
            var p06_p6_overlayC1 = MakePanel(panelDir, "p06_p6_overlayC1", PanelType.Interact,
                caption: "Final overlay alignment. The seventh point is the apex. Place it.",
                archivistNote: "A4 Overlay (3-page) complete. The structure is now fully mapped.");

            var p06_p6_overlayC2 = MakePanel(panelDir, "p06_p6_overlayC2", PanelType.Static,
                caption: "Overlay complete. Seven points. A shape that has no comparable precedent in the record.");

            var p06_p6_pyramid = MakePanel(panelDir, "p06_p6_pyramid", PanelType.Static,
                caption: "",
                gutter: "The shape at the overlay center. No label. No caption.",
                archivistNote:
                    "First glimpse of the pyramid silhouette \u2014 no label, no explanation. " +
                    "Visible only as an outline. Full reveal deferred to Issue 09.");

            var p06_p6_horizon = MakePanel(panelDir, "p06_p6_horizon", PanelType.Static,
                caption: "The shape has always been on the horizon.",
                gutter: "You have been looking at it this whole time.");

            var page06_6 = MakePage(pageDir, "page_06_6", PageLayout.FourPanel,
                new[] { p06_p6_overlayC1, p06_p6_overlayC2, p06_p6_pyramid, p06_p6_horizon });

            // ── Page 7 — "Thread Complete" ────────────────────────────────────
            var p06_p7_mirrorsite = MakePanel(panelDir, "p06_p7_mirrorsite", PanelType.Static,
                caption: "The Mirror Site is documented. City above. City below. Star above. Star below.",
                archivistNote:
                    "Issue 06 complete. " +
                    "The pyramid shape is visible in the Archive Notebook \u2014 no label.");

            var p06_p7_closing = MakePanel(panelDir, "p06_p7_closing", PanelType.Static,
                caption: "The shape on the horizon. You recognise it but cannot name it. That is by design.",
                gutter: "The archive grants no caption for this shape. The shape will caption itself. Later.",
                archivistNote:
                    "T5 constellation nodes now lit. " +
                    "Line to T6 (the Vault) appears.");

            var page06_7 = MakePage(pageDir, "page_06_7", PageLayout.TwoPanel,
                new[] { p06_p7_mirrorsite, p06_p7_closing },
                entryTransition: PageTransition.InkDive);

            // ── Issue 06 SO ───────────────────────────────────────────────────
            var issue06 = LoadOrCreate<IssueData>("Assets/ScriptableObjects/Issues/Issue_06.asset");
            issue06.issueId              = "issue_06";
            issue06.issueNumber          = 6;
            issue06.title                = "Mirror Site";
            issue06.arc                  = "T5 \u2014 The Shape on the Horizon";
            issue06.pages                = new[]
            {
                page06_1, page06_2, page06_3, page06_4, page06_5, page06_6, page06_7
            };
            issue06.prerequisiteIssueIds = new[] { "issue_05" };
            issue06.unlocksLenses        = new LensType[0];
            issue06.unlocksKeys          = new KnowledgeKeyData[0];
            EditorUtility.SetDirty(issue06);

            Debug.Log("[Archive] Issue 06 data built.");
        }

        // ── Issue 07 — "The Vault" ────────────────────────────────────────────────

        private static void BuildIssue07()
        {
            string panelDir = "Assets/ScriptableObjects/Panels/Issue07";
            string pageDir  = "Assets/ScriptableObjects/Pages/Issue07";

            var keyPassage    = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_PASSAGE.asset");
            var keyDescent    = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_DESCENT.asset");
            var keyRevelation = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_REVELATION.asset");
            var keyBlueprint  = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_BLUEPRINT.asset");

            // ── Page 1 — "Exterior to Micro-Camera Deploy" ───────────────────
            var p07_p1_exterior = MakePanel(panelDir, "p07_p1_exterior", PanelType.Static,
                caption: "The structure's exterior. Every measurement corresponds to something else.",
                archivistNote:
                    "Record T6-A \u2014 The Vault interior. Micro-camera required for shaft access.");

            var p07_p1_entrance = MakePanel(panelDir, "p07_p1_entrance", PanelType.Static,
                caption: "The entrance passage is narrow. Descending. Precisely angled.");

            var p07_p1_passage = MakePanel(panelDir, "p07_p1_passage", PanelType.Static,
                caption: "The passage geometry changes as it descends. The angle is deliberate.",
                gutter: "Going in is easy. Finding the center requires the right lenses.");

            var p07_p1_microcam = MakePanel(panelDir, "p07_p1_microcam", PanelType.Interact,
                caption: "The micro-camera is deployed. You can now navigate the shaft from inside the panel.",
                archivistNote:
                    "C2 Shaft Labyrinth: the labyrinth is the panel. Navigate by tapping directions.");

            var page07_1 = MakePage(pageDir, "page_07_1", PageLayout.FourPanel,
                new[] { p07_p1_exterior, p07_p1_entrance, p07_p1_passage, p07_p1_microcam },
                entryTransition: PageTransition.InkDive);

            // ── Page 2 — "C2 Shaft Labyrinth" ────────────────────────────────
            KnowledgeKeyData[] labyrinthKeys;
            if (keyPassage != null && keyDescent != null)
                labyrinthKeys = new[] { keyPassage, keyDescent };
            else if (keyPassage != null)
                labyrinthKeys = new[] { keyPassage };
            else
                labyrinthKeys = new KnowledgeKeyData[0];

            var p07_p2_labyrinth = MakePanel(panelDir, "p07_p2_labyrinth", PanelType.Interact,
                caption: "Navigate the shaft. Two lenses required: Technologic for shaft angles, Symbolic for junction markers.",
                gutter: "There is a chamber at the center. You earn it by reading the geometry.",
                archivistNote:
                    "C2 Shaft Labyrinth: main path reveals PASSAGE key. " +
                    "Alternate shaft (dead end) reveals DESCENT key. " +
                    "Hidden chamber contains Ink Restore diagram (B2 puzzle, Page 5).",
                revealsKeys: labyrinthKeys);

            var page07_2 = MakePage(pageDir, "page_07_2", PageLayout.FullBleed,
                new[] { p07_p2_labyrinth },
                isFullBleed: true);

            // ── Page 3 — "The Central Chamber" ───────────────────────────────
            var p07_p3_chamber = MakePanel(panelDir, "p07_p3_chamber", PanelType.Static,
                caption: "The chamber. Large. Walls smooth and exact. A raised stone platform in the center.",
                archivistNote:
                    "The light shaft from the ceiling aims directly at the platform. " +
                    "One shaft. One angle. One purpose.");

            var p07_p3_void = MakePanel(panelDir, "p07_p3_void", PanelType.Interact,
                caption: "The container precedes the thing contained. The void defines the object.",
                gutter: "The container was built first. This was not storage. This was a homecoming.");

            var p07_p3_archivist = MakePanel(panelDir, "p07_p3_archivist", PanelType.Static,
                caption: "Empty.",
                archivistNote:
                    "The void is the exact proportions seen in T4-B panels since Issue 05. " +
                    "I know the shape. I do not know the name. Yet.");

            var page07_3 = MakePage(pageDir, "page_07_3", PageLayout.Strip,
                new[] { p07_p3_chamber, p07_p3_void, p07_p3_archivist });

            // ── Page 4 — "The Ark Named" (Dual-Lens Activation) ──────────────
            var p07_p4_dark = MakePanel(panelDir, "p07_p4_dark", PanelType.Static,
                caption: "Without the Spiritual lens, I cannot see anything here.",
                archivistNote: "The void: dark. Empty. Nothing else visible.");

            var p07_p4_duallens = MakePanel(panelDir, "p07_p4_duallens", PanelType.Interact,
                caption: "Some objects only manifest under combined interpretation. Activate Spiritual and Technologic lenses.",
                archivistNote:
                    "First dual-lens requirement. Both must be active simultaneously. " +
                    "A new category of revelation.");

            var p07_p4_ark = MakePanel(panelDir, "p07_p4_ark", PanelType.Static,
                archivistNote:
                    "THE ARK \u2014 first labelling in the game. " +
                    "Object found in central chamber. Designed to be carried, placed, and to complete something. " +
                    "The chamber was built around it. Or for it.",
                revealsKeys: keyRevelation != null ? new[] { keyRevelation } : new KnowledgeKeyData[0]);

            var page07_4 = MakePage(pageDir, "page_07_4", PageLayout.Strip,
                new[] { p07_p4_dark, p07_p4_duallens, p07_p4_ark });

            // ── Page 5 — "Ink Restore" (Puzzle: B2) ──────────────────────────
            var p07_p5_restore = MakePanel(panelDir, "p07_p5_restore", PanelType.Interact,
                caption: "The cross-section diagram from the hidden shaft chamber. Eight line segments faded. Trace each to restore it.",
                gutter: "The Architect's blueprint included this object. The grid included its place.",
                archivistNote:
                    "B2 Ink Restore: 8 line segments. Symbolic Lens eases figure outlines. " +
                    "Technologic Lens eases structural outlines. Full diagram in Archive Notebook on completion.",
                revealsKeys: keyBlueprint != null ? new[] { keyBlueprint } : new KnowledgeKeyData[0]);

            var page07_5 = MakePage(pageDir, "page_07_5", PageLayout.FullBleed,
                new[] { p07_p5_restore },
                isFullBleed: true);

            // ── Page 6 — "E1 Ark Assembly: Component 1" ──────────────────────
            var p07_p6_incomplete = MakePanel(panelDir, "p07_p6_incomplete", PanelType.Static,
                caption: "The Ark is visible \u2014 but not whole. One carrying pole is missing.",
                archivistNote:
                    "The ghost-outline of the missing pole is visible on the chamber floor.");

            var p07_p6_pickup = MakePanel(panelDir, "p07_p6_pickup", PanelType.Interact,
                caption: "First Ark component: the missing carrying pole. Tap to recover.",
                gutter: "One of four poles. Two for each side.",
                archivistNote:
                    "E1 Ark Assembly component 1 of 4. " +
                    "Ark Inventory now unlocked in the Archive Notebook.");

            var p07_p6_notebook = MakePanel(panelDir, "p07_p6_notebook", PanelType.Static,
                caption: "The Ark is not complete. It requires all components before it can be moved.",
                archivistNote:
                    "Notebook Ark entry updates: carrying pole filled in. " +
                    "Three more required \u2014 Issues 08, 09, 10 each contain one component.");

            var p07_p6_scribes = MakePanel(panelDir, "p07_p6_scribes", PanelType.Static,
                caption: "Scribe corruption at the chamber edge. Heavy. Directed. Exit before the panel closes.",
                gutter: "Collect all the pieces. Then carry it to where it was always going.",
                startsCorrupted: true, corruptionLevel: 0.4f);

            var page07_6 = MakePage(pageDir, "page_07_6", PageLayout.FourPanel,
                new[] { p07_p6_incomplete, p07_p6_pickup, p07_p6_notebook, p07_p6_scribes });

            // ── Page 7 — "Emerged: Issue 07 Complete" ────────────────────────
            var p07_p7_emerged = MakePanel(panelDir, "p07_p7_emerged", PanelType.Static,
                caption: "Exterior of the structure again. Emerged. The chamber knows it was found.",
                archivistNote:
                    "The interior is faintly pulsing. The Ark component is in inventory.");

            var p07_p7_notebook = MakePanel(panelDir, "p07_p7_notebook", PanelType.Static,
                caption:
                    "Record 07 \u2014 The Ark exists. It was at the center of the largest intentional structure ever built. " +
                    "And it is currently scattered. I will find the rest of it.",
                gutter: "The Ark is named. One piece is found. Three issues remain before it comes home.",
                archivistNote:
                    "T4 cluster nearly complete. T5 lock symbol visible \u2014 same as symbol on wrapped cloth from Issue 05. " +
                    "Dual-lens combinations now permanently available.");

            var page07_7 = MakePage(pageDir, "page_07_7", PageLayout.TwoPanel,
                new[] { p07_p7_emerged, p07_p7_notebook },
                entryTransition: PageTransition.InkDive);

            // ── Issue 07 SO ───────────────────────────────────────────────────
            var issue07 = LoadOrCreate<IssueData>("Assets/ScriptableObjects/Issues/Issue_07.asset");
            issue07.issueId              = "issue_07";
            issue07.issueNumber          = 7;
            issue07.title                = "The Vault";
            issue07.arc                  = "T6 \u2014 The Center of the Structure";
            issue07.pages                = new[]
            {
                page07_1, page07_2, page07_3, page07_4, page07_5, page07_6, page07_7
            };
            issue07.prerequisiteIssueIds = new[] { "issue_06" };
            issue07.unlocksLenses        = new LensType[0];
            issue07.unlocksKeys          = new KnowledgeKeyData[0];
            EditorUtility.SetDirty(issue07);

            Debug.Log("[Archive] Issue 07 data built.");
        }

        // ── Issue 08 — "Convergence Begins" ──────────────────────────────────────

        private static void BuildIssue08()
        {
            string panelDir = "Assets/ScriptableObjects/Panels/Issue08";
            string pageDir  = "Assets/ScriptableObjects/Pages/Issue08";

            var keyTheFive = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_THE_FIVE.asset");
            var keyCenter  = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_CENTER.asset");

            // ── Page 1 — "Thread Ends, Thread Begins" ────────────────────────
            var p08_p1_t1 = MakePanel(panelDir, "p08_p1_t1", PanelType.Static,
                caption: "T1: Origins. Descent over Eridu.");

            var p08_p1_t2 = MakePanel(panelDir, "p08_p1_t2", PanelType.Static,
                caption: "T2: Cities. The grid. The triangle.");

            var p08_p1_t3 = MakePanel(panelDir, "p08_p1_t3", PanelType.Static,
                caption: "T3: The Architect. The blueprint.");

            var p08_p1_t4 = MakePanel(panelDir, "p08_p1_t4", PanelType.Static,
                caption: "T4: The Exile at the horizon. Looking forward. Caption: \"The exile ends somewhere. The thread knows where.\"");

            var p08_p1_t5 = MakePanel(panelDir, "p08_p1_t5", PanelType.Interact,
                caption: "T5: The locked thread. The T5 node pulses for the first time. Not yet. But close.",
                gutter: "On the other side of 'I don't know' is the only thing worth finding.");

            var page08_1 = MakePage(pageDir, "page_08_1", PageLayout.Strip,
                new[] { p08_p1_t1, p08_p1_t2, p08_p1_t3, p08_p1_t4, p08_p1_t5 });

            // ── Page 2 — "Semantic Merge" (Puzzle: D4) ────────────────────────
            var p08_p2_merge = MakePanel(panelDir, "p08_p2_merge", PanelType.Interact,
                caption: "Five symbolic terms, each split across timelines. Merge each pair into its unified form.",
                gutter: "The Ark isn't just a container. It's an instrument. These are its notes.",
                archivistNote:
                    "D4 Semantic Merge: STABILITY, CIRCUIT, CARRY, SEAL, COMPLETE. " +
                    "Incorrect merge of CIRCUIT+SEAL produces an unstable form. Use the Symbolic Lens to distinguish.",
                revealsKeys: keyTheFive != null ? new[] { keyTheFive } : new KnowledgeKeyData[0]);

            var page08_2 = MakePage(pageDir, "page_08_2", PageLayout.FullBleed,
                new[] { p08_p2_merge },
                isFullBleed: true);

            // ── Page 3 — "The Five-Page Overlay" (Puzzle: A4 5-page variant) ─
            var p08_p3_overlayA = MakePanel(panelDir, "p08_p3_overlayA", PanelType.Static,
                caption: "Page A: T1 Origins — descent, E.D.I.N., Limiter. Place it first.",
                archivistNote: "A4 Overlay (5-page): all five timeline pages now available. Stack chronologically.");

            var p08_p3_overlayB = MakePanel(panelDir, "p08_p3_overlayB", PanelType.Static,
                caption: "Page B: T2 Cities — Sumer, Kemet, alignment.");

            var p08_p3_overlayC = MakePanel(panelDir, "p08_p3_overlayC", PanelType.Static,
                caption: "Page C: T3 Architect — blueprint, mirror-city.");

            var p08_p3_overlayD = MakePanel(panelDir, "p08_p3_overlayD", PanelType.Static,
                caption: "Page D: T4 Break — defacement, Exile, vault.");

            var p08_p3_overlayE = MakePanel(panelDir, "p08_p3_overlayE", PanelType.Interact,
                caption: "Page E: T5 — partially blank. The missing center. Align the triangle \u25b2 at center of all five.",
                gutter: "Five layers. One structure. The composite forms a cross-section of Giza.");

            var page08_3 = MakePage(pageDir, "page_08_3", PageLayout.FullBleed,
                new[] { p08_p3_overlayA, p08_p3_overlayB, p08_p3_overlayC, p08_p3_overlayD, p08_p3_overlayE },
                isFullBleed: true);

            // ── Page 4 — "The Detective Board Speaks" ────────────────────────
            var p08_p4_board = MakePanel(panelDir, "p08_p4_board", PanelType.Static,
                caption: "I see it.",
                gutter: "Now you know where to take it.",
                archivistNote:
                    "The Archive Notebook detective board completes itself. " +
                    "The connection lines spontaneously form a pyramid silhouette. " +
                    "Below it, in the Archivist's own handwriting: GIZA. " +
                    "First appearance of the word in the game.");

            var page08_4 = MakePage(pageDir, "page_08_4", PageLayout.FullBleed,
                new[] { p08_p4_board },
                entryTransition: PageTransition.GutterEntity,
                isFullBleed: true);

            // ── Page 5 — "T5 Threshold" (Unlock Sequence) ────────────────────
            var p08_p5_map = MakePanel(panelDir, "p08_p5_map", PanelType.Static,
                caption: "All nodes lit. Connection lines drawn. Pyramid silhouette at center. T5 node pulsing.");

            var p08_p5_lock = MakePanel(panelDir, "p08_p5_lock", PanelType.Interact,
                caption: "Combine five primary Knowledge Keys to unlock T5: SEQUENCE, CORRESPONDENCE, ALIGNMENT, RESTORATION, REVELATION.",
                archivistNote: "Each key glows as it enters the central slot. T5 requires all five milestone keys.");

            var p08_p5_t5lit = MakePanel(panelDir, "p08_p5_t5lit", PanelType.Static,
                caption: "T5: The Missing Center. This is not a forgotten thread. It is the thread everything else was placed around.",
                revealsKeys: keyCenter != null ? new[] { keyCenter } : new KnowledgeKeyData[0]);

            var p08_p5_archivist = MakePanel(panelDir, "p08_p5_archivist", PanelType.Static,
                caption: "I wasn't going to be this moved by a constellation map. Here we are.",
                archivistNote: "CENTER key unlocked. T5 thread accessible in Issue 09.");

            var page08_5 = MakePage(pageDir, "page_08_5", PageLayout.FourPanel,
                new[] { p08_p5_map, p08_p5_lock, p08_p5_t5lit, p08_p5_archivist });

            // ── Page 6 — "Scribe Maximum + Resonants Appear" ─────────────────
            var p08_p6_scribe = MakePanel(panelDir, "p08_p6_scribe", PanelType.Stabilize,
                caption: "Three panels simultaneously attacked. B1 Stabilize x3 sequence — rapid, 6s each.",
                startsCorrupted: true, corruptionLevel: 1.0f,
                puzzleStabilizeDuration: 6f);

            var p08_p6_archivist = MakePanel(panelDir, "p08_p6_archivist", PanelType.Static,
                caption: "This is a suppression attempt. They know what you just saw. They know you know.",
                archivistNote: "Scribe maximum intervention: heaviest corruption event in the game.");

            var p08_p6_resonants = MakePanel(panelDir, "p08_p6_resonants", PanelType.Static,
                caption: "The Resonants. First contact.",
                gutter: "Not today.",
                archivistNote:
                    "Third handwriting style appears in the margin \u2014 clean, precise, defiant. " +
                    "The Resonants have been in the Archive longer than I have. " +
                    "They are the readers who stayed.");

            var page08_6 = MakePage(pageDir, "page_08_6", PageLayout.Strip,
                new[] { p08_p6_scribe, p08_p6_archivist, p08_p6_resonants });

            // ── Page 7 — "What Comes Next" ────────────────────────────────────
            var p08_p7_09 = MakePanel(panelDir, "p08_p7_09", PanelType.Static,
                caption: "Issue 09: Enter.",
                archivistNote: "The Pyramid interior \u2014 lit, active, with open capstone slot.");

            var p08_p7_10 = MakePanel(panelDir, "p08_p7_10", PanelType.Static,
                caption: "Issue 10: Assemble.",
                archivistNote: "The Ark, components scattered and one in the player's hand.");

            var p08_p7_11 = MakePanel(panelDir, "p08_p7_11", PanelType.Static,
                caption: "Issue 11: Complete.",
                archivistNote: "A circuit of light connecting Ark to Pyramid, Djed pillar at center.");

            var p08_p7_12 = MakePanel(panelDir, "p08_p7_12", PanelType.Static,
                caption: "Issue 12: Place.",
                gutter: "You built this. Every panel restored, every glyph recovered. You built it. Now finish it.");

            var page08_7 = MakePage(pageDir, "page_08_7", PageLayout.FourPanel,
                new[] { p08_p7_09, p08_p7_10, p08_p7_11, p08_p7_12 });

            // ── Page 8 — "Issue 08 Complete: All Is Connected" ───────────────
            var p08_p8_giza = MakePanel(panelDir, "p08_p8_giza", PanelType.Static,
                caption: "GIZA. You know where to go. You know what to bring.",
                gutter: "It was always this.",
                archivistNote:
                    "Issue 08 complete. T5 unlocked. Resonants introduced. " +
                    "D4 Semantic Merge active. Giza named and confirmed. " +
                    "All five timeline threads connected.");

            var page08_8 = MakePage(pageDir, "page_08_8", PageLayout.FullBleed,
                new[] { p08_p8_giza },
                entryTransition: PageTransition.InkDive,
                isFullBleed: true);

            // ── Issue 08 SO ───────────────────────────────────────────────────
            var issue08 = LoadOrCreate<IssueData>("Assets/ScriptableObjects/Issues/Issue_08.asset");
            issue08.issueId              = "issue_08";
            issue08.issueNumber          = 8;
            issue08.title                = "Convergence Begins";
            issue08.arc                  = "T4/T5 Threshold \u2014 Cross-Timeline Synthesis";
            issue08.pages                = new[]
            {
                page08_1, page08_2, page08_3, page08_4,
                page08_5, page08_6, page08_7, page08_8
            };
            issue08.prerequisiteIssueIds = new[] { "issue_07" };
            issue08.unlocksLenses        = new LensType[0];
            issue08.unlocksKeys          = new KnowledgeKeyData[0];
            EditorUtility.SetDirty(issue08);

            Debug.Log("[Archive] Issue 08 data built.");
        }

        // ── Issue 09 — "The Pyramid Page" ────────────────────────────────────────

        private static void BuildIssue09()
        {
            string panelDir = "Assets/ScriptableObjects/Panels/Issue09";
            string pageDir  = "Assets/ScriptableObjects/Pages/Issue09";

            var keyFoundation = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_FOUNDATION.asset");
            var keyAligned    = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_ALIGNED.asset");

            // ── Page 1 — "Named Entrance" ─────────────────────────────────────
            var p09_p1_gizagate = MakePanel(panelDir, "p09_p1_gizagate", PanelType.Static,
                caption: "This is the page you've been reading toward.",
                archivistNote:
                    "\"GIZA\" carved into the cornerstone. Not an annotation. Not a caption. " +
                    "Just: there. The complete map is in your hands.");

            var p09_p1_entrance = MakePanel(panelDir, "p09_p1_entrance", PanelType.Static,
                caption: "The entrance. Familiar from Issue 07, but now fully visible.",
                gutter: "You named it. Now enter it. These are both the same act.");

            var p09_p1_shaft = MakePanel(panelDir, "p09_p1_shaft", PanelType.Interact,
                caption: "You have the full cross-section. Navigate inside.",
                archivistNote: "C2 Shaft Labyrinth (three-level variant) begins.");

            var page09_1 = MakePage(pageDir, "page_09_1", PageLayout.Strip,
                new[] { p09_p1_gizagate, p09_p1_entrance, p09_p1_shaft },
                entryTransition: PageTransition.InkDive);

            // ── Page 2 — "Level 1: Ground Passage" ───────────────────────────
            var p09_p2_level1 = MakePanel(panelDir, "p09_p2_level1", PanelType.Interact,
                caption: "Ground level. Two shaft branches: descending (subterranean) and ascending (King's chamber).",
                archivistNote:
                    "Shaft doors appear: stone slabs with mechanism slots. " +
                    "Technologic Lens reveals mechanism type. " +
                    "Junction cardinal geometry aligns with the 15.5\u00b0 offset from Issue 06.");

            var p09_p2_foundation = MakePanel(panelDir, "p09_p2_foundation", PanelType.Gutter,
                caption: "Dead-end subterranean chamber. Something was here before the structure above it.",
                gutter: "The foundation predates the monument. The monument is the argument for the foundation.",
                revealsKeys: keyFoundation != null ? new[] { keyFoundation } : new KnowledgeKeyData[0]);

            var page09_2 = MakePage(pageDir, "page_09_2", PageLayout.FullBleed,
                new[] { p09_p2_level1, p09_p2_foundation },
                isFullBleed: true);

            // ── Page 3 — "Level 2: King's Chamber Return" ────────────────────
            var p09_p3_chamber = MakePanel(panelDir, "p09_p3_chamber", PanelType.Static,
                caption: "The central chamber in full context: its position in all three levels, shafts above and below.",
                archivistNote: "The chamber is known. The context is new.");

            var p09_p3_circuit = MakePanel(panelDir, "p09_p3_circuit", PanelType.Static,
                caption: "The structure is a machine. The coffer is its center. The circuit runs everywhere.",
                gutter: "The circuit is missing one node. The node is rectangular. The node has a name.",
                archivistNote:
                    "Spiritual Lens active: gold circuit threads run from the coffer outward through stone " +
                    "connecting all shafts and chambers. Full circuit architecture visible for the first time.");

            var p09_p3_coffer = MakePanel(panelDir, "p09_p3_coffer", PanelType.Static,
                caption: "The circuit is incomplete. Something is missing from the coffer. Something specific. Something we know the shape of.",
                archivistNote: "The coffer is empty. The rectangular void. The player now understands why it matters.");

            var page09_3 = MakePage(pageDir, "page_09_3", PageLayout.Strip,
                new[] { p09_p3_chamber, p09_p3_circuit, p09_p3_coffer });

            // ── Page 4 — "Cardinal Alignment Final" (Puzzle: C1 Final) ────────
            var p09_p4_c1 = MakePanel(panelDir, "p09_p4_c1", PanelType.Interact,
                caption: "Four shafts. Four stellar targets. Adjust each shaft's angle using the 15.5\u00b0 offset at the correct historical epoch.",
                gutter: "Every angle in this chamber was designed for this moment.",
                archivistNote:
                    "C1 Cardinal Alignment (final, hardest): Orion's Belt, Thuban, Alpha Draconis. " +
                    "On completion: all four shafts align, light enters, the coffer responds. " +
                    "Circuit diagram in Spiritual Lens resolves from partial to complete (capstone node still dark).",
                revealsKeys: keyAligned != null ? new[] { keyAligned } : new KnowledgeKeyData[0]);

            var page09_4 = MakePage(pageDir, "page_09_4", PageLayout.FullBleed,
                new[] { p09_p4_c1 },
                isFullBleed: true);

            // ── Page 5 — "Level 3: The Apex Shaft" ───────────────────────────
            var p09_p5_climb1 = MakePanel(panelDir, "p09_p5_climb1", PanelType.Stabilize,
                caption: "Ascending the vertical shaft. The Scribes have corrupted sections of the passage. Stabilize.",
                startsCorrupted: true, corruptionLevel: 0.6f,
                puzzleStabilizeDuration: 3f);

            var p09_p5_climb2 = MakePanel(panelDir, "p09_p5_climb2", PanelType.Stabilize,
                caption: "Deeper corruption. The shaft narrows. Hold.",
                startsCorrupted: true, corruptionLevel: 0.8f,
                puzzleStabilizeDuration: 4f);

            var p09_p5_capstone = MakePanel(panelDir, "p09_p5_capstone", PanelType.Static,
                caption:
                    "The capstone slot. Exact proportions. Exact orientation. Prepared. Waiting. " +
                    "The only incomplete element in a structure otherwise finished to an inhuman precision.",
                gutter: "Not yet level 3. First: the hardest stabilize.");

            var p09_p5_climb3 = MakePanel(panelDir, "p09_p5_climb3", PanelType.Stabilize,
                caption: "Final stabilize before the apex chamber. Very hard. The Scribes know what is above.",
                startsCorrupted: true, corruptionLevel: 0.95f,
                puzzleStabilizeDuration: 5f);

            var page09_5 = MakePage(pageDir, "page_09_5", PageLayout.FullBleed,
                new[] { p09_p5_climb1, p09_p5_climb2, p09_p5_capstone, p09_p5_climb3 },
                isFullBleed: true);

            // ── Page 6 — "The Capstone Slot" (E4 Preview) ────────────────────
            var p09_p6_above = MakePanel(panelDir, "p09_p6_above", PanelType.Static,
                caption: "From above: the slot is perfectly cut. No rough edges. Sky barely visible through it.");

            var p09_p6_below = MakePanel(panelDir, "p09_p6_below", PanelType.Static,
                caption: "From below: starlight enters the shaft at the correct angle.",
                gutter: "The last panel hasn't been drawn yet. That's where you come in.");

            var p09_p6_trace = MakePanel(panelDir, "p09_p6_trace", PanelType.Interact,
                caption: "The slot requires a capstone. The capstone is not yet in your hands. But you know its shape. Trace it.",
                archivistNote:
                    "E4 Capstone Placement \u2014 INTRO. The tracing is saved. Puzzle marked: PENDING. " +
                    "I know what goes here. I know where to find it. " +
                    "Issue 10: the Ark. Issue 11: the connection. Issue 12: this moment.");

            var page09_6 = MakePage(pageDir, "page_09_6", PageLayout.Strip,
                new[] { p09_p6_above, p09_p6_below, p09_p6_trace });

            // ── Page 7 — "Scribe Final + Resonants Counter" ───────────────────
            var p09_p7_scribe = MakePanel(panelDir, "p09_p7_scribe", PanelType.Stabilize,
                caption: "Maximum Scribe event. Apex chamber, King's chamber, and entry passage simultaneously. Stabilize chain.",
                startsCorrupted: true, corruptionLevel: 1.0f,
                puzzleStabilizeDuration: 6f);

            var p09_p7_resonants = MakePanel(panelDir, "p09_p7_resonants", PanelType.Static,
                caption: "The Resonants counter two of the three corrupted panels. You only need to stabilize one.",
                archivistNote:
                    "Resonants: second contact. Three figures visible in margins. " +
                    "Margin note: 'The circuit architecture is now indexed by the Scribes. " +
                    "They see the full diagram. They know it's close to complete. So do we. Finish this.'");

            var p09_p7_alone = MakePanel(panelDir, "p09_p7_alone", PanelType.Static,
                caption: "I'm not alone in here. I never was.",
                gutter: "Corruption retreats. Something interrupted it.");

            var p09_p7_next = MakePanel(panelDir, "p09_p7_next", PanelType.Static,
                caption: "The Ark. The circuit. The capstone. Three movements. You know all the parts now.",
                archivistNote: "Resonants second contact logged.");

            var page09_7 = MakePage(pageDir, "page_09_7", PageLayout.FourPanel,
                new[] { p09_p7_scribe, p09_p7_resonants, p09_p7_alone, p09_p7_next });

            // ── Page 8 — "The Three-Level Map Completes" ─────────────────────
            var p09_p8_crosssection = MakePanel(panelDir, "p09_p8_crosssection", PanelType.Static,
                caption: "Complete. Except for the capstone. Except for the Ark. Two more things. Two issues. One placement.",
                archivistNote:
                    "Full Giza cross-section fills the page. All three levels mapped. " +
                    "Constellation Map: T5-A and T5-B nodes now lit. T5-C next: Assembly.");

            var page09_8 = MakePage(pageDir, "page_09_8", PageLayout.FullBleed,
                new[] { p09_p8_crosssection },
                isFullBleed: true);

            // ── Page 9 — "Issue 09 Complete" ─────────────────────────────────
            var p09_p9_closing = MakePanel(panelDir, "p09_p9_closing", PanelType.Static,
                caption: "You have the map. You have one piece of the Ark. You know where it goes. Now get the rest.",
                gutter: "GIZA is fully mapped. The circuit waits for its center. The slot waits for its capstone.");

            var p09_p9_constellation = MakePanel(panelDir, "p09_p9_constellation", PanelType.Static,
                caption: "Issue 09 complete. The Constellation Map shows three T5 nodes lit.",
                archivistNote:
                    "Issue 09 complete. Full Giza cross-section in Archive Notebook. " +
                    "Circuit architecture diagram (Spiritual Lens). " +
                    "E4 Capstone Placement: PENDING. T5-A and T5-B lit.");

            var page09_9 = MakePage(pageDir, "page_09_9", PageLayout.TwoPanel,
                new[] { p09_p9_closing, p09_p9_constellation },
                entryTransition: PageTransition.InkDive);

            // ── Issue 09 SO ───────────────────────────────────────────────────
            var issue09 = LoadOrCreate<IssueData>("Assets/ScriptableObjects/Issues/Issue_09.asset");
            issue09.issueId              = "issue_09";
            issue09.issueNumber          = 9;
            issue09.title                = "The Pyramid Page";
            issue09.arc                  = "T5 Cluster \u2014 A/B: Entry + Interior";
            issue09.pages                = new[]
            {
                page09_1, page09_2, page09_3, page09_4,
                page09_5, page09_6, page09_7, page09_8, page09_9
            };
            issue09.prerequisiteIssueIds = new[] { "issue_08" };
            issue09.unlocksLenses        = new LensType[0];
            issue09.unlocksKeys          = new KnowledgeKeyData[0];
            EditorUtility.SetDirty(issue09);

            Debug.Log("[Archive] Issue 09 data built.");
        }

        // ── Issue 10 — "The Artifact Page" ───────────────────────────────────────

        private static void BuildIssue10()
        {
            string panelDir = "Assets/ScriptableObjects/Panels/Issue10";
            string pageDir  = "Assets/ScriptableObjects/Pages/Issue10";

            var keySame       = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_SAME.asset");
            var keyComplete   = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_COMPLETE_INSTRUMENT.asset");
            var keyCarried    = AssetDatabase.LoadAssetAtPath<KnowledgeKeyData>(
                "Assets/ScriptableObjects/Keys/Key_CARRIED.asset");

            // ── Page 1 — "T5-C Entry: The Scattered Ark" ─────────────────────
            var p10_p1_t5c = MakePanel(panelDir, "p10_p1_t5c", PanelType.Static,
                caption: "T5-C: Assembly. The components were never in the same place.",
                archivistNote: "T5-C constellation node pulses.");

            var p10_p1_inventory = MakePanel(panelDir, "p10_p1_inventory", PanelType.Static,
                caption: "Ark Inventory: 1/4 poles. Three empty slots. Three ghost-outlines: a pole, the chest body, the mercy seat.",
                archivistNote: "The Ark was distributed intentionally. Waiting for reassembly. For the right reader.");

            var p10_p1_archivist = MakePanel(panelDir, "p10_p1_archivist", PanelType.Static,
                caption: "The Ark's components traveled through different hands, different timelines.",
                gutter: "Three shards. Any order. Tap each ghost outline to begin.");

            var p10_p1_shards = MakePanel(panelDir, "p10_p1_shards", PanelType.Interact,
                caption: "Tap each ghost outline to activate the recovery sequence for that component.",
                archivistNote: "Three shard paths: Shard A (second pole), Shard B (chest), Shard C (mercy seat). Order is free.");

            var page10_1 = MakePage(pageDir, "page_10_1", PageLayout.FourPanel,
                new[] { p10_p1_t5c, p10_p1_inventory, p10_p1_archivist, p10_p1_shards });

            // ── Page 2 — "Shard A: The Second Pole" ──────────────────────────
            var p10_p2_cache = MakePanel(panelDir, "p10_p2_cache", PanelType.Static,
                caption: "A desert storage cache, ancient. Sealed stone. The Resonants' symbol carved faintly on the seal.",
                archivistNote: "The Resonants preserved it. Waiting until someone finished the other threads.");

            var p10_p2_stabilize = MakePanel(panelDir, "p10_p2_stabilize", PanelType.Stabilize,
                caption: "Scribe interference has corrupted the cache panel. Stabilize to clear.",
                gutter: "Second pole recovered. Ark Inventory: 2/4.",
                startsCorrupted: true, corruptionLevel: 0.6f,
                puzzleStabilizeDuration: 3f);

            var page10_2 = MakePage(pageDir, "page_10_2", PageLayout.TwoPanel,
                new[] { p10_p2_cache, p10_p2_stabilize });

            // ── Page 3 — "Shard B: Chest Body + Identity Node Resolved" ───────
            var p10_p3_exile = MakePanel(panelDir, "p10_p3_exile", PanelType.Static,
                caption: "The Exile, older now, having traveled far. They lower a wrapped object beside a stone marker.",
                archivistNote:
                    "T4-B revisited. The carried thing and the carrier were the same story, opposite ends.");

            var p10_p3_unwrap = MakePanel(panelDir, "p10_p3_unwrap", PanelType.Static,
                caption: "The wrapped object, unwrapped: the chest body. Confirmed. The Identity Node from Issue 05 flashes \u2014 the lock opens.",
                gutter: "Two stories, one person. The Scribes split the record. The record is now whole.");

            var p10_p3_node = MakePanel(panelDir, "p10_p3_node", PanelType.Interact,
                caption: "The identity is now confirmed. Complete the node. Place the merged single glyph between the two figure-portraits.",
                archivistNote:
                    "D3 Identity Node \u2014 RESOLVED. Two figures unite into a single portrait with both role symbols. " +
                    "The exile carried the Ark. The cycle was always going to end this way.",
                revealsKeys: keySame != null ? new[] { keySame } : new KnowledgeKeyData[0]);

            var page10_3 = MakePage(pageDir, "page_10_3", PageLayout.Strip,
                new[] { p10_p3_exile, p10_p3_unwrap, p10_p3_node });

            // ── Page 4 — "Shard C: The Mercy Seat" ───────────────────────────
            var p10_p4_school = MakePanel(panelDir, "p10_p4_school", PanelType.Static,
                caption: "T3-B: A hidden chamber beneath the Mystery School. The Architect placed this here.",
                archivistNote:
                    "The lid guarded through the eras of Scribe defacement by the school structures. " +
                    "Two winged figures on the lid, wings touching.");

            var p10_p4_glyph = MakePanel(panelDir, "p10_p4_glyph", PanelType.Interact,
                caption: "A D1 Glyph chain step required to open the chamber. The access glyph from Issue 04.",
                gutter: "Mercy Seat recovered. Ark Inventory: 4/4. Complete.",
                archivistNote: "D1 Glyph (one step): access glyph recognizable from Issue 04's Glyph Evolution chain.");

            var page10_4 = MakePage(pageDir, "page_10_4", PageLayout.TwoPanel,
                new[] { p10_p4_school, p10_p4_glyph });

            // ── Page 5 — "Ark Assembly" (Puzzle: E1 Full) ────────────────────
            var p10_p5_assembly = MakePanel(panelDir, "p10_p5_assembly", PanelType.Interact,
                caption: "All four components present. Poles first. Then chest. Then lid. Each snaps with a distinct sound: tap, two-tone, resonant thud, chime-chord.",
                gutter: "All five abilities are now accessible. I'm not sure I understand what most of them do yet. That's alright. The Ark seems to.",
                archivistNote:
                    "E1 Ark Assembly (full). Completion moment: two winged figures face each other, " +
                    "wingspan tips touching at center. Ark glows warm under Spiritual Lens for 1.2s. " +
                    "Technologic shows circuit connections activating. Symbolic shows symbol network completing.",
                revealsKeys: keyComplete != null ? new[] { keyComplete } : new KnowledgeKeyData[0]);

            var page10_5 = MakePage(pageDir, "page_10_5", PageLayout.FullBleed,
                new[] { p10_p5_assembly },
                isFullBleed: true);

            // ── Page 6 — "Ark Abilities: First Active Use" ───────────────────
            var p10_p6_seal = MakePanel(panelDir, "p10_p6_seal", PanelType.Interact,
                caption: "Seal Burn: the Ark dissolves a Scribe-locked panel. A hidden Resonant location opens.",
                archivistNote: "Ability 1 of 5 demonstrated.");

            var p10_p6_scan = MakePanel(panelDir, "p10_p6_scan", PanelType.Interact,
                caption: "Scan: hidden text in the Architect's original glyph system becomes visible. A previously unreadable fragment now readable.",
                archivistNote: "Ability 2 of 5 demonstrated.");

            var p10_p6_circuit = MakePanel(panelDir, "p10_p6_circuit", PanelType.Interact,
                caption: "Circuit Link: two panels from different eras with matching symbols connected. A new constellation node lights.",
                archivistNote: "Ability 3 of 5 demonstrated.");

            var p10_p6_gutter = MakePanel(panelDir, "p10_p6_gutter", PanelType.Interact,
                caption: "Gutter Step: enter a Gutter Door through a corrupted barrier from a distance.",
                archivistNote: "Ability 4 of 5 demonstrated.");

            var p10_p6_caplock = MakePanel(panelDir, "p10_p6_caplock", PanelType.Static,
                caption: "Capstone Lock: locked. Requires capstone placement.",
                gutter: "Ability 5 of 5 is the last act. Issue 12.",
                archivistNote: "Fifth ability locked pending E4 Capstone Placement in Issue 12.");

            var page10_6 = MakePage(pageDir, "page_10_6", PageLayout.Strip,
                new[] { p10_p6_seal, p10_p6_scan, p10_p6_circuit, p10_p6_gutter, p10_p6_caplock });

            // ── Page 7 — "Carry Constraint" (Puzzle: E2 Full) ────────────────
            var p10_p7_carry = MakePanel(panelDir, "p10_p7_carry", PanelType.Interact,
                caption: "Two-finger drag: one finger per pole. Maintain parallel grip. Navigate 7 checkpoints to the Giza approach.",
                gutter: "Carry it home.",
                archivistNote:
                    "E2 Carry Constraint (full). Narrow passages require orientation changes. " +
                    "Brief one-handed stabilize required mid-carry at Scribe corruption events. " +
                    "As the Ark approaches Giza: circuit architecture (Spiritual Lens) begins to glow.",
                revealsKeys: keyCarried != null ? new[] { keyCarried } : new KnowledgeKeyData[0]);

            var page10_7 = MakePage(pageDir, "page_10_7", PageLayout.FullBleed,
                new[] { p10_p7_carry },
                isFullBleed: true);

            // ── Page 8 — "Issue 10 Complete: The Ark Is Whole and Moving" ────
            var p10_p8_arrive = MakePanel(panelDir, "p10_p8_arrive", PanelType.Static,
                caption: "The Ark at the Giza entrance. The structure's entrance glowing. The circuit needs only two more elements: the Ark in position, and the capstone sealed.",
                archivistNote:
                    "Ark diagram fully filled in. Identity Node complete. Constellation map: T5-C lit. T5-D next: The Circuit.");

            var p10_p8_notebook = MakePanel(panelDir, "p10_p8_notebook", PanelType.Static,
                caption: "The Ark is whole. The exile is resolved. Giza is waiting.",
                gutter: "All four Ark abilities active. The fifth waits for Issue 12.",
                archivistNote: "Issue 10 complete. E2 Carry Constraint complete. D3 Identity Node fully resolved.");

            var page10_8 = MakePage(pageDir, "page_10_8", PageLayout.TwoPanel,
                new[] { p10_p8_arrive, p10_p8_notebook },
                entryTransition: PageTransition.InkDive);

            // ── Issue 10 SO ───────────────────────────────────────────────────
            var issue10 = LoadOrCreate<IssueData>("Assets/ScriptableObjects/Issues/Issue_10.asset");
            issue10.issueId              = "issue_10";
            issue10.issueNumber          = 10;
            issue10.title                = "The Artifact Page";
            issue10.arc                  = "T5 Cluster \u2014 C: Assembly";
            issue10.pages                = new[]
            {
                page10_1, page10_2, page10_3, page10_4,
                page10_5, page10_6, page10_7, page10_8
            };
            issue10.prerequisiteIssueIds = new[] { "issue_09" };
            issue10.unlocksLenses        = new LensType[0];
            issue10.unlocksKeys          = new KnowledgeKeyData[0];
            EditorUtility.SetDirty(issue10);

            Debug.Log("[Archive] Issue 10 data built.");
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

            // Ensure the directory hierarchy exists before creating the asset.
            var dir = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(dir))
            {
                // Walk from root and create any missing folders.
                var parts = dir.Split('/');
                var current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    var next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }

            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
