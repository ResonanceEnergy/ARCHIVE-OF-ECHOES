#!/usr/bin/env python3
"""
build_ios.py — Automated iOS/Xcode build pipeline for Archive of Echoes

One command to export from Unity and open in Xcode for final build.

Usage:
    python build_ios.py              # full build, open Xcode when done
    python build_ios.py --no-open    # build but don't auto-open Xcode
    python build_ios.py --dry-run    # show what would happen
    python build_ios.py --clean      # remove old build and rebuild
    python build_ios.py --dev        # builds with Debug symbols for TestFlight
    python build_ios.py --prod       # builds optimized Release for App Store (default)

Requirements:
    - macOS 12+ with Xcode command-line tools
    - Unity 2022.3+ installed at /Applications/Unity or ~/Development/Unity
    - Archive of Echoes project at the script's directory
"""

import argparse
import json
import os
import plistlib
import shutil
import subprocess
import sys
import time
from datetime import datetime
from pathlib import Path

# ── Configuration ───────────────────────────────────────────────────────────────

PROJECT_ROOT = Path(__file__).parent.resolve()
BUILDS_DIR = PROJECT_ROOT / "Builds"
IOS_BUILD_DIR = BUILDS_DIR / "iOS"
IOS_SIM_BUILD_DIR = BUILDS_DIR / "iOSSim"
XCODE_PROJECT_NAME = "Unity-iPhone.xcodeproj"
LOG_FILE = PROJECT_ROOT / f"build_ios_{datetime.now().strftime('%Y%m%d_%H%M%S')}.log"

# Unity paths (macOS) — searched in order, first match wins
UNITY_PATHS = [
    Path("/Applications/Unity/Hub/Editor"),               # Unity Hub (default install, singular)
    Path("/Applications/Unity/Hub/Editors"),              # Unity Hub (alt spelling)
    Path.home() / "Library" / "Application Support" / "Unity Hub" / "editors",  # Hub alt path
    Path("/Applications/Unity"),                          # Standalone install
    Path.home() / "Development" / "Unity",               # Custom dev path
    Path.home() / "Unity",                               # Home install
]

DEVICE_BATCH_METHOD = "ArchiveOfEchoes.Editor.ArchiveBootstrapper.BatchBuildIos"
SIMULATOR_BATCH_METHOD = "ArchiveOfEchoes.Editor.ArchiveBootstrapper.BatchBuildIosSimulator"

# ── Logging ──────────────────────────────────────────────────────────────────

def log(msg: str, level: str = "INFO") -> None:
    """Print to console and log file."""
    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    full_msg = f"[{timestamp}] {level:8s} {msg}"
    print(full_msg)
    with open(LOG_FILE, "a", encoding="utf-8") as f:
        f.write(full_msg + "\n")


def banner(text: str) -> None:
    """Print a section banner."""
    bar = "━" * 70
    print(f"\n{bar}")
    print(f"  {text}")
    print(bar)
    log(text)


# ── Utilities ────────────────────────────────────────────────────────────────

def run_cmd(cmd: list[str], *, dry_run: bool = False, capture: bool = False) -> tuple[int, str]:
    """
    Run a shell command.
    Returns: (exit_code, stdout_if_capture_else_empty_string)
    """
    cmd_str = " ".join(str(c) for c in cmd)
    log(f"$ {cmd_str}")
    
    if dry_run:
        log("  [DRY-RUN] skipped", level="SKIP")
        return 0, ""
    
    try:
        start = time.time()
        if capture:
            result = subprocess.run(cmd, capture_output=True, text=True, timeout=600)
            elapsed = time.time() - start
            log(f"  ✓ completed in {elapsed:.1f}s", level="OK")
            return result.returncode, result.stdout
        else:
            result = subprocess.run(cmd, timeout=600)
            elapsed = time.time() - start
            status = "✓" if result.returncode == 0 else "✗"
            log(f"  {status} exit code {result.returncode} ({elapsed:.1f}s)", 
                level="OK" if result.returncode == 0 else "ERROR")
            return result.returncode, ""
    except subprocess.TimeoutExpired:
        log(f"  ✗ timeout (>10min)", level="ERROR")
        return 124, ""
    except Exception as e:
        log(f"  ✗ exception: {e}", level="ERROR")
        return 1, ""


