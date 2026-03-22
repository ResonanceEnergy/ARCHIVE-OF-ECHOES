# ROADMAP — ARCHIVE OF ECHOES: THE ANUNNAKI LEGACY

> **Fantasy Label**: This is a work of speculative myth-fantasy. No factual claims are made.

---

## Phase 0 — Foundation ✅ COMPLETE

- [x] Repo initialized
- [x] README created
- [x] Full Game Design Document (`docs/GDD.md`)
- [x] Story Bible (`docs/STORY_BIBLE.md`)
- [x] Timeline document (`docs/TIMELINE.md`)
- [x] Character Roster (`docs/CHARACTER_ROSTER.md`)
- [x] Puzzle Taxonomy (`docs/PUZZLE_TAXONOMY.md`)
- [x] UI/UX Spec (`docs/UI_UX_SPEC.md`)
- [x] Issue storyboards (`docs/issues/issue_00.md` – `issue_12.md`)

**Deliverable**: ✅ Complete design bible ready for Unity sprint

---

## Phase 1 — Vertical Slice (Issues 00–01) 🔄 IN PROGRESS

Build the minimum playable loop:

- [ ] Unity project initialized (iPhone portrait)
- [x] `Definitions.cs` — all enums (LensType, PanelType, MotifType, HapticFeedback, etc.)
- [x] ScriptableObject data model (PanelData, PageData, IssueData, LensDefinition, KnowledgeKeyData)
- [x] Core systems (ArchiveState save/load, GameManager, NarrativeState)
- [x] TouchInputManager — tap, drag, swipe, pinch, two-finger gestures
- [x] LensSystem + LensSelectorUI (radial two-finger gesture)
- [x] PanelRenderer — lens art swap, dual-lens gating, knowledge key collection
- [x] PageViewController — swipe navigation, page-complete detection
- [x] ComicController — issue load, page sequencing, issue completion
- [x] TransitionController — ink-color fade overlays per transition type
- [x] PanelEntryController — pinch zoom into micro-scene
- [x] Panel renderer (static + interactive panel types)
- [x] Page view with swipe navigation
- [x] Panel entry / exit (pinch zoom → micro-scene)
- [x] Stabilize corruption puzzle (B1)
- [x] Panel reorder puzzle (A1)
- [x] Basic branch logic
- [x] Archive notebook (ArchiveNotebook.cs — diegetic codex, pyramid silhouette)
- [x] Lens selector (radial UI — LensSelectorUI.cs)
- [x] AudioManager — drone crossfade, motifs, haptics, finale chord
- [x] TitleScreenController, Frame2100Controller, IssueCompleteController
- [x] Gutter puzzles: G1 GutterDoorPuzzle, G2 GutterWhisperTrace
- [x] UI: ConstellationMapUI, ArkInventoryUI
- [x] C-series puzzles: C1 CardinalAlignment, C2 ShaftLabyrinth, C3 Correspondence, C4 MirrorCity
- [x] D-series puzzles: D1/D2 GlyphEvolution, D2 CouncilDecree, D3 IdentityNode, D4 SemanticMerge
- [x] E-series puzzles: E1 ArkAssembly, E2 CarryConstraint, E3 CircuitCompletion, E4 CapstonePlacement
- [x] Faction MonoBehaviours: ScribeCorruptionBehaviour, ResonantProtectionBehaviour
- [x] Runtime stubs: KeyEntryWidget, LensSlot, MicroScene
- [x] Editor: ArchiveBootstrapper — folder structure, menu, build settings registration
- [x] Editor: ArchiveDataBuilder — Issue 00 & 01 ScriptableObject data assets
- [x] Editor: ArchiveSceneBuilder — all 4 Unity scenes + all prefabs, fully wired
- [ ] Issue 00: "The Find" — Unity scene wired up (run **Tools → Archive of Echoes → ⚡ BUILD EVERYTHING**)
- [ ] Issue 01: "Broken Page" — Unity scene wired up
- [ ] Art assets imported (panel images, lens overlays)
- [ ] Audio assets imported (drone clips, motif clips)

**Deliverable**: Demo slice proving core experience

---

## Phase 2 — Timeline Threads (Issues 02–07)

- [ ] Origins Thread (Issue 02)
- [ ] Cities Thread (Issue 03)
- [ ] Architect Thread (Issue 04)
- [ ] The Break (Issue 05)
- [ ] Mirror Site (Issue 06)
- [ ] The Vault (Issue 07)
- [ ] Faction system (Scribes vs Resonants pressure)
- [ ] Timeline Constellation Map UI
- [ ] Ark silhouette foreshadowing system

**Deliverable**: First half of game, full lens system, recurring symbol engine

---

## Phase 3 — Convergence Arc (Issues 08–10)

- [ ] Convergence Begins (Issue 08) — pyramid silhouette forms
- [ ] The Pyramid Page (Issue 09) — Giza dungeon unlocks
- [ ] The Artifact Page (Issue 10) — Ark assembly begins
- [ ] Ark as tool system (Seal Burn, Resonance Scan, Circuit Link)
- [ ] "Defacement" enemy mechanic
- [ ] Pyramid hub scene (persistent restoration progress)

**Deliverable**: The late-game reveal lands correctly; replayability confirmed

---

## Phase 4 — Grand Finale (Issues 11–12)

- [ ] The Circuit (Issue 11) — Djed pillar + node connection puzzle
- [ ] The Last Panel (Issue 12) — Final assembly, full-page spread, escape
- [ ] Ending variants (3 flavors)
- [ ] 2100 utopia epilogue cutscene/page
- [ ] Credits as final comic page

**Deliverable**: Complete Season 1 experience

---

## Phase 5 — Polish & Ship

- [ ] Audio design (haptics, foley, ambient, music)
- [ ] Art polish (ink textures, glow, paper grain)
- [ ] Accessibility (font size, contrast modes)
- [ ] App Store prep (screenshots, description)
- [ ] Soft launch / TestFlight

---

## Future Seasons (Expansion Arcs)

These plug directly into the existing engine without breaking the spine:

- **Season 2**: Mystery Schools Arc (initiation, knowledge carriers)
- **Season 3**: Yeshua Node (as mythic teacher — fantasy only)
- **Season 4**: Atlantis Deep Arc (civilization rise/collapse)
- **Season 5**: Modern Echo Arc (21st century belief fragmentation)

---

## Key Design Rules (Never Break These)

1. The goal is not announced at the start — it is discovered
2. All content is explicitly fantasy — the in-game disclaimer is permanent
3. Branches converge — no dead-end timelines
4. The comic medium is the mechanic — don't fight it
5. Panels complete, they don't "unlock as reward" — completion IS the reward
