# UI/UX SPECIFICATION — ARCHIVE OF ECHOES: THE ANUNNAKI LEGACY

> iPhone-first, portrait mode, touch-only. Premium. Minimal. Comic-native.

---

## 1. Design Philosophy

**The UI should feel like reading.**

Not playing a menu. Not navigating an app. Reading a very old, very alive book.

Rules:
- HUD never covers art
- All UI lives in margins, gutters, and overlays — not floating over panels
- Transitions feel like ink and paper, not digital slides
- Sound and haptics are integral to UI feedback

---

## 2. Screen Flow

```
LAUNCH
  ↓
DISCLAIMER (Fantasy label — full screen, skippable after read)
  ↓
TITLE PAGE (comic cover — tap to open)
  ↓
SAVE SELECT (if returning player — presented as "chapter bookmark")
  ↓
2100 SCENE (exploration: find the comic)
  ↓
PICK UP COMIC → PULL-IN SEQUENCE (non-interactive cinematic)
  ↓
COMIC READER MODE (default in-game state)
  ↓  ↑ exit via "eject" (unlock late game)
PANEL ENTRY MODE (zoom into a panel)
  ↓  ↑
PUZZLE OVERLAY (lightweight, fits within panel frame)
  ↓
PANEL RESOLVE ANIMATION
  ↓
RETURN TO PAGE VIEW
  ↓
ISSUE COMPLETE PAGE → continue / return to Archive
  ↓
RETURN TO 2100 (inter-issue frame)
  ↓
NEXT ISSUE or HUB NAVIGATION
```

---

## 3. Core Screens

---

### 3.1 DISCLAIMER SCREEN

- **Purpose**: Fantasy label. Platform protection. Legal clarity.
- **Appears**: On first launch only; accessible via settings always
- **Layout**: Full black screen. White text. Comic panel border around the text.
- **Text**:
  > *"ARCHIVE OF ECHOES is a work of speculative myth-fantasy.*
  > *All narratives, characters, and events are fictional.*
  > *Ancient myths and historical motifs are used as creative ingredients only.*
  > *No factual, historical, or scientific claims are made.*
  > *The Archive presents what was believed — not what was true."*
- **Continue**: Tap to acknowledge. No "skip" on first launch.
- **Visual tone**: Aged paper texture. Single candle illustration (or lantern). No animation.

---

### 3.2 TITLE PAGE (Comic Cover)

- **Layout**: Full bleed portrait comic cover art
- **Title**: "ARCHIVE OF ECHOES" — large, distressed serif font
- **Subtitle**: "THE ANUNNAKI LEGACY" — smaller, with a "FANTASY" badge
- **Interaction**: Tap anywhere to "open" the comic
- **Animation**: Pages fan open; the cover curls back (paper physics simulation)
- **Sound**: Paper rustle; deep bass note

---

### 3.3 2100 OPENING SCENE

- **Format**: NOT a standard game scene — it's formatted like a comic page
- **Layout**: 4–6 panels showing the protagonist discovering the Archive, finding the forgotten sector, finding the comic
- **Interaction**: Tap to advance each panel (static → static → enter the last panel)
- **Visual style**: Desaturated (2100 world is hollow; color is muted)
- **Contrast**: The comic, when found, has the *only* full color in the scene

---

### 3.4 COMIC READER MODE (Primary Game State)

**Layout**:
```
┌─────────────────────────┐
│  [Title top bar]        │  ← Issue name; minimal; semi-transparent
│                         │
│  ┌───────┬──────────┐   │
│  │PANEL 1│ PANEL 2  │   │
│  ├───────┴──────────┤   │
│  │    PANEL 3       │   │
│  ├──────────┬───────┤   │
│  │ PANEL 4  │PANEL 5│   │
│  └──────────┴───────┘   │
│                         │
│  [progress dots]        │  ← Page progress indicator (bottom)
└─────────────────────────┘
```

**Active UI elements**:
- Top bar: Issue name + lens icon (top right)
- Bottom: Page progress dots (not panel count — page progress)
- Side margin (right): Archivist annotations appear here (small, dismissable)
- Gutter (between panels): Gutter text appears between panels, in the whitespace

**What is NOT in the UI**:
- Health bars
- Score
- Inventory (until Issue 09 unlocks it)
- Minimap

---

### 3.5 PANEL ENTRY MODE

**Triggered by**: Pinch into a panel OR tap on an interactive panel

**Transition animation**: The panel "breathes" outward → the background desaturates → the panel fills the screen