def find_unity_exe() -> Path:
    """Find the latest Unity executable on macOS."""
    log("Searching for Unity installation...")

    hub_app = Path("/Applications/Unity Hub.app")
    hub_only = hub_app.exists()

    for base_path in UNITY_PATHS:
        if not base_path.exists():
            continue

        log(f"  Checking {base_path}...")

        # Hub/Editors directory — look for versioned sub-folders
        candidate_dirs = sorted(
            [d for d in base_path.iterdir() if d.is_dir()],
            key=lambda p: _version_tuple(p.name),
            reverse=True,
        )
        for version_dir in candidate_dirs:
            unity_exe = version_dir / "Unity.app" / "Contents" / "MacOS" / "Unity"
            if unity_exe.exists():
                log(f"  ✓ Found Unity {version_dir.name} at {unity_exe.parent.parent}")
                return unity_exe

        # Direct Unity.app at the path itself
        unity_exe = base_path / "Unity.app" / "Contents" / "MacOS" / "Unity"
        if unity_exe.exists():
            log(f"  ✓ Found Unity at {base_path}")
            return unity_exe

    # Nothing found — give targeted instructions
    if hub_only:
        raise FileNotFoundError(
            "Unity Hub is installed but NO Unity Editor version is installed.\n\n"
            "To install the Unity Editor:\n"
            "  1. Open Unity Hub  (Applications → Unity Hub)\n"
            "  2. Click \"Installs\" in the left sidebar\n"
            "  3. Click \"Install Editor\"\n"
            "  4. Choose Unity 6 LTS (or 2022.3 LTS) and add the iOS Build Support module\n"
            "  5. Re-run this script once installation completes\n"
        )
    raise FileNotFoundError(
        "Could not find Unity installation.\n"
        "Checked:\n"
        + "\n".join(f"  - {p}" for p in UNITY_PATHS) + "\n\n"
        "Options:\n"
        "  A) Install Unity via Unity Hub (https://unity.com/download)\n"
        "  B) Add your Unity path to UNITY_PATHS in build_ios.py\n"
    )


def _version_tuple(version_str: str) -> tuple:
    """Convert version string like '2023.2.15f1' to a comparable tuple."""
    parts = version_str.replace("f", ".").replace("a", ".").replace("b", ".").replace("rc", ".")
    try:
        return tuple(int(p) for p in parts.split(".")[:3])
    except (ValueError, IndexError):
        return (0, 0, 0)


def get_export_dir(simulator: bool) -> Path:
    """Return the Unity export directory for the selected target."""
    return IOS_SIM_BUILD_DIR if simulator else IOS_BUILD_DIR


def get_batch_method(simulator: bool) -> str:
    """Return the Unity batch method for the selected target."""
    return SIMULATOR_BATCH_METHOD if simulator else DEVICE_BATCH_METHOD


def get_xcode_project_dir(export_dir: Path) -> Path:
    """Return the generated Xcode project path inside a Unity iOS export."""
    return export_dir / XCODE_PROJECT_NAME


