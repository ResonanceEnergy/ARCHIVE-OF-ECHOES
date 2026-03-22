"""List all xAI API models to find the correct image generation model name."""
from pathlib import Path
from openai import OpenAI

env = {}
for line in Path(r"C:\dev\DIGITAL LABOUR\DIGITAL LABOUR\.env").read_text().splitlines():
    line = line.strip()
    if line and not line.startswith("#") and "=" in line:
        k, _, v = line.partition("=")
        env[k.strip()] = v.strip().strip('"')

client = OpenAI(api_key=env["GROK_API_KEY"], base_url="https://api.x.ai/v1")
models = client.models.list()
image_keywords = ("image", "aurora", "flux", "vision", "generat", "draw", "diffus")
print("=== xAI models ===")
for m in sorted(models.data, key=lambda x: x.id):
    tag = "  [IMAGE?]" if any(k in m.id.lower() for k in image_keywords) else ""
    print(f"  {m.id}{tag}")
