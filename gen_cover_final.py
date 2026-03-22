#!/usr/bin/env python3
"""gen_cover_final.py — Generate the official Archive of Echoes game cover using E4 style."""
import base64, sys, time, urllib.request
from pathlib import Path

ENV_FILE = Path(r"C:\dev\DIGITAL LABOUR\DIGITAL LABOUR\.env")
OUT_DIR  = Path(__file__).parent / "Assets" / "Art" / "Cover"
XAI_BASE = "https://api.x.ai/v1"
MODEL    = "grok-imagine-image"

# E4 style — Serpent Archive / Midnight Ink — refined for final cover
PROMPT = (
    "official game cover art, portrait orientation, "
    "a colossal ancient Sumerian serpent deity coiled around a towering archive library column, "
    "the serpent's scales are comic book pages containing imprisoned histories of twelve ages, "
    "its eyes are the only light source — twin burning gold suns casting dramatic upward shadows, "
    "an archivist figure stands below at the base of the column, dwarfed and awestruck, "
    "holding a single open comic book whose pages glow with faint warm gold light, "
    "pure black ground, deep dramatic shadows, thick gestural ink strokes, "
    "midnight ink style, monochromatic with only gold highlights, "
    "overwhelming mythological scale, ancient Anunnaki cosmic terror and wonder, "
    "masterpiece composition, no text, no logos, no speech bubbles"
)

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
    out = OUT_DIR / "cover_game_official.png"

    print("Generating official game cover (E4 Serpent Archive style) … ", end="", flush=True)
    try:
        resp = client.images.generate(model=MODEL, prompt=PROMPT, n=1)
        b64  = resp.data[0].b64_json
        url  = resp.data[0].url
        img  = base64.b64decode(b64) if b64 else _download(url)
        out.write_bytes(img)
        print(f"✓  {len(img)//1024}KB → {out}")
        import subprocess
        subprocess.Popen(["explorer", str(OUT_DIR)])
        subprocess.Popen([str(out)])
    except Exception as e:
        print(f"✗  {e}")

if __name__ == "__main__":
    main()