def ensure_app_store_icon(export_dir: Path) -> bool:
    """Ensure AppIcon.appiconset contains a 1024x1024 App Store icon entry and file."""
    appicon_dir = export_dir / "Unity-iPhone" / "Images.xcassets" / "AppIcon.appiconset"
    contents_path = appicon_dir / "Contents.json"
    target_filename = "Icon-AppStore-1024.png"
    target_path = appicon_dir / target_filename

    if not contents_path.exists():
        log(f"  ⚠ App icon catalog not found at {contents_path}", level="WARN")
        return False

    with open(contents_path, "r", encoding="utf-8") as f:
        contents = json.load(f)

    images = contents.get("images", [])
    has_marketing_entry = any(img.get("idiom") == "ios-marketing" for img in images)

    if not target_path.exists():
        source_candidates = [
            PROJECT_ROOT / "Assets" / "Art" / "Cover" / "cover_game_official.png",
            PROJECT_ROOT / "Assets" / "Art" / "Cover" / "cover_issue_00.png",
            PROJECT_ROOT / "Assets" / "Art" / "Cover" / "cover_issue_01.png",
        ]
        source_icon = next((p for p in source_candidates if p.exists()), None)
        if source_icon is None:
            log("  ⚠ Could not find a source PNG for 1024 app icon", level="WARN")
            return False

        result = subprocess.run(
            ["sips", "-z", "1024", "1024", str(source_icon), "--out", str(target_path)],
            capture_output=True,
            text=True,
        )
        if result.returncode != 0:
            log("  ⚠ Failed to generate 1024 app icon via sips", level="WARN")
            return False

    if not has_marketing_entry:
        images.append(
            {
                "filename": target_filename,
                "idiom": "ios-marketing",
                "scale": "1x",
                "size": "1024x1024",
            }
        )
    else:
        for image in images:
            if image.get("idiom") == "ios-marketing":
                image["filename"] = target_filename
                image["scale"] = "1x"
                image["size"] = "1024x1024"

    contents["images"] = images
    with open(contents_path, "w", encoding="utf-8") as f:
        json.dump(contents, f, indent=2)
        f.write("\n")

    log("  ✓ Ensured 1024x1024 App Store icon in AppIcon.appiconset")
    return True


def disable_shell_script_dependency_warning(xcode_project_dir: Path) -> bool:
    """Patch Unity shell script phase so Xcode no longer warns about missing outputs."""
    pbxproj_path = xcode_project_dir / "project.pbxproj"
    if not pbxproj_path.exists():
        log(f"  ⚠ project.pbxproj not found at {pbxproj_path}", level="WARN")
        return False

    with open(pbxproj_path, "r", encoding="utf-8") as f:
        text = f.read()

    anchor = "C62A2A42F32E085EF849CF0B /* ShellScript */ = {"
    start = text.find(anchor)
    if start == -1:
        log("  ⚠ ShellScript phase anchor not found; skipping script warning patch", level="WARN")
        return False

    end = text.find("\n\t\t};", start)
    if end == -1:
        log("  ⚠ Could not locate end of ShellScript phase block", level="WARN")
        return False

    block = text[start:end]
    changed = False

    if "basedOnDependencyAnalysis" not in block:
        marker = "buildActionMask = 2147483647;\n"
        if marker in block:
            block = block.replace(
                marker,
                marker + "\t\t\tbasedOnDependencyAnalysis = 0;\n",
                1,
            )
            changed = True

    # Xcode may still warn unless the phase declares at least one output path.
    if "outputPaths = (" not in block:
        files_marker = "files = (\n\t\t\t);\n"
        if files_marker in block:
            block = block.replace(
                files_marker,
                files_marker
                + "\t\t\toutputPaths = (\n"
                + "\t\t\t\t\"$(CONFIGURATION_BUILD_DIR)/libGameAssembly.a\",\n"
                + "\t\t\t);\n",
                1,
            )
            changed = True

    if not changed:
        return True

    updated = text[:start] + block + text[end:]

    with open(pbxproj_path, "w", encoding="utf-8") as f:
        f.write(updated)

    log("  ✓ Patched Xcode shell script phase (dependency analysis + output paths)")
    return True


def step_postprocess_xcode_export(*, export_dir: Path, xcode_project_dir: Path, dry_run: bool = False) -> bool:
    """Apply small deterministic fixes to generated Xcode projects."""
    banner("STEP 2.5: Post-process Xcode Export")

    if dry_run:
        log("  [DRY-RUN] Would patch app icon and shell script warning", level="SKIP")
        return True

    ok_icon = ensure_app_store_icon(export_dir)
    ok_script = disable_shell_script_dependency_warning(xcode_project_dir)
    return ok_icon or ok_script


