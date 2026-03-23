#!/usr/bin/env python3
"""
generate_art_polish.py — Procedurally generate art-polish overlay textures.

Outputs to Assets/Art/UI/:
    paper_grain.png     512×512    Tileable greyscale paper grain noise
    vignette.png       1024×1024   Radial dark vignette (RGBA)
    ink_lines.png       512×512    Subtle horizontal ink scan-lines (RGBA)
    glow_overlay.png    512×512    Warm centre glow (RGBA, for additive blend)

Requirements: pillow  (pip install pillow)

Usage:
    python generate_art_polish.py                 # generate all missing
    python generate_art_polish.py --force         # overwrite existing
    python generate_art_polish.py --dry-run       # print what would be done
"""

import argparse
import hashlib
import io
import json
import math
import random
import sys
from pathlib import Path

if sys.stdout.encoding and sys.stdout.encoding.lower() != "utf-8":
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

try:
    from PIL import Image, ImageDraw, ImageFilter
except ImportError:
    print("ERROR: Pillow is required.  Run: pip install pillow", file=sys.stderr)
    sys.exit(1)

# ── Config ──────────────────────────────────────────────────────────────────────

SCRIPT_DIR   = Path(__file__).parent
OUTPUT_DIR   = SCRIPT_DIR / "Assets" / "Art" / "UI"
MANIFEST_OUT = SCRIPT_DIR / "Assets" / "Art" / "art_polish_manifest.json"

CATALOGUE = [
    {
        "id":   "paper_grain",
        "file": "paper_grain.png",
        "mode": "L",
        "size": (512, 512),
        "desc": "Tileable 512×512 greyscale paper grain noise",
    },
    {
        "id":   "vignette",
        "file": "vignette.png",
        "mode": "RGBA",
        "size": (1024, 1024),
        "desc": "1024×1024 radial vignette — white centre, dark edges, RGBA",
    },
    {
        "id":   "ink_lines",
        "file": "ink_lines.png",
        "mode": "RGBA",
        "size": (512, 512),
        "desc": "512×512 horizontal ink scan-lines, RGBA alpha texture",
    },
    {
        "id":   "glow_overlay",
        "file": "glow_overlay.png",
        "mode": "RGBA",
        "size": (512, 512),
        "desc": "512×512 warm golden centre glow, RGBA additive blend layer",
    },
]

# ── Texture generators ────────────────────────────────────────────────────────────

def _gen_paper_grain(size: tuple) -> Image.Image:
    """Tileable greyscale paper grain.  Mid-grey base (≈135) with low-amplitude
    pseudo-Gaussian noise and a very subtle lift at the centre."""
    w, h = size
    rng  = random.Random(0xB00C_0FFE)
    cx, cy = w / 2.0, h / 2.0
    data = []
    for y in range(h):
        for x in range(w):
            # Three-sample average approximates Gaussian (CLT)
            n = (rng.random() + rng.random() + rng.random()) / 3.0
            v = 105 + int(n * 65)     # 105..170, mean ≈ 137
            # Very slight radial brightness lift at centre (+8 max)
            dx = (x - cx) / cx
            dy = (y - cy) / cy
            lift = int(8 * max(0.0, 1.0 - math.hypot(dx, dy)))
            data.append(max(0, min(255, v + lift)))
    img = Image.new("L", size)
    img.putdata(data)
    return img.filter(ImageFilter.GaussianBlur(radius=0.4))


def _gen_vignette(size: tuple) -> Image.Image:
    """RGBA vignette: nearly transparent at centre, dark + opaque at corners."""
    w, h = size
    cx, cy = w / 2.0, h / 2.0
    max_r  = math.hypot(cx, cy)
    pixels = []
    for y in range(h):
        for x in range(w):
            r = math.hypot(x - cx, y - cy) / max_r   # 0 (centre) → ~1.41 (corner)
            t = min(1.0, r / 0.80) ** 2.4             # smooth push-in; full dark at 80% radius
            alpha = int(t * 210)                      # 0 transparent centre → 210 dark edge
            grey  = int(255 * (1.0 - t * 0.85))      # white centre → near-black edge
            pixels.append((grey, grey, grey, alpha))
    img = Image.new("RGBA", size)
    img.putdata(pixels)
    return img.filter(ImageFilter.GaussianBlur(radius=5))


