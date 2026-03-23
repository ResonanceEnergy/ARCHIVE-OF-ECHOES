#!/usr/bin/env python3
"""
gen_appstore.py — Generate App Store submission metadata for Archive of Echoes.

Outputs to Assets/AppStore/:
    metadata.json           Structured app metadata (title, category, URLs …)
    store_description.txt   Full App Store long description (≤ 4000 chars)
    keywords.txt            Comma-separated keyword string (≤ 100 chars)
    screenshots_spec.json   Required screenshot sizes and scene compositions
    changelog.md            v1.0.0 release notes

Usage:
    python gen_appstore.py
    python gen_appstore.py --dry-run
"""

import argparse
import io
import json
import sys
from pathlib import Path

if sys.stdout.encoding and sys.stdout.encoding.lower() != "utf-8":
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

SCRIPT_DIR = Path(__file__).parent
OUTPUT_DIR = SCRIPT_DIR / "Assets" / "AppStore"

# ── App metadata ──────────────────────────────────────────────────────────────────

METADATA: dict = {
    "title":    "Archive of Echoes",
    "subtitle": "A gnostic graphic novel",
    "version":  "1.0.0",
    "build":    1,
    "category":    "BOOKS",
    "subcategory": "COMICS",
    "content_rating": "9+",
    "price_tier":     0,
    "support_url":      "https://resonanceenergy.io/support",
    "marketing_url":    "https://resonanceenergy.io/archive",
    "privacy_policy_url": "https://resonanceenergy.io/privacy",
    "localizations": {
        "en-US": {
            "name":     "Archive of Echoes",
            "subtitle": "A gnostic graphic novel",
            "description":       "see store_description.txt",
            "keywords":          "see keywords.txt",
            "promotional_text": (
                "Read the hidden history of consciousness — five lenses, "
                "twelve issues, one awakening.  A graphic novel unlike any other."
            ),
            "whats_new": (
                "Initial release.  Twelve issues.  Five epistemic lenses.  "
                "One capstone resolution chord."
            ),
        }
    },
    "app_icon": "Assets/Art/Cover/app_icon_1024.png",
    "rating_details": {
        "cartoon_violence":   "none",
        "realistic_violence": "none",
        "horror":             "mild",
        "mature_suggestive":  "none",
        "alcohol":            "none",
        "drugs":              "none",
    },
    "requires_network": False,
    "game_center":      False,
}

# ── Store description (≤ 4 000 chars) ────────────────────────────────────────────

STORE_DESCRIPTION = """\
ARCHIVE OF ECHOES — A Gnostic Graphic Novel

Twelve issues.  Five lenses.  One capstone truth.

Archive of Echoes is an immersive interactive graphic novel that reimagines \
the suppressed record of human consciousness.  Using five distinct epistemic \
lenses — Mythic, Technologic, Symbolic, Political, and Spiritual — you navigate \
twelve issues of layered panel sequences, uncovering a story that cannot be told \
in a single reading.

── HOW TO READ IT ──

Each issue is a full comic-format page spread.  Tap panels to unlock micro-scene \
interactions.  Long-press to stabilise corrupted data panels.  Drag panels into \
their correct narrative sequence to restore suppressed knowledge fragments.  The \
active lens filters what you see: the same panel tells a different story in Mythic \
mode than in Technologic mode.  The Gutter Entity lives between the panels.  Watch \
the white space.

── THE FIVE LENSES ──

• MYTHIC — Ancient resonance.  Symbol over fact.  The oldest grammar.
• TECHNOLOGIC — Logic, structure, the mechanical substrate of thought.
• SYMBOLIC — The inner dimension.  Archetypes.  Depth psychology.
• POLITICAL — Power, suppression, the archaeology of authority.
• SPIRITUAL — Ascension, coherence, the frequency of liberation.

Each lens unlocks a distinct Issue track and a tuned ambient drone soundtrack.  \
Complete all five tracks at the capstone to trigger the Resolution Circuit — a \
full-chord synthesis of all five root frequencies held together for the first and \
only time.

── PUZZLE MECHANICS ──

B-SERIES: Stabilise corrupted panels — long-press sequences that hold the resonance \
long enough for the panel to restore itself.

A-SERIES: Reorder panels — drag knowledge fragments back into their true sequence.  \
Five-panel truth reconstructed from twelve scattered echoes.

KNOWLEDGE KEYS: Resonance tokens embedded in decoded panels.  Each key unlocks a \
cross-reference, a definition, or an archived fragment from the deeper record.

── THE ARCHIVE'S MUSIC ──

Every lens has a tuned ambient drone — procedurally synthesised at its root \
frequency using additive synthesis with LFO modulation.  Solve puzzles to hear the \
complexity increase.  Place the capstone to hear all five frequencies resolve into \
harmony.

── WHO THIS IS FOR ──

Readers who feel the standard historical narrative is incomplete.  Anyone who has \
lingered too long on the margins of a comic page.  Students of Gnosticism, \
Hermeticism, Kabbalah, Sacred Geometry, and Conspiracy.  People who believe the \
answers are hidden in the questions.

No login required.  No ads.  No tracking.  One purchase, complete experience.

ARCHIVE OF ECHOES — v1.0.0
A Resonance Energy production.\
"""

# ── Keywords ≤ 100 chars ──────────────────────────────────────────────────────────

