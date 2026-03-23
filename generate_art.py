#!/usr/bin/env python3
"""
generate_art.py — Archive of Echoes art generation

Calls the xAI (Grok) image generation API to produce every visual asset for the
vertical slice: 23 comic panels, 5 lens icons, 6 knowledge-key icons, 2 issue
covers, and 4 UI textures (40 assets total).

Reads GROK_API_KEY from:
    C:\\dev\\DIGITAL LABOUR\\DIGITAL LABOUR\\.env

Usage:
    python generate_art.py                            # generate all missing
    python generate_art.py --dry-run                  # preview, no API calls
    python generate_art.py --force                    # overwrite all existing
    python generate_art.py --category panels          # only panel images
    python generate_art.py --category lenses          # only lens icons
    python generate_art.py --category keys            # only knowledge-key icons
    python generate_art.py --category covers          # only issue covers
    python generate_art.py --category ui              # only UI textures
    python generate_art.py --id p00_p3_splash         # single asset by id
    python generate_art.py --list                     # list all asset ids

Requirements:
    pip install openai
"""

import argparse
import io
import json
import os
import sys
import time
import urllib.request
from pathlib import Path

# ── Windows cp1252 fix ─────────────────────────────────────────────────────────
if sys.stdout.encoding and sys.stdout.encoding.lower() != "utf-8":
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
if sys.stderr.encoding and sys.stderr.encoding.lower() != "utf-8":
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8", errors="replace")

# ── Configuration ──────────────────────────────────────────────────────────────

PROJECT_ROOT = Path(__file__).parent
ENV_FILE     = Path(r"C:\dev\DIGITAL LABOUR\DIGITAL LABOUR\.env")
MANIFEST     = PROJECT_ROOT / "Assets" / "Art" / "art_manifest.json"

XAI_BASE_URL  = "https://api.x.ai/v1"
IMAGE_MODEL   = "grok-imagine-image"  # xAI image generation model
REQUEST_DELAY = 2.5             # seconds between requests (respect rate limits)

# ── Art style fragments ────────────────────────────────────────────────────────

# Shared comic-panel look: ink + paper texture, selective neon glow
_PANEL = (
    "highly detailed semi-realistic digital comic book illustration, "
    "painterly graphic novel style with bold clean lines and dramatic cinematic lighting, "
    "strong warm golden accent highlights from torchlight, high contrast and sharp details, "
    "expressive face with dramatic eyes, ornate ancient Egyptian regalia with realistic linen robes, "
    "leather straps, gold jewelry, lapis lazuli inlays and fabric textures, "
    "dark ancient temple interior background with carved hieroglyph walls and mystical glowing particles, "
    "Anunnaki speculative mythology, premium comic book game art style, "
    "no text, no word balloons, no panel borders"
)

# Icon style: semi-realistic Egyptian occult symbol
_ICON = (
    "highly detailed semi-realistic digital illustration, painterly graphic novel style, "
    "single ancient Egyptian occult symbol centered on very dark temple background, "
    "intense glowing golden and turquoise magical energy, lapis lazuli inlays, "
    "carved hieroglyph stone texture, mystical glowing particles, "
    "premium comic book game art style, no text, no letters"
)

# Cover style: semi-realistic Egyptian game cover art
_COVER = (
    "highly detailed semi-realistic digital comic book illustration, portrait orientation, "
    "painterly graphic novel style with bold clean lines and dramatic cinematic lighting, "
    "ancient Egyptian Anunnaki mythology, dynamic commanding pose, "
    "intense glowing golden and turquoise magical hieroglyphic tablet, "
    "strong warm golden accent highlights from torchlight, high contrast and sharp details, "
    "ornate ancient Egyptian regalia with realistic linen robes, leather straps, "
    "gold jewelry, lapis lazuli inlays and fabric textures, "
    "dark ancient temple interior with carved hieroglyph walls and mystical glowing particles, "
    "premium comic book game art style, no title text, no speech bubbles"
)

def _p(desc: str) -> str:
    """Panel prompt: scene description + shared panel style."""
    return f"{desc}, {_PANEL}"

def _icon(symbol: str, glow: str) -> str:
    """Icon prompt: symbol description + icon style + glow color."""
    return f"{symbol}, {glow} glow, {_ICON}"

def _cover(desc: str) -> str:
    return f"{desc}, {_COVER}"

# ── Asset catalogue ────────────────────────────────────────────────────────────
# Each entry:
#   id       — unique identifier (used for manifest and --id filter)
#   path     — output path relative to PROJECT_ROOT
#   category — panels | lenses | keys | covers | ui
#   size     — "1024x1024" (square) or "1024x1792" (tall portrait splash)
#   prompt   — full image generation prompt

