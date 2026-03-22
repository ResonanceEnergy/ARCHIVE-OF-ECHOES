#!/usr/bin/env python3
"""
test_covers_eden.py — Eden-themed cover variations for Archive of Echoes.
Anunnaki mythology places Eden as the original Sumerian garden Edin.
Six variations exploring that primordial paradise-lost aesthetic.
"""
import base64, sys, time, urllib.request
from pathlib import Path

ENV_FILE = Path(r"C:\dev\DIGITAL LABOUR\DIGITAL LABOUR\.env")
OUT_DIR  = Path(__file__).parent / "Assets" / "Art" / "CoverTest" / "Eden"
XAI_BASE = "https://api.x.ai/v1"
MODEL    = "grok-imagine-image"
DELAY    = 3.0

COVERS = [

    # E1 — Paradise Lost (lush garden collapsing into archive ruin)
    (
        "e1_paradise_lost",
        "game cover art, portrait orientation, "
        "split composition: upper half is an impossibly lush ancient Sumerian garden Edin -- "
        "enormous hanging gardens, rivers of light, golden fruit trees, celestial beings in silhouette, "
        "lower half is the cold brutalist 2100 archive in deep shadow and ruin, "
        "the two worlds bleeding into each other at the centre seam, "
        "ink-wash painterly style, warm gold above cold teal below, "
        "a lone figure stands at the exact split reaching upward, no text, no logos"
    ),

    # E2 — The First Garden (Anunnaki as architects of Eden)
    (
        "e2_first_garden",
        "game cover art, portrait orientation, "
        "an ancient Sumerian garden of paradise at cosmic scale, "
        "enormous Anunnaki deity figures in silhouette tending luminous trees, "
        "every leaf made of hammered gold and lapis lazuli, rivers of pure liquid light, "
        "a single open comic book floats at the center of the garden radiating warm white glow, "
        "ultra dark sky above studded with cuneiform star charts, "
        "oil-wash painterly texture, deep shadow with burning gold highlights, no text, no logos"
    ),

    # E3 — Corrupted Eden (paradise infected by archive decay)
    (
        "e3_corrupted_eden",
        "game cover art, portrait orientation, "
        "once-beautiful Sumerian paradise garden now rotting and glitching, "
        "golden trees have circuit-board veins exposed, flowers are half ink-panel fragments, "
        "the garden river has become data streams of teal text dissolving, "
        "a hooded figure moves through the decay holding an open glowing comic book, "
        "the comic pages are the last clean beautiful thing left, "
        "dark atmospheric, VHS corruption artifacts at image edges, no text, no logos"
    ),

    # E4 — The Serpent Archive (Anunnaki serpent as cosmic knowledge keeper)
    (
        "e4_serpent_archive",
        "game cover art, portrait orientation, "
        "a colossal ancient Sumerian serpent deity coiled around a towering archive library column, "
        "the serpent's scales are comic book pages containing imprisoned histories, "
        "its eyes are the only light source -- twin burning gold suns, "
        "an archivist figure stands below dwarfed and awestruck, "
        "deep black ground, ink-wash style, dramatic scale contrast, "
        "mythological terror and wonder in equal measure, no text, no logos"
    ),

    # E5 — Memory of Eden (2100 archivist glimpses original paradise)
    (
        "e5_memory_eden",
        "game cover art, portrait orientation, "
        "close portrait of an archivist's face in deep shadow, "
        "they hold an open comic book at chest height, "
        "from the open pages a vision of the Sumerian garden Edin projects upward like a hologram -- "
        "lush impossible paradise flowers gold and green, Anunnaki figures in the mist, "
        "the vision is ethereal and barely there, dissolving at the edges, "
        "the archivist's expression is reverent grief -- they know this world is gone, "
        "extreme chiaroscuro, one warm gold glow from the book, everything else black, no text, no logos"
    ),

    # E6 — Edin Awakens (Eden rising inside the archive itself)
    (
        "e6_edin_awakens",
        "game cover art, portrait orientation, "
        "inside a brutalist stone archive library vast as a cathedral, "
        "from the open pages of one specific comic book on a shelf, "
        "the Sumerian garden Edin is physically erupting and blooming outward into the archive, "
        "golden vines and light pour from the book across the cold stone floor, "
        "reaching toward a figure standing at the door in silhouette, "
        "the contrast of organic living gold against dead grey stone is total, "
        "ink-wash painterly with sacred geometry faintly burned into the stone walls, "
        "threshold moment of mythological awakening, no text, no logos"
    ),
]

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
