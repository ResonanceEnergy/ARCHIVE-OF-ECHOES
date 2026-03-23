#!/usr/bin/env python3
"""
pipeline.py — One-command master automation runner for Archive of Echoes

Steps executed in order:
  1. generate_art.py          — AI-generated panel / UI artwork (xAI Aurora API)
  2. generate_audio.py        — procedurally synthesised WAV audio (stdlib only)
  3. Unity batchmode          — data build + scene build + asset imports
  4. generate_art_polish.py   — PIL overlay textures (grain, vignette, glow)
  5. gen_appstore.py          — App Store metadata (description, keywords, spec)

Usage:
    python pipeline.py                     # full run
    python pipeline.py --skip-art          # skip art generation
    python pipeline.py --skip-audio        # skip audio generation
    python pipeline.py --skip-unity        # skip Unity batchmode step
    python pipeline.py --skip-polish       # skip art-polish overlays
    python pipeline.py --skip-appstore     # skip App Store metadata
    python pipeline.py --dry-run           # print what would happen, run nothing
    python pipeline.py --force-art         # force-regenerate all art
    python pipeline.py --force-audio       # force-regenerate all audio
    python pipeline.py --force-polish      # force-regenerate all polish overlays

Requirements:
    - Python 3.8+
    - generate_art.py needs: pip install openai pillow  (for art step)
    - generate_audio.py needs: nothing (stdlib only)
    - generate_art_polish.py needs: pip install pillow
    - Unity Hub installed at the default Windows path
    - Unity project root = this script's directory
"""

import argparse
import io
import subprocess
import sys
import time
from pathlib import Path

if sys.stdout.encoding and sys.stdout.encoding.lower() != "utf-8":
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
if sys.stderr.encoding and sys.stderr.encoding.lower() != "utf-8":
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8", errors="replace")

# ── Paths ───────────────────────────────────────────────────────────────────────

PROJECT_ROOT   = Path(__file__).parent
UNITY_HUB_ROOT = Path(r"C:\Program Files\Unity\Hub\Editor")
LOG_FILE       = PROJECT_ROOT / "pipeline_build.log"

GENERATE_ART    = PROJECT_ROOT / "generate_art.py"
GENERATE_AUDIO  = PROJECT_ROOT / "generate_audio.py"
GENERATE_POLISH = PROJECT_ROOT / "generate_art_polish.py"
GEN_APPSTORE    = PROJECT_ROOT / "gen_appstore.py"

BATCH_METHOD   = "ArchiveOfEchoes.Editor.ArchiveBootstrapper.BatchBuildAll"

# ── Utilities ────────────────────────────────────────────────────────────────────

def _banner(text: str) -> None:
    bar = "─" * 60
    print(f"\n{bar}")
    print(f"  {text}")
    print(bar)


def _run(cmd: list[str], *, dry_run: bool, label: str, capture: bool = False) -> int:
    """Run a subprocess, return exit code.  Always prints the command."""
    print(f"  $ {' '.join(str(c) for c in cmd)}")
    if dry_run:
        print("  [dry-run] skipped")
        return 0
    start = time.time()
    if capture:
        result = subprocess.run(cmd, capture_output=False, text=True)
    else:
        result = subprocess.run(cmd)
    elapsed = time.time() - start
    status  = "✓" if result.returncode == 0 else "✗"
    print(f"  {status}  {label} ({elapsed:.1f}s, exit {result.returncode})")
    return result.returncode


# ── Unity detection ──────────────────────────────────────────────────────────────

def _find_unity_exe() -> Path:
    """
    Scan C:/Program Files/Unity/Hub/Editor/ for installed versions.
    Returns the path to the highest-version Unity.exe found.
    """
    if not UNITY_HUB_ROOT.is_dir():
        raise FileNotFoundError(
            f"Unity Hub Editor directory not found: {UNITY_HUB_ROOT}\n"
            "Install Unity via Unity Hub or set UNITY_HUB_ROOT in pipeline.py."
        )

    versions = sorted(
        (d for d in UNITY_HUB_ROOT.iterdir() if d.is_dir()),
        key=lambda d: _version_key(d.name),
        reverse=True,
    )
    if not versions:
        raise FileNotFoundError(f"No Unity installations found in {UNITY_HUB_ROOT}")

    for v in versions:
        exe = v / "Editor" / "Unity.exe"
        if exe.exists():
            return exe

    raise FileNotFoundError(
        f"No Unity.exe found in any subdirectory of {UNITY_HUB_ROOT}"
    )


def _version_key(version_str: str) -> tuple:
    """Convert '2022.3.15f1' → (2022, 3, 15, 0) for sorting."""
    import re
    parts = re.findall(r"\d+", version_str)
    return tuple(int(p) for p in parts[:4]) + (0,) * (4 - len(parts[:4]))


# ── Step runners ─────────────────────────────────────────────────────────────────

def step_art(dry_run: bool, force: bool) -> int:
    _banner("STEP 1 — Generate Art  (xAI Aurora API)")
    if not GENERATE_ART.exists():
        print(f"  WARN: {GENERATE_ART} not found — skipping.")
        return 0
    cmd = [sys.executable, str(GENERATE_ART)]
    if force:
        cmd.append("--force")
    return _run(cmd, dry_run=dry_run, label="generate_art.py")


