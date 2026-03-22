#!/usr/bin/env python3
"""
extract_style.py — Save reference image + extract style descriptor via xAI vision.
Place your reference image at: Assets/Art/Reference/style_ref.png
Then run this script to generate style_descriptor.json used by generate_art.py
"""
import base64, sys, json
from pathlib import Path
from openai import OpenAI

ENV_FILE    = Path(r"C:\dev\DIGITAL LABOUR\DIGITAL LABOUR\.env")
REF_IMAGE   = Path(__file__).parent / "Assets" / "Art" / "Reference" / "style_ref.png"
STYLE_JSON  = Path(__file__).parent / "Assets" / "Art" / "Reference" / "style_descriptor.json"
XAI_BASE    = "https://api.x.ai/v1"
VISION_MODEL = "grok-2-vision-latest"

VISION_PROMPT = """Analyze this game art image and produce a detailed art style descriptor for use in an AI image generation prompt.

Describe ONLY the visual style — NOT the content/subject matter. Include:
- Rendering technique (cel-shading, painterly, ink, etc.)
- Color palette (dominant colors, accent colors, temperature)
- Lighting style (light sources, shadow quality, glow effects)
- Line work (thick/thin, clean/rough, presence/absence)
- Texture quality (smooth, grain, noise)
- Atmosphere and mood
- Any distinctive visual signatures

Output a single-paragraph prompt fragment (no bullet points, no headers) that could be appended to any image prompt to reproduce this exact art style. Start directly with the style description, no preamble."""

def _load_key() -> str:
    for line in ENV_FILE.read_text(encoding="utf-8").splitlines():
        if line.startswith("GROK_API_KEY="):
            return line.split("=", 1)[1].strip()
    sys.exit("GROK_API_KEY not found")

def main():
    if not REF_IMAGE.exists():
        print(f"ERROR: Reference image not found at:\n  {REF_IMAGE}")
        print("Save your reference image there and re-run.")
        sys.exit(1)

    client = OpenAI(api_key=_load_key(), base_url=XAI_BASE)

    # Encode image as base64
    img_b64 = base64.b64encode(REF_IMAGE.read_bytes()).decode()
    ext = REF_IMAGE.suffix.lstrip(".")
    data_url = f"data:image/{ext};base64,{img_b64}"

    print(f"Analyzing style from {REF_IMAGE.name} via {VISION_MODEL} …")

    resp = client.chat.completions.create(
        model=VISION_MODEL,
        messages=[{
            "role": "user",
            "content": [
                {"type": "image_url", "image_url": {"url": data_url}},
                {"type": "text",      "text": VISION_PROMPT},
            ],
        }],
        max_tokens=400,
    )

    descriptor = resp.choices[0].message.content.strip()
    print(f"\n── Style Descriptor ──────────────────────────────────────────\n")
    print(descriptor)
    print(f"\n──────────────────────────────────────────────────────────────")

    STYLE_JSON.parent.mkdir(parents=True, exist_ok=True)
    STYLE_JSON.write_text(
        json.dumps({"style_descriptor": descriptor, "ref_image": REF_IMAGE.name}, indent=2, ensure_ascii=False),
        encoding="utf-8"
    )
    print(f"\n✓  Saved → {STYLE_JSON}")
    print("Now run: python generate_art.py --force  to regenerate all assets with this style.")

if __name__ == "__main__":
    main()