**Layout**: Panel fills screen fully. Minimal UI.
```
┌─────────────────────────┐
│  [← back icon]          │  ← Barely visible; top-left; appears on long-press
│                         │
│                         │
│    [PANEL ART FILLS]    │
│    [PUZZLE OVERLAY]     │
│                         │
│                         │
│  [Archivist note]       │  ← Appears at bottom if relevant
└─────────────────────────┘
```

**Exit**: Pinch-out to return to Page View, or complete the puzzle (auto-exit on resolve)

---

### 3.6 PUZZLE OVERLAY

**Appears within Panel Entry Mode** — doesn't cover the full screen

**Principles**:
- Puzzle controls appear *inside* the panel's art, not on top of it
- Object manipulation feels like touching actual elements of the scene
- Completion animation is visual (ink solidifies, glow settles) not a pop-up

**Feedback layer**:
- Correct: soft color fill + "ink snap" haptic
- Wrong: panel edges redden slightly, subtle buzz haptic
- Near-correct: warm amber underline on relevant element

---

### 3.7 LENS SELECTOR

**Triggered by**: Lens icon (top right of Reader Mode) or swipe-down with two fingers

**Layout**: Radial dial expands from the lens icon

```
          [Mythic]
   [Spiritual]   [Technologic]
   [Political]   [Symbolic]
          [close]
```

**Behavior**:
- Current lens highlighted
- Unavailable lenses grayed out with a lock icon
- Selecting a lens: smooth visual transition (panels shift color/style)
- Switching lens mid-puzzle: allowed; resets puzzle state for lens-sensitive puzzles

**Visual character per lens**:
| Lens | Panel Color Shift | Texture |
|------|------------------|---------|
| Mythic | Warm gold + deep blue | Stars/divine glow |
| Technologic | Cool grey + neon green trace | Schematics/circuit lines |
| Symbolic | Soft purple + watercolor | Abstract forms |
| Political | High contrast black/white | Sharp geometric borders |
| Spiritual | Soft white + diffuse light | Dissolving edges |

---

### 3.8 ARCHIVE NOTEBOOK (Diegetic Codex)

**What it is**: An in-world sketchbook that acts as the player's codex and progress log. Never presented as a game menu.

**Triggered by**: Swipe-up from bottom of Reader Mode screen

**Contents automatically populated by**:
- Resolved panels (thumbnail + a 1-line interpretation)
- Discovered symbols (auto-drawn as the player encounters them)
- Knowledge Keys earned
- Connections between timelines (drawn as lines between thumbnails when overlap is discovered)

**Visual design**: Kraft paper background. Blue ink sketches. Archivist type. Gutter whisper text in a lighter, italic hand.

**The "detective board" moment**: When enough panels connect in Issue 08, the Notebook's connection lines spontaneously form a pyramid silhouette. This is the reveal moment. No text explains it.

---

### 3.9 TIMELINE CONSTELLATION MAP

**What it is**: A visual map of all timeline eras, showing which have been visited and which panels are resolved.

**Triggered by**: Tap on constellation icon (replaces lens icon at inter-issue screens)

**Layout**: Dark field with constellation-like nodes connected by fine lines
- Unvisited eras: faint outline stars
- Visited, incomplete: dim glow
- Fully visited: bright star
- Lines between connected timelines appear as discoveries are made
- Late game (Issue 08+): constellation shape resolves into pyramid outline

**Not available during active puzzles** — only accessible at inter-issue or page-complete moments

---

### 3.10 ARK INVENTORY (Late Game — Unlocks Issue 09)

**What it is**: A panel in the Archive Notebook that shows Ark component status

**Layout**: Outline diagram of the Ark. Components that have been recovered appear as filled-in sections. Missing components appear as white space.

**Triggered by**: Tap on Ark icon that appears in Notebook margin after first Ark void is discovered

**Ability indicators**: Icons for Seal Burn, Scan, Circuit Link, Gutter Step, Capstone Lock appear beneath the Ark diagram. Gray = locked. Gold = available.

---

## 4. Gesture Map (Complete)

| Gesture | Context | Action |
|---------|---------|--------|
| Tap | Panel in Reader Mode | Focus / advance static panel |
| Double-tap | Panel | Quick inspect (show Archivist note) |
| Long-press | Panel | Stabilize (enters stabilize puzzle mode) |
| Pinch-in | Panel | Enter Panel Mode (zoom into scene) |
| Pinch-out | Panel Entry Mode | Exit back to Page View |
| Swipe left | Page View | Next page |
| Swipe right | Page View | Previous page |
| Swipe up | Page View | Archive Notebook |
| Swipe down | Page View | Open timeline map (inter-issue only) |
| Two-finger swipe down | Page View | Lens selector |
| Two-finger rotate | Panel Entry Mode | Rotate panel / element |
| Two-finger tap-drag | Carry Constraint puzzle | Move Ark via poles |
| Drag | Panel/object in puzzle | Move element |
| Drag (panel borders) | Reader Mode | Reorder panels (layout puzzles) |
| Swipe along gutter edge | Reader Mode | Gutter Door scan |
| Trace | Ink Restore puzzle | Draw missing linework |
| Spread (panels) | Panel Merge/Split | Split panel |
| Collapse (panels) | Panel Merge/Split | Merge panels |
| Phone tilt (opt-in only) | Capstone Placement | Elevation alignment axis |