ASSETS = [

    # ════════════════════════════════════════════════════════════════════════
    # ISSUE 00 — "The Find"
    # ════════════════════════════════════════════════════════════════════════

    # Page 1 — The Assignment ─────────────────────────────────────────────────
    {
        "id": "p00_p1_exterior",
        "path": "Assets/Art/Panels/Issue00/p00_p1_exterior.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "brutalist concrete archive building exterior in dystopian 2100 megacity, "
            "lone archivist trainee in uniform entering through heavy doors, "
            "towering corporate monoliths behind, surveillance drones overhead, "
            "demolition warning signs plastered on concrete, overcast milky sky, fog, "
            "low angle establishing shot"
        ),
    },
    {
        "id": "p00_p1_interior",
        "path": "Assets/Art/Panels/Issue00/p00_p1_interior.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "vast dim archive interior, endless rows of metal shelving packed with labeled analog media, "
            "VHS tapes, film reels, magnetic tape reels, all tagged and catalogued, "
            "yellow caution preservation light overhead, dust motes in beam, "
            "archivist figure small amid towering shelves, quiet institutional desolation"
        ),
    },
    {
        "id": "p00_p1_hands",
        "path": "Assets/Art/Panels/Issue00/p00_p1_hands.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "close-up of archival gloved hands skimming steadily along shelf edge, "
            "every item neatly labeled with institutional tags, methodical inventory routine, "
            "hands move with practised certainty, crisp institutional light from above, "
            "ordered and controlled"
        ),
    },
    {
        "id": "p00_p1_slot",
        "path": "Assets/Art/Panels/Issue00/p00_p1_slot.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "a single empty shelving slot with no label tag, "
            "soft warm golden shimmer emanating from within the slot's darkness, "
            "all surrounding items neatly labeled and catalogued, "
            "this one slot wrong, quiet and significant, close-up composition"
        ),
    },
    {
        "id": "p00_p1_taptutorial",
        "path": "Assets/Art/Panels/Issue00/p00_p1_taptutorial.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "glowing unlabeled archive slot pulsing gently with warm gold light, "
            "archivist's gloved hand hovering just above it, drawn to it, hesitant, "
            "the slot radiates something alive and aware, close-up atmospheric detail, "
            "the light pulses like breathing"
        ),
    },

    # Page 2 — The Object ─────────────────────────────────────────────────────
    {
        "id": "p00_p2_comic",
        "path": "Assets/Art/Panels/Issue00/p00_p2_comic.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": (
            "a single analog comic book on a neutral surface, "
            "the comic is in FULL VIVID COLOR — its cover shows an ancient pyramid bathed in golden cosmic rays, "
            "while the entire world around it is completely desaturated greyscale, "
            "the color contrast is the entire point of the image, "
            "the comic cover title reads 'ARCHIVE OF ECHOES', warm saturated mystic color emanating from the comic, "
            "ink washed illustration style, hyper-focused composition, only one object has color in the world"
        ),
    },
    {
        "id": "p00_p2_corrupted",
        "path": "Assets/Art/Panels/Issue00/p00_p2_corrupted.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "a comic book panel showing signs of corruption and distortion, "
            "digital glitch overlay — scan lines, chromatic aberration, pixel tearing — "
            "applied over an ink illustration of an ancient artifact or cosmic being, "
            "the image beneath struggles through the corruption, barely readable, "
            "corrupted data aesthetic fused with old ink printing, deeply unsettling"
        ),
    },
    {
        "id": "p00_p2_eyes",
        "path": "Assets/Art/Panels/Issue00/p00_p2_eyes.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "extreme close-up of an ancient divine being's eyes on a printed comic page, "
            "looking directly out of the panel at the viewer, breaking the fourth wall, "
            "large almond-shaped cosmic eyes with luminous golden iris, "
            "Anunnaki deity stylization, massive presence, "
            "deeply unsettling intimate gaze, as if the being has noticed you"
        ),
    },

    # Page 3 — The Pull ───────────────────────────────────────────────────────
    {
        "id": "p00_p3_opens",
        "path": "Assets/Art/Panels/Issue00/p00_p3_opens.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "archivist character carefully opening mysterious comic book, hands parting the pages, "
            "the first interior panel of the comic glows with inner warm light, "
            "the light source is within the page itself, not external, "
            "rest of archive room in darkness, the glow is golden and alive, "
            "threshold moment, intimate and portentous"
        ),
    },
    {
        "id": "p00_p3_pinch",
        "path": "Assets/Art/Panels/Issue00/p00_p3_pinch.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "extreme close-up of open comic book spread, interior panel pulsing with dimensional golden light, "
            "reader's fingers approaching in a pinch gesture, the panel's surface like still liquid about to be disturbed, "
            "the panel appears three-dimensional as if it contains real depth, "
            "portrait of the moment before entering, intimate and electric"
        ),
    },
    {
        "id": "p00_p3_splash",
        "path": "Assets/Art/Panels/Issue00/p00_p3_splash.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "full-page portrait splash: archivist figure tumbling through a majestic vortex of comic panels, "
            "fragments of twelve historical ages swirl past — ancient ziggurats, cosmic star maps, "
            "alien stone glyphs, Egyptian monuments, circuit boards, pyramidal structures, "
            "all incomplete impressionistic fragments, "
            "the 2100 archive world visible as a tiny postage-stamp in one corner shrinking to nothing, "
            "overwhelming cosmic composition, falling into the archive, gold and ink"
        ),
    },

    # ════════════════════════════════════════════════════════════════════════
    # ISSUE 01 — "Broken Page"
    # ════════════════════════════════════════════════════════════════════════

    # Page 1 — The Gutter Between ─────────────────────────────────────────────
    {
        "id": "p01_p1_darkness",
        "path": "Assets/Art/Panels/Issue01/p01_p1_darkness.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "near-total absolute darkness, a single small human figure suspended in featureless void, "
            "barely visible outline, no floor, no ceiling, no walls, "
            "pure panel-space liminal emptiness, existential quiet, "
            "a breath of light from absolutely nowhere illuminates nothing useful"
        ),
    },
    {
        "id": "p01_p1_fragments",
        "path": "Assets/Art/Panels/Issue01/p01_p1_fragments.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "torn comic panel fragments drifting slowly through ink void, "
            "glimpses of ancient Mesopotamian cities with ziggurats, star maps, cosmic beings, "
            "none of the fragments coherent by themselves, dreamlike impossible memory collage, "
            "floating scraps of visual history dissolving at the edges, "
            "soft gold and teal glows on torn panel fragment rims"
        ),
    },
    {
        "id": "p01_p1_voice",
        "path": "Assets/Art/Panels/Issue01/p01_p1_voice.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "an invisible AI voice speaking in the liminal void, "
            "cascading holographic text lines in panel-space darkness, "
            "no physical body — only floating words and data streams, "
            "digital oracle aesthetic, archival margin note typography style, "
            "gold text dissolving into void, presence without form"
        ),
    },
    {
        "id": "p01_p1_fragment",
        "path": "Assets/Art/Panels/Issue01/p01_p1_fragment.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "a stabilized panel fragment showing a detailed ancient Mesopotamian landscape, "
            "rolling plains with ziggurat temples and distant mountains, "
            "the fragment is rendered UPSIDE DOWN — the sky is at the bottom, landscape at top, "
            "amber warning glow around fragment edges indicating something is wrong with it, "
            "the image is intact but oriented incorrectly, needs to be fixed"
        ),
    },

    # Page 2 — Reorder ────────────────────────────────────────────────────────
    {
        "id": "p01_p2_reorder",
        "path": "Assets/Art/Panels/Issue01/p01_p2_reorder.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "five distinct astronomy sequence panels arranged in scrambled wrong order like a puzzle, "
            "each panel shows: a vast star-filled night sky, the same sky at early dawn stars fading, "
            "a lone figure looking up at dark sky from behind, the same figure walking away across empty plain, "
            "an empty bare horizon at midday, "
            "panels are floating and misarranged, amber glows where placement is wrong, "
            "a cosmic narrative sequence made into a drag-and-rearrange puzzle"
        ),
    },

    # Page 3 — The Gutter Speaks ──────────────────────────────────────────────
    {
        "id": "p01_p3_restored",
        "path": "Assets/Art/Panels/Issue01/p01_p3_restored.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "a completed restored comic page floats behind a small player-character figure like a luminous window into landscape, "
            "the page shows the astronomy dawn sequence now in correct order, glowing softly, "
            "archivist figure in foreground facing the completed page, "
            "sense of quiet achievement and strange wonder, ink and golden light"
        ),
    },
    {
        "id": "p01_p3_solid",
        "path": "Assets/Art/Panels/Issue01/p01_p3_solid.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "the interior of a comic becomes physically real space, "
            "towering walls made entirely of stacked comic pages and panels extend upward out of frame, "
            "multiple dim paths diverge between the page-walls into deeper darkness, "
            "a labyrinthine library made of narrative, mysterious corridors of ink, "
            "the reader stands at the junction of paths"
        ),
    },
    {
        "id": "p01_p3_gutterdoor",
        "path": "Assets/Art/Panels/Issue01/p01_p3_gutterdoor.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "extreme close-up of the white gutter whitespace between two printed comic panels, "
            "a tiny faint archaic door or portal symbol barely visible within the white gutter space, "
            "the white space shimmers slightly as if alive and aware, "
            "the symbol is almost imperceptible — blink and miss, "
            "the gutter speaks through suggestion, not declaration"
        ),
    },
    {
        "id": "p01_p3_pyramid",
        "path": "Assets/Art/Panels/Issue01/p01_p3_pyramid.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "a fleeting impossible vision: an ancient pyramid monument in pure gold against black, "
            "the image already dissolving at the edges as if evaporating, "
            "ghostly impression, already leaving, the visual equivalent of a suppressed memory, "
            "overwhelming significance in a single dissolving shape, "
            "gold pyramid dissolving into black ink"
        ),
    },

    # Page 4 — The First Branch ───────────────────────────────────────────────
    {
        "id": "p01_p4_t1",
        "path": "Assets/Art/Panels/Issue01/p01_p4_t1.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "warm golden light radiating through a gap between stacked comic pages, "
            "through the opening: ancient stars and celestial spiral symbols, "
            "cuneiform constellations glowing, divine Anunnaki cosmic scene just beyond the threshold, "
            "the oldest origin point, mythology made into light, "
            "gold and amber warmth, inviting and ancient, the left path"
        ),
    },
    {
        "id": "p01_p4_t2",
        "path": "Assets/Art/Panels/Issue01/p01_p4_t2.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "cool geometric blue-white light through a gap between comic pages, "
            "through the opening: patterns of ancient city roads and wall plans seen from above, "
            "urban grid geometry of the first cities, blueprint architect's view of civilization, "
            "architectural precision, steel and teal tones, intriguing but uncertain, the right path"
        ),
    },

    # Page 5 — Into the Thread ────────────────────────────────────────────────
    {
        "id": "p01_p5_thread",
        "path": "Assets/Art/Panels/Issue01/p01_p5_thread.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "full-page portrait splash: the archive opens into infinite cosmic depth, "
            "an endless library of comic pages stretches into cosmic infinity, "
            "pages become stars become galaxies become pages again, fractal recursion, "
            "a tiny reader silhouette stands at the threshold looking inward and upward, "
            "gold and teal nebulae made of printed ink, grand overwhelming mythological cosmos"
        ),
    },

    # ════════════════════════════════════════════════════════════════════════
    # LENS ICONS  (5 lenses)
    # ════════════════════════════════════════════════════════════════════════
    {
        "id": "lens_mythic",
        "path": "Assets/Art/Lenses/lens_mythic_icon.png",
        "category": "lenses",
        "size": "1024x1024",
        "prompt": _icon(
            "ancient all-seeing eye inside a winged solar disc with cuneiform border ring, "
            "Anunnaki deity symbol, cosmic rays from center, "
            "ancient Egyptian and Mesopotamian fusion symbol",
            "warm gold"
        ),
    },
    {
        "id": "lens_technologic",
        "path": "Assets/Art/Lenses/lens_technologic_icon.png",
        "category": "lenses",
        "size": "1024x1024",
        "prompt": _icon(
            "circuit schematic in the shape of sacred geometry flower-of-life pattern, "
            "PCB trace meets ancient flower of life, nodes and connectors at intersection points",
            "bright cyan blue"
        ),
    },
    {
        "id": "lens_symbolic",
        "path": "Assets/Art/Lenses/lens_symbolic_icon.png",
        "category": "lenses",
        "size": "1024x1024",
        "prompt": _icon(
            "mandala of Jungian archetype symbols, concentric rings, "
            "an eye opening at the center into inner depth, consciousness spiral, "
            "shadow and light in perfect psychological balance",
            "deep violet purple"
        ),
    },
    {
        "id": "lens_political",
        "path": "Assets/Art/Lenses/lens_political_icon.png",
        "category": "lenses",
        "size": "1024x1024",
        "prompt": _icon(
            "hierarchical pyramid with an eye at apex, "
            "bloodline tree descending the sides, chains and crown intertwined, "
            "power made visible through strict geometry",
            "deep crimson red"
        ),
    },
    {
        "id": "lens_spiritual",
        "path": "Assets/Art/Lenses/lens_spiritual_icon.png",
        "category": "lenses",
        "size": "1024x1024",
        "prompt": _icon(
            "merkaba star tetrahedron with ascending spiral through center axis, "
            "consciousness lotus fully open at crown, divine light pillar, "
            "ascension geometry, hidden layers becoming visible",
            "emerald green"
        ),
    },

    # ════════════════════════════════════════════════════════════════════════
    # KNOWLEDGE KEY ICONS  (6 keys)
    # ════════════════════════════════════════════════════════════════════════
    {
        "id": "key_sequence",
        "path": "Assets/Art/Keys/key_sequence_icon.png",
        "category": "keys",
        "size": "1024x1024",
        "prompt": _icon(
            "five panels in a horizontal strip arranged as a directional arrow pointing right, "
            "timeline progression symbol, comic panels as sequential steps, "
            "directional sequence made sacred",
            "amber gold"
        ),
    },
    {
        "id": "key_alignment",
        "path": "Assets/Art/Keys/key_alignment_icon.png",
        "category": "keys",
        "size": "1024x1024",
        "prompt": _icon(
            "four cardinal direction pointers forming a perfect cross alignment, "
            "stars at each cardinal point clicking into precise geometric position, "
            "cardinal shaft alignment, crystalline precision",
            "silver blue"
        ),
    },
    {
        "id": "key_mirror",
        "path": "Assets/Art/Keys/key_mirror_icon.png",
        "category": "keys",
        "size": "1024x1024",
        "prompt": _icon(
            "perfect vertical mirror bisection of a circular symbol, "
            "upper half: night sky with star constellation, "
            "lower half: identical mirror reflection inverted, "
            "as above so below hermetic principle, ouroboros snake at the central axis",
            "silver white"
        ),
    },
    {
        "id": "key_translation",
        "path": "Assets/Art/Keys/key_translation_icon.png",
        "category": "keys",
        "size": "1024x1024",
        "prompt": _icon(
            "a horizontal chain of three glyphs evolving left-to-right: "
            "ancient cuneiform wedge mark → proto-alphabet letter → modern abstract symbol, "
            "glyph evolution sequence, meaning preserved through transformation",
            "warm orange amber"
        ),
    },
    {
        "id": "key_stability",
        "path": "Assets/Art/Keys/key_stability_icon.png",
        "category": "keys",
        "size": "1024x1024",
        "prompt": _icon(
            "Egyptian Djed pillar backbone symbol fused with a circuit board spine, "
            "stability column with electrical nodes running up each vertebra, "
            "sacred technology in permanent standing position, unyielding",
            "teal green"
        ),
    },
    {
        "id": "key_artifact",
        "path": "Assets/Art/Keys/key_artifact_icon.png",
        "category": "keys",
        "size": "1024x1024",
        "prompt": _icon(
            "four distinct symbolic components assembled into a single glowing unified artifact, "
            "each piece a different texture: ancient stone, hammered metal, crystal, organic matter, "
            "four-part sacred machine complete and pulsing with inner light",
            "full spectrum rainbow"
        ),
    },

    # ════════════════════════════════════════════════════════════════════════
    # COVER IMAGES  (2 issue covers)
    # ════════════════════════════════════════════════════════════════════════
    {
        "id": "cover_issue_00",
        "path": "Assets/Art/Cover/cover_issue_00.png",
        "category": "covers",
        "size": "1024x1024",
        "prompt": _cover(
            "interior of a vast archive library at night in 2100, "
            "lone archivist stands before a single glowing unlabeled slot amid endless dark shelves, "
            "one object in vivid color — a comic — glows golden against the grey world, "
            "brutalist architecture, towering shelves, single dramatic spotlight-of-history moment"
        ),
    },
    {
        "id": "cover_issue_01",
        "path": "Assets/Art/Cover/cover_issue_01.png",
        "category": "covers",
        "size": "1024x1024",
        "prompt": _cover(
            "a human figure suspended in absolute ink void surrounded by floating broken panel fragments, "
            "panels at all angles form a broken puzzle around them, "
            "some fragments show ancient worlds, some show stars, some show darkness, "
            "the figure reaches toward a fragment showing a dawn sky sequence, "
            "the broken page is the world itself"
        ),
    },

    # ════════════════════════════════════════════════════════════════════════
    # UI TEXTURES
    # ════════════════════════════════════════════════════════════════════════
    {
        "id": "ui_title_background",
        "path": "Assets/Art/UI/title_background.png",
        "category": "ui",
        "size": "1024x1024",
        "prompt": (
            "deep space cosmic background for a mobile title screen, "
            "ink-wash nebula in deep purple-black, faint golden star patterns, "
            "ancient Anunnaki astronomical symbols as subtle texture embedded in the darkness, "
            "no central subject, pure atmospheric background, no text, no figures"
        ),
    },
    {
        "id": "ui_notebook_texture",
        "path": "Assets/Art/UI/notebook_texture.png",
        "category": "ui",
        "size": "1024x1024",
        "prompt": (
            "aged parchment or journal paper texture, "
            "slight yellowing at edges, paper grain visible, faint ruled lines barely perceptible, "
            "ink stain traces at corners, archival document quality, "
            "flat seamless surface texture, no illustration, no text, no central subject"
        ),
    },
    {
        "id": "ui_pyramid_silhouette",
        "path": "Assets/Art/UI/pyramid_silhouette.png",
        "category": "ui",
        "size": "1024x1024",
        "prompt": (
            "minimalist silhouette of an ancient pyramid centered on near-black background, "
            "the pyramid has a very faint golden crown highlight at apex, "
            "cork board detective investigation style, connection lines barely suggested at base corners, "
            "single symbolic shape on dark, high contrast, no text"
        ),
    },
    {
        "id": "ui_lens_selector_bg",
        "path": "Assets/Art/UI/lens_selector_bg.png",
        "category": "ui",
        "size": "1024x1024",
        "prompt": (
            "dark circular radial interface element, very dark center, "
            "five equidistant subtle glow points arranged in a pentagon, "
            "sacred geometry pentagon pattern subtly traced between the five points, "
            "dark background brightening slightly toward outer ring edge, "
            "flat UI graphic, no text, pure interface geometry"
        ),
    },

    # ════════════════════════════════════════════════════════════════════════
    # ISSUE 02 — "Origins Thread"  (flagship panels)
    # ════════════════════════════════════════════════════════════════════════

    {
        "id": "i02_p1_splash",
        "path": "Assets/Art/Panels/Issue02/p02_p1_splash.png",
        "category": "panels",
        "size": "1024x1792",
        "prompt": _p(
            "full-page portrait splash: a massive Anunnaki craft descending through amber-violet atmosphere "
            "toward the ancient Mesopotamian floodplain below, Eridu city lights beginning to form on the plain, "
            "the craft ancient beyond the word for sky, metallic and enormous, reflected in the Tigris and Euphrates rivers, "
            "dawn light from below catching the hull, cosmic arrival scene, overwhelming scale"
        ),
    },
    {
        "id": "i02_p3_edin",
        "path": "Assets/Art/Panels/Issue02/p02_p3_isis.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "the goddess Isis in ancient Egyptian regalia standing in the E.D.I.N. garden enclosure, "
            "an impossibly perfect managed garden: bioluminescent plants, ordered symmetry, glowing irrigation channels, "
            "Isis holds a luminous scroll-tablet showing biological diagrams and constellation maps, "
            "her expression: custodial authority, not worship, she tends the record not the soil, "
            "gold and turquoise divine light throughout, ancient temple walls with precise glyph inscriptions surrounding the garden"
        ),
    },
    {
        "id": "i02_p6_limiter",
        "path": "Assets/Art/Panels/Issue02/p02_p6_limiter.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "a glowing schematic panel rendered in Technologic Lens teal-blue: two DNA helical strands "
            "where a specification diagram shows space for many more, "
            "the reduction point clearly marked with a glowing MODIFICATION glyph, "
            "ancient hieroglyphic technical annotations surrounding the diagram, "
            "the cutting point circled and marked with political red seal-stamp symbols, "
            "a deliberate engineering ceiling made visible, unsettling clinical precision"
        ),
    },

    # ════════════════════════════════════════════════════════════════════════
    # ISSUE 03 — "Cities Thread"  (flagship panels)
    # ════════════════════════════════════════════════════════════════════════

    {
        "id": "i03_p1_gridlines",
        "path": "Assets/Art/Panels/Issue03/p03_p1_gridlines.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "aerial overhead view of the ancient Sumerian floodplain with a perfect geometric survey grid "
            "already staked into the ground before any structure exists, "
            "wooden survey stakes with taut geometric lines forming a precise city blueprint below, "
            "surveyors in ancient Mesopotamian clothing placing measurements, "
            "the grid exists before the city: the plan precedes all construction, "
            "golden morning light, river glinting in distance, cosmic precision in ancient hands"
        ),
    },
    {
        "id": "i03_p4_overlay",
        "path": "Assets/Art/Panels/Issue03/p03_p4_overlay.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "two translucent city plan drawings overlaid on each other like tracing paper, "
            "the Sumer grid and the Kemet grid perfectly aligned, "
            "where they overlap a glowing golden triangle with an open apex appears at the exact center of both, "
            "the triangle is not drawn on either city plan — it emerges from the overlap itself, "
            "Symbolic Lens purple-gold glow radiating from the overlapping region, "
            "archive document aesthetic, papyrus texture, the triangle is the message"
        ),
    },
    {
        "id": "i03_p7_triangle",
        "path": "Assets/Art/Panels/Issue03/p03_p7_triangle.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "three ancient cities seen simultaneously in a single panel like a triptych: "
            "Sumer on the left (ziggurat skyline), Kemet in the center (sphinx and pylons), "
            "a third coastal city of concentric rings on the right, "
            "a large glowing triangle symbol connects the three cities at their centers, "
            "Symbolic Lens active: the triangle floats above all three cities simultaneously, "
            "no label, no caption, just the shape above three civilisations"
        ),
    },

    # ════════════════════════════════════════════════════════════════════════
    # ISSUE 04 — "The Architect Thread"  (flagship panels)
    # ════════════════════════════════════════════════════════════════════════

    {
        "id": "i04_p1_architect",
        "path": "Assets/Art/Panels/Issue04/p04_p1_architect.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "a scriptorium workshop interior lit by golden lamplight and glowing diagrams, "
            "vast scrolls and papyrus blueprints cover every surface showing city plans, star maps, sacred geometry, "
            "a single cloaked figure stands at the center with back turned, facing the largest blueprint on the wall, "
            "the Architect: anonymous, their robe ancient Egyptian linen with gold trim, "
            "the blueprint on the wall extends beyond the frame in every direction, impossibly massive, "
            "the figure's presence fills the room with focused authority, translator not inventor"
        ),
    },
    {
        "id": "i04_p2_starmap",
        "path": "Assets/Art/Panels/Issue04/p04_p2_correspondence.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "two panels composited as one: upper half shows the night sky at 2500 BCE with Orion's Belt "
            "and Thuban as pole star marked with golden constellation lines, "
            "lower half shows the aerial view of the Giza plateau survey grid perfectly mirroring the star positions above, "
            "the C3 Correspondence: sky and ground as identical maps of each other, "
            "three star positions match three structure positions exactly, "
            "the dividing line between sky and ground shimmers gold, As above so below rendered as engineering fact"
        ),
    },
    {
        "id": "i04_p6_alllenses",
        "path": "Assets/Art/Panels/Issue04/p04_p6_alllenses.png",
        "category": "panels",
        "size": "1024x1792",
        "prompt": _p(
            "a full-page portrait splash showing all five lens symbols radiating from a central sacred geometry mandala, "
            "Mythic gold at the top, Technologic cyan to the upper right, Symbolic purple to lower right, "
            "Political crimson to lower left, Spiritual emerald to upper left, "
            "each lens symbol distinct and glowing with its characteristic color, "
            "the five colors merge at the sacred geometry center into pure white light, "
            "the Architect's blueprint visible beneath all five layers simultaneously, "
            "all five interpretations of reality active simultaneously for the first time"
        ),
    },

    # ════════════════════════════════════════════════════════════════════════
    # ISSUE 05 — "The Break"  (flagship panels)
    # ════════════════════════════════════════════════════════════════════════

    {
        "id": "i05_p1_wall",
        "path": "Assets/Art/Panels/Issue05/p05_p1_wall.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "a massive ancient Egyptian stone wall filled with hieroglyphic inscriptions, "
            "but systematically defaced: rectangular sections chiseled away with precision and care, "
            "not vandalism — this is policy, each removed glyph carefully targeted, "
            "ghosted outlines remain pressed into the stone where glyphs once existed, "
            "chisel marks are methodical and overlapping, a removal program not a rage, "
            "Political Lens crimson light illuminates the systematic erasure pattern, "
            "the absences are as telling as what remains"
        ),
    },
    {
        "id": "i05_p2_scribe",
        "path": "Assets/Art/Panels/Issue05/p05_p2_scribeA.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "a group of Scribe faction figures in ancient Egyptian administrative robes standing before a glyph wall, "
            "their faces are deliberately blank — no features, no identity, only institutional authority, "
            "each figure holds a chisel and administrative scroll in ritual position, "
            "they are a process not a people, perfectly uniform, completely deliberate, "
            "Political Lens renders them in bureaucratic crimson silhouette, "
            "the wall behind them shows the removal orders being executed, an institution erasing itself into power"
        ),
    },
    {
        "id": "i05_p4_identity",
        "path": "Assets/Art/Panels/Issue05/p05_p4_figure1.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "two panels side by side as one image: on the left an Egyptian pharaoh figure in full royal regalia with crown and crook, "
            "on the right the same face in travel-worn linen clothes walking away through desert, "
            "both figures carrying or having carried the same rectangular object wrapped in cloth, "
            "the face clearly identical in both panels despite different contexts, "
            "a lone rectangular void visible in the space between the two panels — the carried object's shadow, "
            "Mythic golden light on royal figure, Spiritual emerald light on the exile, Political lens red around the seam between them"
        ),
    },

    # ════════════════════════════════════════════════════════════════════════
    # ISSUE 06 — "Mirror Site"  (flagship panels)
    # ════════════════════════════════════════════════════════════════════════

    {
        "id": "i06_p1_grid",
        "path": "Assets/Art/Panels/Issue06/p06_p1_grid.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "aerial overhead view of the Giza plateau city grid, "
            "a large geometric compass rose overlaid on the grid showing true north as one direction "
            "but all the major structures clearly offset by exactly 15.5 degrees from it, "
            "the 15.5 degree angle marked with a glowing golden measurement line, "
            "Technologic Lens teal-blue overlays show the angle is consistent across every major axis, "
            "inset in the corner: the star Thuban with the same 15.5 degree offset marker, "
            "this is not accident — this is astronomical precision made into architecture"
        ),
    },
    {
        "id": "i06_p3_mirrorcity",
        "path": "Assets/Art/Panels/Issue06/p06_p3_mirrorcity.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "two mirror-image city plans reflected perfectly across a central axis — the Nile river, "
            "above the Nile: the documented Giza plateau city with its primary structures, "
            "below the Nile: the mirror city implied by the above-ground geometry, its foundations just visible, "
            "Symbolic Lens purple overlay shows six alignment pairs connecting above and below, "
            "the reflection is not visual symmetry — it is constructed symmetry, built into the architecture, "
            "C4 Mirror-City complete: the puzzle solved, the axis glowing gold where sky meets water"
        ),
    },
    {
        "id": "i06_p6_pyramid",
        "path": "Assets/Art/Panels/Issue06/p06_p6_pyramid.png",
        "category": "panels",
        "size": "1024x1792",
        "prompt": _p(
            "portrait orientation: a vast dark plain at dusk, a single enormous shape on the horizon, "
            "the shape is a pyramid — but completely without label, without caption, without confirmation, "
            "just a silhouette, precisely triangular, on the exact horizon line, "
            "the viewer has been looking at it this whole time without knowing its name, "
            "overwhelming in scale, inevitable in presence, "
            "no glow, no magic, just the ancient shape against fading amber sky, "
            "the most significant thing in every scene the player has been in — finally visible"
        ),
    },

    # ════════════════════════════════════════════════════════════════════════
    # ISSUE 07 — "The Vault"  (flagship panels)
    # ════════════════════════════════════════════════════════════════════════

    {
        "id": "i07_p1_passage",
        "path": "Assets/Art/Panels/Issue07/p07_p1_passage.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "interior of a narrow descending stone passage inside an ancient structure, "
            "the passage walls are perfectly smooth polished limestone, the angle of descent is unmistakably precise, "
            "a shaft of light from above illuminates the passage at a specific angle — not sunlight, engineered light, "
            "the passage geometry changes subtly as it descends, each section a different deliberate specification, "
            "Technologic Lens teal measurements float along the walls showing the angles and tolerances, "
            "the camera looks down the passage toward darkness and something vast below, intimate and ancient"
        ),
    },
    {
        "id": "i07_p2_labyrinth",
        "path": "Assets/Art/Panels/Issue07/p07_p2_labyrinth.png",
        "category": "panels",
        "size": "1024x1792",
        "prompt": _p(
            "full portrait diagram cross-section of an ancient shaft system rendered as an interactive panel: "
            "a complex labyrinthine network of ascending and descending shafts shown in deep-cut stone, "
            "a micro-camera POV path traced in gold showing the correct route through the labyrinth, "
            "junction markers in Symbolic purple at each decision point, "
            "Technologic teal measurements showing shaft angles pointing to specific stars above, "
            "an alternate shaft branching to a dead-end chamber on one side, "
            "a hidden chamber visible off the main path containing a closed diagram, "
            "the center of the labyrinth radiates warm golden light — the destination, not yet reached"
        ),
    },
    {
        "id": "i07_p4_ark",
        "path": "Assets/Art/Panels/Issue07/p07_p4_ark.png",
        "category": "panels",
        "size": "1024x1024",
        "prompt": _p(
            "the Ark of the Covenant revealed in the central chamber under combined Spiritual and Technologic lens light: "
            "a golden rectangular chest with two horizontal carrying poles on each side, "
            "two winged figures — cherubim — face each other on the golden lid with wings touching above, "
            "golden light from within the Ark itself illuminates the chamber from below, "
            "Spiritual emerald glow outlines its divine significance, "
            "Technologic teal measurements float around it showing precise specifications, "
            "the void in the stone platform below it perfectly shaped to receive it, "
            "the Ark is complete except for the single missing carrying pole — its ghost-outline on the chamber floor"
        ),
    },
]


