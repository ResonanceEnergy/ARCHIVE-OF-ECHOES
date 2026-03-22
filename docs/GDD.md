# GAME DESIGN DOCUMENT (GDD)
## ARCHIVE OF ECHOES: THE ANUNNAKI LEGACY

> **Fantasy Label**: This is a work of speculative myth-fantasy. All content is fictional. No factual or historical claims are made.

**Version**: 0.1 (Design Phase)
**Platform**: iOS (iPhone-first, portrait)
**Engine**: Unity 2D

---

## 1. Vision Statement

A player in 2100 discovers a forbidden analog comic book. The moment he touches it, he is pulled inside. Inside the comic, timelines are fractured, panels are missing, and meaning has been deliberately removed. To escape, the reader must complete the comic — panel by panel, puzzle by puzzle — until the hidden convergence is revealed: a Pyramid, an Artifact, and a final panel that closes the circuit of history and sends him home to a transformed world.

This is not a game about history. It is a game about how stories survive time, and what happens when one becomes complete.

---

## 2. Genre & Platform

| Attribute | Value |
|-----------|-------|
| Genre | Mythic puzzle / interactive comic |
| Platform | iOS (iPhone, portrait) |
| Controls | Touch-only |
| Session length | 10–30 min per session |
| Total playtime | ~8–12 hours (Season 1) |
| Art direction | Ink/paper texture, digital glow accents |
| Tone | Mystic serious with cosmic wonder |

---

## 3. Core Game Loop

```
READ → ENTER → SOLVE → UNLOCK → CONNECT → REVEAL
```

1. **READ** — observe panel: captions, speech bubbles, margin notes
2. **ENTER** — pinch/tap into a panel (zoom to micro-scene)
3. **SOLVE** — complete a puzzle within the panel
4. **UNLOCK** — panel state changes: Corrupted → Resolved, Hidden → Visible
5. **CONNECT** — resolved panel links to adjacent panels / cross-timeline echoes
6. **REVEAL** — page meaning clarifies; new pages / branches open

The loop repeats until an issue is complete. Issues unlock the next shard of the comic.

---

## 4. Core Player Verbs

| Verb | Action |
|------|--------|
| READ | Absorb text, captions, margin notes |
| ENTER | Zoom into a panel (pinch / tap) |
| MANIPULATE | Drag, rotate, connect, reorder panels |
| CHOOSE | Branch paths, switch lenses, pick interpretations |
| STABILIZE | Repair corrupted panels (long-press / swipe) |
| ANCHOR | Lock a resolved panel into the archive |

---

## 5. Panel System

### 5.1 Panel Types

| Type | Description | Interaction |
|------|-------------|-------------|
| Static | Read-only story beat | Tap to advance |
| Interact | Contains manipulable objects | Drag / tap targets |
| Gate | Requires condition to open | Solve prerequisite first |
| Branch | Presents a fork | Choose — consequences follow |
| Corrupted | Scrambled meaning | Stabilize to restore |
| Hidden | Not yet visible | Revealed via scan / gutter action |
| Gutter | Exists in the space between panels | Swipe/hold to access |

### 5.2 Panel States

| State | Visual | Meaning |
|-------|--------|---------|
| Dormant | Faded, low saturation | Visible but incomplete |
| Locked | Dark overlay, padlock glyph | Inaccessible — needs key |
| Torn | Ragged edges, missing fragments | Historical interruption |
| Corrupted | Glitch distortion, static | Conflicting / suppressed belief |
| Hidden | Invisible until triggered | Forgotten / never recorded |
| Resolved | Full color, clean borders | Integrated into the story |

### 5.3 Panel Physics (Gameplay Rules)

- Panels can be **reordered** on a page (drag)
- Panels can be **rotated** (two-finger rotate — alignment puzzles)
- Panels can be **merged** or **split** (semantic puzzle types)
- Panels can be **stacked as overlays** (timeline convergence puzzles)
- The **gutter** between panels is traversable (Gutter Step ability — late game)

---

## 6. Branching System

Branching is controlled at three scales to prevent scope explosion:

### Scale 1 — Panel Branch (Micro)
- Affects next 1–3 panels
- Small, frequent, low-stakes
- Example: "Add Essence" vs "Refuse" changes which objects appear in the next panel

### Scale 2 — Page Branch (Meso)
- Affects the layout or available panels on the current page
- Medium consequence
- Example: choosing Mythic vs Technologic lens changes what's interactive

### Scale 3 — Issue Branch (Macro)
- Affects which optional chapters unlock, faction reputation, ending flavor
- Does NOT change the win condition — only the path and earned meaning

**Convergence Rule**: All branches converge toward the finale. The Pyramid + Ark is the only ending. The ending's flavor (which of 3 variants) is determined by cumulative branch choices.

---

## 7. Interpretation Lens System

Every panel/scene is viewable through 5 lenses. Switching lens changes:
- Which objects/interactables are visible
- Puzzle rules
- NPC/caption dialogue variant
- Which branch options appear

