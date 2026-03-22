#!/usr/bin/env python3
"""gen_closing_scene.py — Closing scene in V1 Midnight Ink style."""
import base64, sys, urllib.request
from pathlib import Path

ENV_FILE = Path(r"C:\dev\DIGITAL LABOUR\DIGITAL LABOUR\.env")
OUT      = Path(__file__).parent / "Assets" / "Art" / "Cover" / "closing_scene.png"
XAI_BASE = "https://api.x.ai/v1"
MODEL    = "grok-imagine-image"

PROMPT = (
    "game closing scene art, portrait orientation, pure black background, "
    "a lone archivist figure standing before a vast dark archive shelf, "
    "gently closing an enormous glowing comic book with both hands, "
    "the last gold light bleeding out between the pages as they shut, "
    "the entire archive plunging into absolute darkness panel by panel around them, "
    "each shelf going dark in sequence like lights extinguishing toward infinity, "
    "the figure's face lit only by the final dying glow of the closing pages, "
    "monochromatic ink-wash with single gold highlight color only, "
    "thick gestural ink strokes, deep dramatic shadows, "
    "the weight of every history being sealed away, "
    "cinematic and mythic, no text, no logos"
)

def _load_key() -> str:
    for line in ENV_FILE.read_text(encoding="utf-8").splitlines():
        if line.startswith("GROK_API_KEY="):
            return line.split("=", 1)[1].strip()
    sys.exit("GROK_API_KEY not found")

def main():
    try:
        from openai import OpenAI
    except ImportError:
        sys.exit("pip install openai")

    OUT.parent.mkdir(parents=True, exist_ok=True)
    client = OpenAI(api_key=_load_key(), base_url=XAI_BASE)

    print("Generating closing scene (V1 Midnight Ink) … ", end="", flush=True)
    resp = client.images.generate(model=MODEL, prompt=PROMPT, n=1, response_format="b64_json")
    b64  = resp.data[0].b64_json
    img  = base64.b64decode(b64)
    OUT.write_bytes(img)
    print(f"done  {len(img)//1024}KB → {OUT.name}")

    import subprocess
    subprocess.Popen(["explorer", str(OUT.parent)])
    subprocess.Popen(["cmd", "/c", "start", "", str(OUT)])

if __name__ == "__main__":
    main()