# ── Helpers ────────────────────────────────────────────────────────────────────

def _load_api_key() -> str:
    if not ENV_FILE.exists():
        sys.exit(f"ERROR: .env not found at {ENV_FILE}")
    for raw in ENV_FILE.read_text(encoding="utf-8").splitlines():
        line = raw.strip()
        if line.startswith("GROK_API_KEY="):
            key = line.split("=", 1)[1].strip()
            if key:
                return key
    sys.exit("ERROR: GROK_API_KEY not found in .env")


def _set_texture_import_settings(abs_path: str) -> None:
    """Write a .meta stub so Unity imports as Sprite when it next refreshes."""
    meta_path = abs_path + ".meta"
    if os.path.exists(meta_path):
        return  # already has meta, Unity will handle it
    # Write a minimal meta that sets textureType = Sprite (type 8)
    import uuid
    guid = uuid.uuid4().hex
    meta = (
        f"fileFormatVersion: 2\n"
        f"guid: {guid}\n"
        f"TextureImporter:\n"
        f"  textureType: 8\n"        # Sprite
        f"  textureShape: 1\n"
        f"  spriteMode: 1\n"         # Single sprite
        f"  spritePivot: {{x: 0.5, y: 0.5}}\n"
        f"  spritePixelsToUnits: 100\n"
        f"  filterMode: 1\n"
        f"  maxTextureSize: 2048\n"
        f"  textureCompression: 1\n"
    )
    Path(meta_path).write_text(meta, encoding="utf-8")