# ── Build Steps ──────────────────────────────────────────────────────────────

def step_clean(export_dir: Path, *, dry_run: bool = False) -> bool:
    """Remove old build artifacts."""
    banner("STEP 1: Clean Build Directory")
    
    if export_dir.exists():
        log(f"Removing old build at {export_dir}...")
        if not dry_run:
            shutil.rmtree(export_dir)
        log("  ✓ Cleaned")
    else:
        log("  No previous build found")
    
    return True


def step_unity_export(
    unity_exe: Path,
    *,
    export_dir: Path,
    batch_method: str,
    simulator: bool = False,
    dry_run: bool = False,
    development: bool = False,
) -> bool:
    """Export iOS project from Unity."""
    banner("STEP 2: Export from Unity (Batch Mode)")
    
    # Ensure build directory exists
    export_dir.parent.mkdir(parents=True, exist_ok=True)
    
    # Build command
    cmd = [
        str(unity_exe),
        "-batchmode",
        "-quit",
        "-projectPath", str(PROJECT_ROOT),
        "-buildTarget", "iOS",
        "-logFile", "-",
        "-executeMethod", batch_method,
    ]
    
    log(f"Exporting to: {export_dir}")
    log(f"Target: {'iOS Simulator' if simulator else 'iPhone / device'}")
    log(f"Development build: {development}")
    
    exit_code, _ = run_cmd(cmd, dry_run=dry_run)
    
    if exit_code != 0:
        log(f"  ✗ Unity export failed (exit {exit_code})", level="ERROR")
        return False
    
    # Verify Xcode project was created
    if not dry_run:
        xcode_project_dir = get_xcode_project_dir(export_dir)
        if not xcode_project_dir.exists():
            log(f"  ✗ Xcode project not found at {xcode_project_dir}", level="ERROR")
            return False
        log(f"  ✓ Xcode project created at {xcode_project_dir}")
        step_postprocess_xcode_export(export_dir=export_dir, xcode_project_dir=xcode_project_dir, dry_run=dry_run)
    
    return True


def step_xcode_build(*, xcode_project_dir: Path, dry_run: bool = False, development: bool = False) -> bool:
    """Build in Xcode (optional—mostly we just export and let user build)."""
    banner("STEP 3: Validate Xcode Project")

    if dry_run:
        log(f"  [DRY-RUN] Would validate {xcode_project_dir}", level="SKIP")
        return True

    if not xcode_project_dir.exists():
        log(f"  ✗ Xcode project not found at {xcode_project_dir}", level="ERROR")
        return False

    # Just verify it's a valid Xcode project
    pbxproj = xcode_project_dir / "project.pbxproj"
    if not pbxproj.exists():
        log(f"  ✗ project.pbxproj not found", level="ERROR")
        return False

    log(f"  ✓ Xcode project is valid")
    return True


def find_booted_simulator() -> tuple[str, str] | None:
    """Return (name, uuid) for the first booted iOS simulator, if any."""
    exit_code, output = run_cmd(["xcrun", "simctl", "list", "devices", "booted"], capture=True)
    if exit_code != 0:
        return None

    for raw_line in output.splitlines():
        line = raw_line.strip()
        if "iPhone" not in line or "(Booted)" not in line:
            continue
        parts = line.split(" (")
        if len(parts) < 3:
            continue
        name = parts[0]
        uuid = parts[1].rstrip(")")
        return name, uuid

    return None


def find_available_simulator() -> tuple[str, str] | None:
    """Return (name, uuid) for the first available iPhone simulator."""
    exit_code, output = run_cmd(["xcrun", "simctl", "list", "devices", "available"], capture=True)
    if exit_code != 0:
        return None

    for raw_line in output.splitlines():
        line = raw_line.strip()
        if "iPhone" not in line or "unavailable" in line:
            continue
        parts = line.split(" (")
        if len(parts) < 3:
            continue
        name = parts[0]
        uuid = parts[1].rstrip(")")
        return name, uuid

    return None