def step_audio(dry_run: bool, force: bool) -> int:
    _banner("STEP 2 — Generate Audio  (stdlib synthesis)")
    if not GENERATE_AUDIO.exists():
        print(f"  WARN: {GENERATE_AUDIO} not found — skipping.")
        return 0
    cmd = [sys.executable, str(GENERATE_AUDIO)]
    if force:
        cmd.append("--force")
    return _run(cmd, dry_run=dry_run, label="generate_audio.py")


def step_unity(dry_run: bool) -> int:
    _banner("STEP 3 — Unity batchmode build")

    try:
        unity_exe = _find_unity_exe()
    except FileNotFoundError as e:
        print(f"  ERROR: {e}")
        return 1

    print(f"  Unity : {unity_exe}")
    print(f"  Method: {BATCH_METHOD}")
    print(f"  Log   : {LOG_FILE}")

    cmd = [
        str(unity_exe),
        "-batchmode",
        "-nographics",
        "-projectPath", str(PROJECT_ROOT),
        "-executeMethod", BATCH_METHOD,
        "-logFile", str(LOG_FILE),
        "-quit",
    ]
    rc = _run(cmd, dry_run=dry_run, label="Unity BatchBuildAll")

    if not dry_run:
        _tail_log(LOG_FILE, lines=30)

    return rc


def _tail_log(log_path: Path, lines: int = 30) -> None:
    """Print the last N lines of the Unity log file."""
    if not log_path.exists():
        print(f"  (log file not found: {log_path})")
        return
    print(f"\n  ── Last {lines} lines of {log_path.name} ──────────────────────")
    all_lines = log_path.read_text(encoding="utf-8", errors="replace").splitlines()
    for line in all_lines[-lines:]:
        print(f"  {line}")
    print("  ────────────────────────────────────────────────────────────")


def step_polish(dry_run: bool, force: bool) -> int:
    _banner("STEP 4 — Art Polish  (PIL overlay textures)")
    if not GENERATE_POLISH.exists():
        print(f"  WARN: {GENERATE_POLISH} not found — skipping.")
        return 0
    cmd = [sys.executable, str(GENERATE_POLISH)]
    if force:
        cmd.append("--force")
    return _run(cmd, dry_run=dry_run, label="generate_art_polish.py")


def step_appstore(dry_run: bool) -> int:
    _banner("STEP 5 — App Store Metadata  (description, keywords, spec)")
    if not GEN_APPSTORE.exists():
        print(f"  WARN: {GEN_APPSTORE} not found — skipping.")
        return 0
    cmd = [sys.executable, str(GEN_APPSTORE)]
    return _run(cmd, dry_run=dry_run, label="gen_appstore.py")


# ── Main ─────────────────────────────────────────────────────────────────────────

def main() -> None:
    parser = argparse.ArgumentParser(
        description="Archive of Echoes — full asset generation + Unity build pipeline"
    )
    parser.add_argument("--skip-art",     action="store_true", help="Skip art generation")
    parser.add_argument("--skip-audio",   action="store_true", help="Skip audio generation")
    parser.add_argument("--skip-unity",   action="store_true", help="Skip Unity batchmode")
    parser.add_argument("--skip-polish",  action="store_true", help="Skip art-polish overlays")
    parser.add_argument("--skip-appstore",action="store_true", help="Skip App Store metadata")
    parser.add_argument("--force-art",    action="store_true", help="Force-regenerate all art")
    parser.add_argument("--force-audio",  action="store_true", help="Force-regenerate all audio")
    parser.add_argument("--force-polish", action="store_true", help="Force-regenerate polish overlays")
    parser.add_argument("--dry-run",      action="store_true", help="Print commands, run nothing")
    args = parser.parse_args()

    if args.dry_run:
        print("\n  *** DRY-RUN MODE — no files will be created or modified ***")

    errors = []

    if not args.skip_art:
        rc = step_art(args.dry_run, args.force_art)
        if rc != 0:
            errors.append(f"Art generation failed (exit {rc})")

    if not args.skip_audio:
        rc = step_audio(args.dry_run, args.force_audio)
        if rc != 0:
            errors.append(f"Audio generation failed (exit {rc})")

    if not args.skip_unity:
        rc = step_unity(args.dry_run)
        if rc != 0:
            errors.append(f"Unity batchmode failed (exit {rc})")

    if not args.skip_polish:
        rc = step_polish(args.dry_run, args.force_polish)
        if rc != 0:
            errors.append(f"Art polish failed (exit {rc})")

    if not args.skip_appstore:
        rc = step_appstore(args.dry_run)
        if rc != 0:
            errors.append(f"App Store metadata failed (exit {rc})")

    # ── Summary ──────────────────────────────────────────────────────────────────
    _banner("PIPELINE COMPLETE")
    if errors:
        for e in errors:
            print(f"  ✗ {e}")
        print()
        print("  Exit 1 — one or more steps failed.")
        sys.exit(1)
    else:
        print("  ✓  All steps succeeded.")
        if not args.skip_unity and not args.dry_run:
            print(f"  ✓  Unity log: {LOG_FILE}")
        print()
        print("  Open Unity and check:")
        print("    • Assets/Audio/  — SFX + drone WAV files (18 total)")
        print("    • Assets/Art/UI/ — polish overlays (grain, vignette, ink lines, glow)")
        print("    • Assets/AppStore/ — store_description.txt, metadata.json, keywords.txt")
        print("    • Lens SOs         — ambientNote assigned")
        print("    • AudioManagerPrefab — all 12 SFX clips assigned")
        print("    • AccessibilityDefaults.asset — font scale 1.0, Normal contrast")
        print()


if __name__ == "__main__":
    main()