---

## 5. Transitions & Animations

### Page Turn
- Paper curl physics (right- or left-weighted based on swipe direction)
- Sound: softly layered paper + slight wind
- Duration: 0.3s

### Panel Entry (Zoom-In)
- "Ink dive" — camera sinks into panel; grain texture fills frame
- Background desaturates during dive
- Duration: 0.4s
- Sound: low bass pulse + grain texture crackle

### Panel Exit (Zoom-Out)
- Reverse ink dive
- Page fades back to full contrast
- Duration: 0.3s

### Lens Switch
- Page recolors via a soft wave from center-out
- Interactables phase in/out depending on new lens
- Duration: 0.5s
- Sound: subtle tone shift (each lens has a distinct key)

### Panel Resolve (Completion)
- Panel borders sharpen and glow briefly
- Art saturates fully if previously desaturated
- A fine ink line traces the panel border from corner to corner
- Duration: 0.6s
- Sound: "ink snap" — short, satisfying paper-tension release + single clean note
- Haptic: medium firm pulse

### Gutter Crossing
- Subtle tick haptic as player's interaction crosses panel borders
- Sound: faint crease

### Corruption Increase
- Page edges darken
- Affected panels start a slow, low pulse
- Sound: subsonic flutter

### Gutter Entity Speaks
- Text fades in between panels (no animation — just appears)
- Slightly skewed text alignment
- Sound: whisper (binaural stereo drift — left/right mic pan)
- Haptic: very light, long, slow pulse

---

## 6. Typography System

| Usage | Font Style | Notes |
|-------|-----------|-------|
| Panel captions | Serif, aged | Comic caption box corners |
| Speech/thought | Hand-lettered style | Interior speech bubbles |
| Archivist | Clean sans-serif | Margin annotations only |
| Gutter | Italic, slightly ragged | No box — floats between panels |
| UI labels | Minimal sans | Ultra-small weight |
| Issue title | Distressed serif | Cover + top bar |

**Accessibility**:
- Font scaling: UI respects iOS Dynamic Type for caption/annotation text
- Minimum readable size: 12pt at default scale
- High contrast mode: available in settings (increases border visibility, reduces texture effects)

---

## 7. Audio Architecture

### Spatial Design
- Diagetic sounds (panel interactions) come from screen position (left panels → left ear, etc.)
- Gutter whispers drift in stereo (never center)
- Archivist speaks from center

### Sound Motifs

| Event | Sound |
|-------|-------|
| Panel entry | Low bass pulse + grain crackle |
| Panel resolve | Ink snap + single clean chime |
| Corruption increase | Sub flutter (felt more than heard) |
| Gutter whisper | Breathy whisper + binaural pan |
| Lens switch | Tonal shift (each lens has a root note) |
| Page turn | Paper + soft wind |
| Issue complete | Multi-note swell (builds as the page completes) |
| Final issue complete | Full chord resolution — then silence |

### Lens Root Notes
| Lens | Root Note |
|------|-----------|
| Mythic | D minor |
| Technologic | F# minor |
| Symbolic | A minor |
| Political | C minor |
| Spiritual | E major |

### Haptics
| Event | Haptic Type |
|-------|-------------|
| Panel resolve | Medium firm pulse |
| Corruption | Soft long pulse (warning) |
| Gutter crossing | Light micro-tick |
| Panel entry | Heavy single thump |
| Puzzle correct step | Soft click |
| Puzzle wrong | Double buzz |
| Ark ability used | Sharp medium pulse |
| Final completion (Issue 12) | Long slow pulse rising to full intensity, then full stop |

---

## 8. Accessibility

| Feature | Implementation |
|---------|---------------|
| Font scaling | Dynamic Type support |
| Color modes | High contrast option (reduces texture effects) |
| Haptic toggle | Separate haptic/sound toggles |
| Puzzle time limits | Extended mode option (disables time pressure on any timed puzzle) |
| Dyslexia-friendly font | Optional font substitute for caption/annotation text |
| Colorblind mode | Panel lens indicators use shape + color (never color alone) |