def ensure_booted_simulator(*, dry_run: bool = False) -> tuple[str, str] | None:
    """Return a booted simulator, booting the first available iPhone if needed."""
    booted = find_booted_simulator()
    if booted is not None:
        return booted

    available = find_available_simulator()
    if available is None:
        return None

    simulator_name, simulator_uuid = available
    log(f"No booted simulator found. Booting {simulator_name} ({simulator_uuid})...")

    exit_code, _ = run_cmd(["xcrun", "simctl", "boot", simulator_uuid], dry_run=dry_run)
    if exit_code != 0:
        log("  ✗ Failed to boot simulator", level="ERROR")
        return None

    run_cmd(["open", "-a", "Simulator", "--args", "-CurrentDeviceUDID", simulator_uuid], dry_run=dry_run)

    if dry_run:
        return simulator_name, simulator_uuid

    for _ in range(20):
        booted = find_booted_simulator()
        if booted is not None:
            return booted
        time.sleep(1)

    log("  ✗ Simulator did not finish booting in time", level="ERROR")
    return None


def step_run_simulator(*, xcode_project_dir: Path, dry_run: bool = False) -> bool:
    """Build, install, and launch the app on a booted iOS simulator."""
    banner("STEP 4: Build + Run on Booted Simulator")

    booted = ensure_booted_simulator(dry_run=dry_run)
    if booted is None:
        log("  ✗ No usable iPhone simulator found.", level="ERROR")
        return False

    simulator_name, simulator_uuid = booted
    derived_data = BUILDS_DIR / "DerivedDataSim"
    app_products_dir = derived_data / "Build" / "Products" / "Debug-iphonesimulator"

    log(f"Booted simulator: {simulator_name} ({simulator_uuid})")

    build_cmd = [
        "xcodebuild",
        "-project", str(xcode_project_dir),
        "-scheme", "Unity-iPhone",
        "-configuration", "Debug",
        "-sdk", "iphonesimulator",
        "-destination", "generic/platform=iOS Simulator",
        "-derivedDataPath", str(derived_data),
        "CODE_SIGNING_ALLOWED=NO",
        "build",
    ]

    exit_code, _ = run_cmd(build_cmd, dry_run=dry_run)
    if exit_code != 0:
        log("  ✗ Simulator build failed", level="ERROR")
        return False

    if dry_run:
        log("  [DRY-RUN] Would install and launch the built app", level="SKIP")
        return True

    app_candidates = sorted(app_products_dir.glob("*.app"))
    if not app_candidates:
        log(f"  ✗ Built app not found under {app_products_dir}", level="ERROR")
        return False

    app_path = app_candidates[0]
    with open(app_path / "Info.plist", "rb") as plist_file:
        bundle_id = plistlib.load(plist_file)["CFBundleIdentifier"]

    log(f"Installing {app_path.name}...")
    exit_code, _ = run_cmd(["xcrun", "simctl", "install", simulator_uuid, str(app_path)], dry_run=dry_run)
    if exit_code != 0:
        log("  ✗ Simulator install failed", level="ERROR")
        return False

    log(f"Launching {bundle_id}...")
    exit_code, _ = run_cmd(["xcrun", "simctl", "launch", simulator_uuid, bundle_id], dry_run=dry_run)
    if exit_code != 0:
        log("  ✗ Simulator launch failed", level="ERROR")
        return False

    log("  ✓ App installed and launched in Simulator")
    return True


def step_open_xcode(*, xcode_project_dir: Path, dry_run: bool = False, no_open: bool = False) -> bool:
    """Open the Xcode project."""
    banner("STEP 5: Open in Xcode")
    
    if no_open:
        log("  Skipping (--no-open flag)")
        return True

    if dry_run:
        log(f"  [DRY-RUN] Would open {xcode_project_dir} in Xcode", level="SKIP")
        return True

    if not xcode_project_dir.exists():
        log(f"  ✗ Xcode project not found", level="ERROR")
        return False

    log(f"Opening {xcode_project_dir} in Xcode...")
    exit_code, _ = run_cmd(["open", "-a", "Xcode", str(xcode_project_dir)], dry_run=dry_run)
    
    if exit_code == 0:
        log("  ✓ Xcode is opening...")
    
    return exit_code == 0


