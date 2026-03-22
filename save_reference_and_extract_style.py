#!/usr/bin/env python3
"""
save_reference_and_extract_style.py
1. Saves a faithful reproduction of the reference image to Assets/Art/Reference/style_ref.png
2. Runs vision API on it to extract style descriptor
3. Saves style_descriptor.json
4. Updates generate_art.py style strings
"""
import base64, sys, json, re
from pathlib import Path
from openai import OpenAI

ENV_FILE    = Path(r"C:\dev\DIGITAL LABOUR\DIGITAL LABOUR\.env")
REF_DIR     = Path(__file__).parent / "Assets" / "Art" / "Reference"
REF_IMAGE   = REF_DIR / "style_ref.png"
STYLE_JSON  = REF_DIR / "style_descriptor.json"
XAI_BASE    = "https://api.x.ai/v1"
IMAGE_MODEL  = "grok-imagine-image"
VISION_MODEL = "grok-2-vision-latest"

# Faithful reproduction prompt for the reference style
REF_PROMPT = (
    "anime cel-shaded game character art, portrait orientation, "
    "Egyptian goddess warrior, close-up bust portrait, "
    "dark skin, long black hair, intense amber/gold eyes, "
    "ornate gold Egyptian headdress with lapis lazuli blue gem inlays, "
    "gold collar necklace with Egyptian pectoral armor, ankh symbol at chest, "
    "wielding Was scepter staff wrapped in dark leather, "
    "holding a glowing teal magical stone tablet covered in luminous hieroglyphs, "
    "ancient stone temple columns with carved hieroglyphs in background, "
    "warm amber and orange fire-lit atmosphere, floating ember particles, "
    "dramatic lighting from below and sides, deep warm shadows, "
    "vibrant saturated color palette: gold, teal, amber, deep brown, "
    "clean sharp cel-shading with defined outline strokes, "
    "epic fantasy game art style, no text"
)

VISION_PROMPT = (
    "Analyze this game art image and produce a detailed art style descriptor for AI image generation. "
    "Describe ONLY the visual style — rendering technique, color palette, lighting, line work, texture, atmosphere. "
    "Output a single dense comma-separated prompt fragment with no bullet points or headers. "
    "Start directly with the style, no preamble."
)

def _load_key() -> str:
    for line in ENV_FILE.read_text(encoding="utf-8").splitlines():
        if line.startswith("GROK_API_KEY="):
            return line.split("=", 1)[1].strip()
    sys.exit("GROK_API_KEY not found")

def main():
    REF_DIR.mkdir(parents=True, exist_ok=True)
    client = OpenAI(api_key=_load_key(), base_url=XAI_BASE)

    # ── Step 1: Generate + save reference image ──────────────────────────────
    print("[1/3] Generating reference image … ", end="", flush=True)
    resp = client.images.generate(
        model=IMAGE_MODEL, prompt=REF_PROMPT, n=1, response_format="b64_json"
    )
    img = base64.b64decode(resp.data[0].b64_json)
    REF_IMAGE.write_bytes(img)
    print(f"✓  {len(img)//1024}KB → {REF_IMAGE.name}")

    # ── Step 2: Vision — extract style descriptor ─────────────────────────────
    print("[2/3] Extracting style descriptor via vision … ", end="", flush=True)
    img_b64  = base64.b64encode(img).decode()
    data_url = f"data:image/png;base64,{img_b64}"
    vresp = client.chat.completions.create(
        model=VISION_MODEL,
        messages=[{"role": "user", "content": [
            {"type": "image_url", "image_url": {"url": data_url}},
            {"type": "text",      "text": VISION_PROMPT},
        ]}],
        max_tokens=350,
    )
    descriptor = vresp.choices[0].message.content.strip()
    print("✓")
    print(f"\n  Style: {descriptor[:120]}…\n")

    STYLE_JSON.write_text(
        json.dumps({"style_descriptor": descriptor, "ref_image": "style_ref.png"}, indent=2, ensure_ascii=False),
        encoding="utf-8"
    )
    print(f"  Saved → {STYLE_JSON.name}")

    # ── Step 3: Patch generate_art.py style strings ───────────────────────────
    print("[3/3] Patching generate_art.py … ", end="", flush=True)
    ga = Path(__file__).parent / "generate_art.py"
    src = ga.read_text(encoding="utf-8")

    # Build new style fragments from the descriptor
    base_style = descriptor

    new_panel = (
        f'(\n'
        f'    "{base_style}, "\n'
        f'    "comic panel composition, dramatic scene, no text, no word balloons, no panel borders"\n'
        f')'
    )
    new_icon = (
        f'(\n'
        f'    "{base_style}, "\n'
        f'    "flat round icon, single central symbol, very dark background, no text, no letters, clean graphic design"\n'
        f')'
    )
    new_cover = (
        f'(\n'
        f'    "{base_style}, "\n'
        f'    "game cover art, portrait orientation, cinematic composition, no title text, no speech bubbles"\n'
        f')'
    )

    # Replace the three style fragment blocks
    src = re.sub(
        r'_PANEL\s*=\s*\([^)]*\)',
        f'_PANEL = {new_panel}',
        src, flags=re.DOTALL
    )
    src = re.sub(
        r'_ICON\s*=\s*\([^)]*\)',
        f'_ICON = {new_icon}',
        src, flags=re.DOTALL
    )
    src = re.sub(
        r'_COVER\s*=\s*\([^)]*\)',
        f'_COVER = {new_cover}',
        src, flags=re.DOTALL
    )

    # Also fix response_format to always use b64_json
    src = src.replace(
        "response = client.images.generate(\n                model=IMAGE_MODEL,\n                prompt=asset[\"prompt\"],\n                n=1,\n            )",
        "response = client.images.generate(\n                model=IMAGE_MODEL,\n                prompt=asset[\"prompt\"],\n                n=1,\n                response_format=\"b64_json\",\n            )"
    )

    ga.write_text(src, encoding="utf-8")
    print("✓")

    print("\n✓  All done. Run: python generate_art.py --force")
    import subprocess
    subprocess.Popen(["cmd", "/c", "start", "", str(REF_IMAGE)])

if __name__ == "__main__":
    main()