| Lens | Theme | Example Effect |
|------|-------|----------------|
| Mythic | Gods, rituals, cosmic beings | Divine objects glow; priest NPCs speak in metaphor |
| Technologic | Machines, circuits, science | Sacred objects become schematics; puzzles become engineering |
| Symbolic | Psychology, archetypes | Characters = aspects of mind; puzzles = inner journey |
| Political | Power, control, bloodlines | Hierarchy is visible; control mechanics unlock |
| Spiritual | Consciousness, ascension | Hidden layers appear; Gutter speaks more often |

**Lens Unlock Progression**:
- Issues 00–01: No lens (raw experience)
- Issue 04: Lens system introduced (Mythic + Technologic first)
- Issue 06: All 5 lenses unlocked

---

## 8. Progression System

No XP. No kill counts. No loot grinding.

Progression is measured by:

| Track | What It Represents |
|-------|--------------------|
| Panel Completion % | How much of the comic exists / is legible |
| Issue Completion | Major story milestones |
| Knowledge Keys | Earned by solving cross-timeline puzzles |
| Lens Unlocks | Earned by completing specific issue arcs |
| Ark Components | Collected in late game (Issues 10–12) |

### Knowledge Keys (Examples)

| Key | How Earned | What It Opens |
|-----|------------|---------------|
| Alignment Key | Solve cardinal geometry puzzle | Giza shaft doors |
| Mirror Key | Complete mirror-city puzzle | Cross-timeline overlays |
| Translation Key | Glyph evolution puzzle | Hidden ink panels |
| Stability Key | Djed pillar puzzle | Ark seat access |
| Artifact Key | Full Ark assembly | Final circuit puzzle |

---

## 9. The Ark as a Game System (Late Game)

First referenced as a faint silhouette in Issue 07. Named and assembled in Issues 10–11.

### Ark States

| State | When | Effect |
|-------|------|--------|
| Dormant | Before Issue 07 | Not present |
| Silhouetted | Issues 07–09 | Negative space only; hints |
| Assembled | Issue 10 | Becomes player tool |
| Activated | Issue 11 | Full ability set unlocked |
| Installed | Issue 12 | WIN condition trigger |

### Ark Abilities (Unlocked as Assembled)

| Ability | Mechanic |
|---------|----------|
| Seal Burn | Removes corrupted locks and censor seals |
| Resonance Scan | Reveals hidden ink, suppressed panels, gutter doors |
| Circuit Link | Connects nodes across panels / pages |
| Gutter Step | Move between panels through gutter-space |
| Capstone Lock | Final ability — locks the convergence page into place |

---

## 10. Factions

Factions apply pressure throughout the game but never block the critical path:

### The Scribes
- Want one locked interpretation
- Mechanically: erase panels, simplify pages, block branch paths
- Counter: Ark Seal Burn restores them; Gutter routes bypass them

### The Resonants
- Want all interpretations preserved
- Mechanically: reveal hidden panels, amplify Gutter whispers, unlock rare branches
- Counter: can over-complicate pages (too much noise)

### The Gutter (Neutral / Trickster)
- Entity of the space between panels
- Offers shortcuts at a cost (increased corruption elsewhere)
- Becomes a guide in the final issue

---

## 11. The Grand Reveal (Design Engineering)

The Pyramid + Ark must feel **discovered**, not announced.

**Seeding Schedule**:

| Issue | Seed Planted |
|-------|-------------|
| 02 | Recurring 4-bar symbol (Djed echo) |
| 03 | Triangle geometry appears across culture panels |
| 04 | "Blueprint" language introduced (Architect) |
| 05 | Panels with void-shaped negative space |
| 06 | Mirror-city layout echoes specific proportions |
| 07 | A locked "door" inside a narrow passage |
| 07 | Blank void shaped exactly like the missing object |
| 08 | Timeline constellation resolves into pyramid silhouette |
| 08 | Archivist: *"Something was removed to prevent completion."* |
| 09 | "GIZA" label appears for the first time — as a discovered in-game label |

---

## 12. Endings (Season 1)

All three endings require: capstone placed + Ark installed + circuit closed.

What differs is the type of utopia:

| Ending | How Earned | Result |
|--------|-----------|--------|
| Locked Stability | High Scribes influence, high Certainty trait | Ordered world; stable but rigid |
| Resonant Harmony | High Resonants influence, high Wonder trait | Creative utopia; ambiguity preserved |
| Gutter Path | Frequent Gutter shortcuts, high Liberation trait | Humanity self-governs without mythic scaffolding |

**Final line (all endings)**:
> *"Completion does not mean certainty. It means the story can continue."*

---

## 13. Player Resonance Traits

Instead of good/evil alignment:

| Trait | What It Tracks |
|-------|---------------|
| Wonder | Openness to multiple interpretations |
| Certainty | Drive toward single answers |
| Control | Preference for consolidation / authority |
| Liberation | Drive toward autonomy / ambiguity |

Traits shift based on:
- Branch choices
- Lens selections
- Gutter usage
- Faction interactions

---

## 14. Content Safety

- All mythic/historical content is framed as **in-universe fantasy Archive Shards**
- First-launch disclaimer is permanent and visual: *"Fantasy / speculative myth-fiction; no factual claims."*
- No religion or culture is mocked, dismissed, or falsely validated
- Contradictions between belief systems are intentional design features, not errors
- The Archive treats all content as **signal** (what humans believed/feared), not **truth**