# ── Main ─────────────────────────────────────────────────────────────────────

def main():
    parser = argparse.ArgumentParser(
        description="Build Archive of Echoes for iOS",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python build_ios.py                # Full build + open Xcode
    python build_ios.py --simulator    # Export simulator-compatible Xcode project
    python build_ios.py --simulator --run  # Build/install/launch on a booted simulator
  python build_ios.py --no-open      # Build but don't open Xcode
  python build_ios.py --dry-run      # Show what would happen
  python build_ios.py --clean        # Clean build (remove old artifacts)
  python build_ios.py --dev          # Development build (debug symbols)
        """
    )
    
    parser.add_argument("--no-open", action="store_true", help="Don't auto-open Xcode after build")
    parser.add_argument("--dry-run", action="store_true", help="Show commands without running them")
    parser.add_argument("--clean", action="store_true", help="Remove old build before starting")
    parser.add_argument("--dev", action="store_true", help="Development build (debug symbols)")
    parser.add_argument("--prod", action="store_true", help="Production build (default, optimized)")
    parser.add_argument("--simulator", action="store_true", help="Export for iOS Simulator instead of physical device")
    parser.add_argument("--run", action="store_true", help="Build/install/launch on a booted simulator after export")
    
    args = parser.parse_args()
    
    # Initialize log
    log(f"Archive of Echoes iOS Build", level="START")
    log(f"Project: {PROJECT_ROOT}")
    log(f"Log: {LOG_FILE}")
    export_dir = get_export_dir(args.simulator)
    xcode_project_dir = get_xcode_project_dir(export_dir)
    batch_method = get_batch_method(args.simulator)
    
    try:
        # Step 0: Clean if requested
        if args.clean:
            if not step_clean(export_dir, dry_run=args.dry_run):
                return 1
        
        # Step 1: Find Unity
        unity_exe = find_unity_exe()
        
        # Step 2: Export from Unity
        if not step_unity_export(
            unity_exe,
            export_dir=export_dir,
            batch_method=batch_method,
            simulator=args.simulator,
            dry_run=args.dry_run,
            development=args.dev,
        ):
            log("Build failed at Unity export step", level="ERROR")
            return 1
        
        # Step 3: Validate Xcode project
        if not step_xcode_build(xcode_project_dir=xcode_project_dir, dry_run=args.dry_run, development=args.dev):
            log("Build failed at Xcode validation step", level="ERROR")
            return 1

        # Step 4: Optional simulator run
        if args.run:
            if not args.simulator:
                log("--run requires --simulator because device launch needs signing/provisioning", level="ERROR")
                return 1
            if not step_run_simulator(xcode_project_dir=xcode_project_dir, dry_run=args.dry_run):
                log("Build failed at simulator run step", level="ERROR")
                return 1
        
        # Step 5: Open Xcode
        if not step_open_xcode(xcode_project_dir=xcode_project_dir, dry_run=args.dry_run, no_open=args.no_open):
            log("Warning: Could not open Xcode (it may already be running)", level="WARN")
        
        # Success
        banner("✓ BUILD COMPLETE")
        log(f"Xcode project ready at: {xcode_project_dir}")
        log(f"Next steps:")
        log(f"  1. Open {xcode_project_dir} in Xcode")
        if args.simulator:
            log(f"  2. Select a simulator or use --run with a booted simulator")
        else:
            log(f"  2. Select a signed iPhone target in Xcode")
        log(f"  3. Press ⌘B to build, ⌘R to run")
        log(f"  4. Or ⌘⇧K to archive for TestFlight/App Store")
        
        return 0
    
    except Exception as e:
        log(f"Fatal error: {e}", level="ERROR")
        import traceback
        traceback.print_exc()
        return 1


if __name__ == "__main__":
    sys.exit(main())