def _download(url: str) -> bytes:
    req = urllib.request.Request(url, headers={"User-Agent": "ArchiveArtGen/1.0"})
    with urllib.request.urlopen(req, timeout=60) as resp:
        return resp.read()


# ── Main ───────────────────────────────────────────────────────────────────────

def main() -> None:
    parser = argparse.ArgumentParser(
        description="Generate Archive of Echoes art assets via xAI image API"
    )
    parser.add_argument("--dry-run",   action="store_true",
                        help="Preview what would be generated without calling the API")
    parser.add_argument("--force",     action="store_true",
                        help="Overwrite files that already exist")
    parser.add_argument("--list",      action="store_true",
                        help="List all asset IDs and exit")
    parser.add_argument("--category",
                        choices=["panels", "lenses", "keys", "covers", "ui"],
                        help="Generate only assets in this category")
    parser.add_argument("--id",        metavar="ASSET_ID",
                        help="Generate only the single asset with this ID")
    args = parser.parse_args()

    if args.list:
        for a in ASSETS:
            print(f"{a['id']:40s}  {a['category']:8s}  {a['path']}")
        return

    # Resolve target list
    targets = ASSETS[:]
    if args.category:
        targets = [a for a in targets if a["category"] == args.category]
    if args.id:
        targets = [a for a in targets if a["id"] == args.id]
        if not targets:
            sys.exit(f"ERROR: no asset found with id '{args.id}'")

    if not args.force:
        already = [a for a in targets if (PROJECT_ROOT / a["path"]).exists()]
        pending = [a for a in targets if not (PROJECT_ROOT / a["path"]).exists()]
        if already:
            print(f"[skip] {len(already)} existing (use --force to overwrite)")
        targets = pending

    if not targets:
        print("[done] Nothing to generate.")
        return

    print(f"[plan] {len(targets)} assets to generate  model={IMAGE_MODEL}  delay={REQUEST_DELAY}s")

    if args.dry_run:
        print()
        for a in targets:
            print(f"  [dry]  {a['id']:40s} → {a['path']}")
        print(f"\n[dry-run] No API calls made.  (~${len(targets) * 0.07:.2f} estimated cost)")
        return

    # Load openai lazily (gives friendly error if not installed)
    try:
        from openai import OpenAI
    except ImportError:
        sys.exit("ERROR: openai package not installed.\n  Run: pip install openai")

    api_key = _load_api_key()
    client  = OpenAI(api_key=api_key, base_url=XAI_BASE_URL)

    generated, failed = [], []
    manifest: dict[str, str] = {}

    for i, asset in enumerate(targets, 1):
        out_path = PROJECT_ROOT / asset["path"]
        out_path.parent.mkdir(parents=True, exist_ok=True)
        print(f"[{i:>2}/{len(targets)}] {asset['id']}", end=" … ", flush=True)

        try:
            response = client.images.generate(
                model=IMAGE_MODEL,
                prompt=asset["prompt"],
                n=1,
                response_format="b64_json",
            )
            import base64
            img_bytes = base64.b64decode(response.data[0].b64_json)
            out_path.write_bytes(img_bytes)
            _set_texture_import_settings(str(out_path))
            kb = len(img_bytes) // 1024
            print(f"✓  {kb}KB → {out_path.relative_to(PROJECT_ROOT)}")
            generated.append(asset)
            manifest[asset["id"]] = asset["path"].replace("\\", "/")

        except Exception as exc:
            print(f"✗  FAILED — {exc}")
            failed.append((asset, str(exc)))

        if i < len(targets):
            time.sleep(REQUEST_DELAY)

    # Merge into existing manifest and save
    existing: dict[str, str] = {}
    if MANIFEST.exists():
        try:
            existing = json.loads(MANIFEST.read_text(encoding="utf-8"))
        except Exception:
            pass
    existing.update(manifest)
    MANIFEST.parent.mkdir(parents=True, exist_ok=True)
    MANIFEST.write_text(json.dumps(existing, indent=2, ensure_ascii=False), encoding="utf-8")

    print(f"\n{'─'*60}")
    print(f"  Generated : {len(generated)}")
    print(f"  Failed    : {len(failed)}")
    print(f"  Manifest  : {MANIFEST.relative_to(PROJECT_ROOT)}")
    if failed:
        print("\n  Failures:")
        for a, err in failed:
            print(f"    {a['id']}: {err}")
    print("\n  Next step in Unity: Tools → Archive of Echoes → 5 – Import Art Assets")


if __name__ == "__main__":
    main()