def _gen_ink_lines(size: tuple) -> Image.Image:
    """RGBA ink scan-lines: repeating semi-transparent dark horizontal stripes."""
    w, h = size
    img  = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    for y in range(0, h, 4):
        # Opacity undulates slightly down the image for organic feel
        opacity = 22 + int(14 * math.sin(y / h * math.pi * 3))
        draw.line([(0, y), (w - 1, y)], fill=(10, 6, 3, opacity))
    return img.filter(ImageFilter.GaussianBlur(radius=0.3))


def _gen_glow_overlay(size: tuple) -> Image.Image:
    """RGBA warm golden glow: fully opaque gold at centre, transparent at edges.
    Designed for additive or soft-light blend in Unity UI."""
    w, h   = size
    cx, cy = w / 2.0, h / 2.0
    max_r  = math.hypot(cx, cy)
    pixels = []
    for y in range(h):
        for x in range(w):
            r   = math.hypot(x - cx, y - cy) / max_r    # 0..1.41
            t   = min(1.0, r / 0.65)                     # normalise edge to 65% radius
            glow = (1.0 - t) ** 2.8                      # tight central bloom
            alpha = int(glow * 185)
            pixels.append((255, 218, 95, alpha))          # warm archival gold
    img = Image.new("RGBA", size)
    img.putdata(pixels)
    return img.filter(ImageFilter.GaussianBlur(radius=9))


# ── Unity .meta stub ──────────────────────────────────────────────────────────────

def _stable_guid(path: Path) -> str:
    """Deterministic 32-hex GUID from file path (no external UUID library needed)."""
    return hashlib.md5(str(path).encode()).hexdigest()


def _write_meta(png_path: Path) -> None:
    """Write a minimal Unity TextureImporter .meta stub beside the PNG."""
    meta = png_path.parent / (png_path.name + ".meta")
    guid = _stable_guid(png_path)
    content = (
        f"fileFormatVersion: 2\n"
        f"guid: {guid}\n"
        f"TextureImporter:\n"
        f"  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}\n"
        f"  spritePivot: {{x: 0.5, y: 0.5}}\n"
        f"  spritePixelsPerUnit: 100\n"
        f"  textureType: Sprite\n"
        f"  filterMode: Bilinear\n"
        f"  wrapMode: Repeat\n"
        f"  maxTextureSize: 2048\n"
        f"  textureCompression: Normal\n"
        f"  sRGBTexture: 1\n"
    )
    meta.write_text(content, encoding="utf-8")


# ── Entry point ───────────────────────────────────────────────────────────────────

_GENERATORS = {
    "paper_grain":  _gen_paper_grain,
    "vignette":     _gen_vignette,
    "ink_lines":    _gen_ink_lines,
    "glow_overlay": _gen_glow_overlay,
}


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Generate art-polish overlay textures for Archive of Echoes"
    )
    parser.add_argument("--force",   action="store_true", help="Overwrite existing textures")
    parser.add_argument("--dry-run", action="store_true", help="Print what would be done, run nothing")
    args = parser.parse_args()

    if args.dry_run:
        print("[dry-run] No files will be written.")

    if not args.dry_run:
        OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    manifest        = []
    total_generated = 0

    for entry in CATALOGUE:
        out_path = OUTPUT_DIR / entry["file"]

        if out_path.exists() and not args.force:
            print(f"  skip (exists)  {out_path.name}")
            manifest.append({
                "id":     entry["id"],
                "path":   str(out_path.relative_to(SCRIPT_DIR)).replace("\\", "/"),
                "status": "cached",
            })
            continue

        if args.dry_run:
            print(f"  [dry-run] would generate  {out_path.name}  — {entry['desc']}")
            continue

        print(f"  generating  {out_path.name}  ({entry['desc']}) ...", end="", flush=True)
        img = _GENERATORS[entry["id"]](entry["size"])
        img.save(str(out_path))
        _write_meta(out_path)
        manifest.append({
            "id":     entry["id"],
            "path":   str(out_path.relative_to(SCRIPT_DIR)).replace("\\", "/"),
            "status": "generated",
        })
        total_generated += 1
        print(f"  ✓  {entry['size'][0]}×{entry['size'][1]} {entry['mode']}")

    if not args.dry_run:
        MANIFEST_OUT.parent.mkdir(parents=True, exist_ok=True)
        MANIFEST_OUT.write_text(
            json.dumps(manifest, indent=2, ensure_ascii=False),
            encoding="utf-8",
        )
        print(f"\n  Manifest : {MANIFEST_OUT}")
        print(f"  Generated: {total_generated}/{len(CATALOGUE)} textures.")
    else:
        print(f"\n  [dry-run] would generate {len(CATALOGUE)} textures → {OUTPUT_DIR}")


if __name__ == "__main__":
    main()