KEYWORDS = "graphic novel,gnostic,comic,interactive,puzzle,mystery,esoteric,resonance,history,lens"

# ── Screenshots specification ─────────────────────────────────────────────────────

SCREENSHOTS_SPEC: dict = {
    "note": (
        "Capture in iOS Simulator or on-device.  "
        "Recommended: portrait orientation unless the layout is landscape-native."
    ),
    "devices": {
        "iPhone_6_7_inch": {
            "resolution": "1290x2796",
            "required_count": 5,
            "shots": [
                {"n": 1, "scene": "Title screen — full-bleed cover art with warm glow overlay"},
                {"n": 2, "scene": "ComicReader four-panel grid — Mythic lens, Issue 00 page 1"},
                {"n": 3, "scene": "B-series stabilise — long-press progress ring active on corrupt panel"},
                {"n": 4, "scene": "A-series reorder — panels being dragged to correct sequence"},
                {"n": 5, "scene": "Issue 12 capstone — all-lens chord resolution, circuit glow effect"},
            ],
        },
        "iPhone_5_5_inch": {
            "resolution": "1242x2208",
            "required_count": 5,
            "shots": "Same scenes as 6.7-inch — re-export at 5.5-inch resolution.",
        },
        "iPad_Pro_12_9_inch": {
            "resolution": "2048x2732",
            "required_count": 5,
            "shots": [
                {"n": 1, "scene": "Title screen — archival cover art, landscape or portrait"},
                {"n": 2, "scene": "ComicReader wide grid — Technologic lens active, Issue 02 page 2"},
                {"n": 3, "scene": "Lens-switch radial overlay — all 5 lens icons, Spiritual selected"},
                {"n": 4, "scene": "Knowledge Fragment expanded — decoded panel full-screen, key glowing"},
                {"n": 5, "scene": "Issue Complete screen — score panel, keys collected, unlock animation"},
            ],
        },
    },
}

# ── Changelog ─────────────────────────────────────────────────────────────────────

CHANGELOG = """\
# Archive of Echoes — Changelog

## v1.0.0 — Initial Release

### Features
- Twelve-issue graphic novel with five epistemic lenses \
(Mythic, Technologic, Symbolic, Political, Spiritual)
- Interactive panel puzzles: B-series Stabilise and A-series Reorder
- Knowledge Key collection system (24 keys across Issues 00–11)
- Procedurally synthesised ambient drone per lens (additive synthesis + LFO)
- SFX library: panel restore, lens unlock, page flip, gutter entity,
  T5 unlock, Djed bar activation, circuit close
- Phase 5 foley: paper rustle and capstone-placed one-shot SFX
- Issue 12 Resolution Circuit: all-five-frequency capstone held chord (8 s)
- Art polish overlays: paper grain, vignette, ink scan-lines, warm glow
- Accessibility: font size multiplier (1–2×), contrast modes
  (Normal / High Contrast / Monochrome), haptics toggle, reduce-motion flag
- App Store metadata and screenshot specifications

### Technical
- Unity 6000.3.6f1, iOS target
- Procedural audio synthesis pipeline (Python stdlib only — no external libs)
- ScriptableObject-driven data model
- Pipeline automation: art → audio → Unity batchmode in one command
- Fully offline — no network required after initial download
"""

# ── Writer ────────────────────────────────────────────────────────────────────────

def _write(path: Path, content: str, label: str, dry_run: bool) -> None:
    if dry_run:
        print(f"  [dry-run] would write  {path.name}  ({len(content):,} chars — {label})")
        return
    path.write_text(content, encoding="utf-8")
    print(f"  ✓  {path.name}  ({len(content):,} chars — {label})")


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Generate App Store submission metadata for Archive of Echoes"
    )
    parser.add_argument("--dry-run", action="store_true",
                        help="Print what would be written, write nothing")
    args = parser.parse_args()

    if args.dry_run:
        print("[dry-run] No files will be written.")
    else:
        OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    _write(OUTPUT_DIR / "metadata.json",
           json.dumps(METADATA, indent=2, ensure_ascii=False),
           "app metadata", args.dry_run)

    _write(OUTPUT_DIR / "store_description.txt",
           STORE_DESCRIPTION,
           "App Store long description", args.dry_run)

    _write(OUTPUT_DIR / "keywords.txt",
           KEYWORDS,
           "App Store keywords", args.dry_run)

    _write(OUTPUT_DIR / "screenshots_spec.json",
           json.dumps(SCREENSHOTS_SPEC, indent=2, ensure_ascii=False),
           "screenshot spec", args.dry_run)

    _write(OUTPUT_DIR / "changelog.md",
           CHANGELOG,
           "changelog", args.dry_run)

    if not args.dry_run:
        desc_len = len(STORE_DESCRIPTION)
        kw_len   = len(KEYWORDS)
        print()
        print(f"  Description : {desc_len:,} chars  (App Store limit: 4 000)")
        print(f"  Keywords    : {kw_len} chars  (App Store limit: 100)")
        if desc_len > 4000:
            print("  ⚠  Description exceeds 4 000 chars — trim before submission.")
        if kw_len > 100:
            print("  ⚠  Keywords exceed 100 chars — trim before submission.")
        print(f"\n  Output: {OUTPUT_DIR}")
    else:
        print(f"\n  [dry-run] would write 5 files → {OUTPUT_DIR}")


if __name__ == "__main__":
    main()
