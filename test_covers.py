#!/usr/bin/env python3
"""
test_covers.py — Generate 6 cover style variations for Archive of Echoes.
Results saved to Assets/Art/CoverTest/cover_v1.png … cover_v6.png
"""
import base64, sys, time, urllib.request
from pathlib import Path

ENV_FILE   = Path(r"C:\dev\DIGITAL LABOUR\DIGITAL LABOUR\.env")
OUT_DIR    = Path(__file__).parent / "Assets" / "Art" / "CoverTest"
XAI_BASE   = "https://api.x.ai/v1"
MODEL      = "grok-imagine-image"
DELAY      = 3.0

# ── 6 cover prompts ────────────────────────────────────────────────────────────

COVERS = [

    # V1 — Midnight Ink (current baseline, refined)
    (
        "v1_midnight_ink",
        "game cover art, portrait orientation, pure black background, "
        "lone hooded figure standing inside a vast ancient stone archive, "
        "single dramatic beam of gold light from above illuminating towering shelves "
        "of glowing comic books stretching to infinity, "
        "thick gestural ink strokes, deep dramatic shadows, "
        "monochromatic ink-wash with single gold highlight color only, "
        "no text, no logos, cinematic and mythic"
    ),

    # V2 — Brutalist Comic (Frank Miller × cyberpunk)
    (
        "v2_brutalist_comic",
        "game cover art, portrait orientation, "
        "extreme high-contrast black and white brutalist comic illustration, "
        "massive Anunnaki relief carvings on stone walls with glowing teal neon data streams "
        "bleeding into ancient cuneiform script, "
        "a solitary archivist figure dwarfed by brutal stone geometry, "
        "Frank Miller Sin City aesthetic, no grey tones only pure black and white plus teal, "
        "no text, no logos, oppressive scale, mythological weight"
    ),

    # V3 — Painted Noir (oil/gouache painterly)
    (
        "v3_painted_noir",
        "game cover art, portrait orientation, "
        "oil and gouache painterly illustration, Dishonored loading-screen aesthetic, "
        "rich textured brushwork, desaturated library interior with warm amber lantern light, "
        "ancient Mesopotamian artifacts and star maps on reading tables, "
        "a figure hunched over a glowing open comic book, "
        "the pages radiate impossible golden light upward onto their face, "
        "cinematic chiaroscuro lighting, no text, no logos, painterly not line art"
    ),

    # V4 — Sacred Geometry Glitch
    (
        "v4_sacred_geometry_glitch",
        "game cover art, portrait orientation, "
        "clean sacred geometry -- flower of life, metatron's cube, golden spiral -- "
        "overlaid with deliberate VHS scan-line corruption and digital glitch artifacts, "
        "an eye at the center opening into infinite archive depth, "
        "ultra dark near-black background, glowing gold geometric lines, "
        "teal chromatic aberration at glitch points, esoteric-tech fusion aesthetic, "
        "feels ancient and broken simultaneously, no text, no logos"
    ),

    # V5 — Woodcut Revival (2-color ancient+modern)
    (
        "v5_woodcut_revival",
        "game cover art, portrait orientation, "
        "bold woodblock print illustration style, rough hand-cut edges, "
        "strict two-color palette: deep black ink on aged cream parchment, "
        "a massive winged Anunnaki figure looming over a tiny human reader "
        "sitting cross-legged with an open comic book in their lap, "
        "the comic's pages emit the only light source, a halo of warmth, "
        "graphic bold shapes, no halftone, no gradients, no text, no logos"
    ),

    # V6 — Midnight Ink × Sacred Geometry fusion (recommended hybrid)
    (
        "v6_midnight_sacred_fusion",
        "game cover art, portrait orientation, "
        "pure black ground, brutalist stone archive interior, "
        "massive Anunnaki relief carvings on walls lit by a single column of gold light, "
        "teal digital data streams dissolving into ancient cuneiform at the edges, "
        "sacred geometry -- golden ratio spiral -- subtly embedded in the composition, "
        "ink-wash painterly texture on every surface, nothing is clean or vectorized, "
        "a lone hooded figure stands at the threshold holding an open glowing comic, "
        "the comic emits the only warm light in the scene, "
        "overwhelming scale, mythological atmosphere, no text, no logos"
    ),
]

# ── Run ────────────────────────────────────────────────────────────────────────

def _load_key() -> str:
    for line in ENV_FILE.read_text(encoding="utf-8").splitlines():
        if line.startswith("GROK_API_KEY="):
            return line.split("=", 1)[1].strip()
    sys.exit("GROK_API_KEY not found")

def _download(url: str) -> bytes:
    req = urllib.request.Request(url, headers={"User-Agent": "ArchiveArtGen/1.0"})
    with urllib.request.urlopen(req, timeout=60) as r:
        return r.read()

def main():
    try:
        from openai import OpenAI
    except ImportError:
        sys.exit("pip install openai")

    OUT_DIR.mkdir(parents=True, exist_ok=True)
    client = OpenAI(api_key=_load_key(), base_url=XAI_BASE)

    for i, (name, prompt) in enumerate(COVERS, 1):
        out = OUT_DIR / f"{name}.png"
        print(f"[{i}/6] {name} … ", end="", flush=True)
        try:
            resp = client.images.generate(model=MODEL, prompt=prompt, n=1)
            b64  = resp.data[0].b64_json
            url  = resp.data[0].url
            img  = base64.b64decode(b64) if b64 else _download(url)
            out.write_bytes(img)
            print(f"✓  {len(img)//1024}KB → {out.name}")
        except Exception as e:
            print(f"✗  {e}")
        if i < len(COVERS):
            time.sleep(DELAY)

    print(f"\nAll done → {OUT_DIR}")
    import subprocess
    subprocess.Popen(["explorer", str(OUT_DIR)])

if __name__ == "__main__":
    main()
