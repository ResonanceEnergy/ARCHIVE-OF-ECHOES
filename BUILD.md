# iOS Build Guide — Archive of Echoes

This document explains how to build Archive of Echoes for iPhone using the automated build pipeline.

## Quick Start

### Prerequisites

1. **macOS 12+** with Xcode command-line tools
   ```bash
   xcode-select --install
   ```

2. **Unity 2022.3+** installed (via [Unity Hub](https://unity.com/download))
   - Hub will auto-detect the latest version
   - Supported locations:
  - `/Applications/Unity/Hub/Editor/` (Hub default)
  - `/Applications/Unity/Hub/Editors/` (alternate legacy path)
     - `/Applications/Unity/` (direct install)
     - `~/Development/Unity/` (custom path)

3. **Python 3.8+** (pre-installed on macOS)

### One-Command Build

```bash
cd /Users/natrix/Documents/GitHub/ARCHIVE-OF-ECHOES

# Export from Unity and open Xcode automatically
./build.sh

# Or using Python directly
python3 build_ios.py

# Or using make
make build-ios

# Export simulator-compatible project
make build-ios-sim

# Export and run on a booted simulator
make run-ios-sim
```

The script will:
1. ✅ Find your Unity installation
2. ✅ Export the game to iOS
3. ✅ Generate an Xcode project
4. ✅ Open Xcode automatically

## Build Options

### Clean Rebuild
```bash
./build.sh --clean
```
Removes old build artifacts and rebuilds from scratch.

### Development Build
```bash
./build.sh --dev
```
Includes debug symbols, suitable for debugging on-device.

### Simulator Export
```bash
python3 build_ios.py --simulator --no-open
```
Exports a simulator-compatible Xcode project to `Builds/iOSSim/Unity-iPhone.xcodeproj`.

### Build + Run on Booted Simulator
```bash
python3 build_ios.py --simulator --run --no-open
```
If no iPhone simulator is booted, the script will boot the first available one automatically.

### Production Build
```bash
./build.sh --prod
```
Optimized build for App Store (default).

### Don't Open Xcode
```bash
./build.sh --no-open
```
Exports but leaves Xcode to you. Useful in CI/CD.

### Dry Run (Preview)
```bash
./build.sh --dry-run
```
Shows all commands without running them.

## After Export: Building in Xcode

Once the script completes, Xcode opens automatically. From there:

### Debug Build (Run on Device/Simulator)
1. Connect iPhone or select simulator
2. Press **⌘B** to build
3. Press **⌘R** to build + run
4. Wait for app to install and launch

### Archive for TestFlight/App Store
1. Select generic iOS Device (not simulator)
2. Press **⌘⇧K** to archive
3. In Organizer window, click "Distribute App"
4. Choose TestFlight or App Store Connect

## Troubleshooting

### "Could not find Unity installation"

Check if Unity is installed:
```bash
ls /Applications/Unity/Hub/Editor/
```

If nothing appears, install via Unity Hub:
1. Download [Unity Hub](https://unity.com/download)
2. Open Unity Hub → Downloads → Install version 2022.3+

If Unity is in a custom location, edit `build_ios.py`:
```python
UNITY_PATHS = [
    Path("/path/to/your/unity"),
    # ... other paths
]
```

### "Xcode project not found"

The Unity export may have failed. Check the detailed log:
```bash
ls build_ios_*.log
tail -f build_ios_20260402_232618.log
```

Common causes:
- Scenes not listed in `ProjectSettings/EditorBuildSettings.asset`
- Missing scripting define symbols
- Corrupted project

### Permission Denied

Make scripts executable:
```bash
chmod +x build.sh build_ios.py
```

## Make Commands (Optional)

If you prefer `make`:

```bash
make build-ios             # Export + open Xcode
make build-ios-no-open     # Export only
make build-ios-clean       # Clean build
make build-ios-dev         # Development build
make build-ios-prod        # Production build
make build-ios-dry         # Dry-run preview
make help-build            # Show all targets
```

## Build Pipeline Architecture

```
┌─────────────────────────────────────────────────────────────┐
│ build.sh (shell wrapper)                                    │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────────┐
│ build_ios.py (Python orchestrator)                          │
├─────────────────────────────────────────────────────────────┤
│ 1. Find Unity executable                                    │
│ 2. Launch Unity in batch mode                               │
│ 3. Export iOS project                                       │
│ 4. Validate Xcode project                                   │
│ 5. Open Xcode                                               │
│ 6. Log all output to build_ios_TIMESTAMP.log                │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────────┐
│ Xcode Project (Builds/iOS/Unity-iPhone.xcodeproj)           │
├─────────────────────────────────────────────────────────────┤
│ Manual steps:                                               │
│ - Select device/simulator                                   │
│ - ⌘B to build    ⌘R to run    ⌘⇧K to archive               │
└─────────────────────────────────────────────────────────────┘
```

## Project Configuration

Build settings are defined in:
- **Scenes**: `ProjectSettings/EditorBuildSettings.asset` (4 scenes)
- **Player**: `ProjectSettings/PlayerSettings.asset`
  - Product Name: "ARCHIVE OF ECHOES"
  - Company: "DefaultCompany"
  - Portrait orientation

Unity Batch Methods:
- `ArchiveOfEchoes.Editor.ArchiveBootstrapper.BatchBuildIos`
- `ArchiveOfEchoes.Editor.ArchiveBootstrapper.BatchBuildIosSimulator`

## Build Output

Device export: **`Builds/iOS/Unity-iPhone.xcodeproj`**

Simulator export: **`Builds/iOSSim/Unity-iPhone.xcodeproj`**

Total size: ~200–400 MB (uncompressed)

## Performance Notes

- First build: 5–10 minutes (initial compilation)
- Incremental builds: 2–5 minutes (depending on changes)
- Archive for store: 2–3 minutes
- Upload to TestFlight: 5–10 minutes

## Next Steps After App Delivery

### TestFlight Beta Distribution
1. Archive build (`⌘⇧K`)
2. Distribute → TestFlight
3. Add beta testers
4. Share link for testing

### App Store Release
1. Archive build (`⌘⇧K`)
2. Distribute → App Store Connect
3. Complete metadata (screenshots, description, etc.)
4. Submit for review

## Debugging

For detailed logs, check the timestamped log file:
```bash
tail -50 build_ios_*.log
```

To rebuild with verbose output:
```bash
python3 build_ios.py --clean --no-open
# Then check Builds/iOS/ for any errors
```

## Extending the Build

To modify build options, edit `build_ios.py`:

```python
# Add custom build flags
def step_unity_export(...):
    cmd = [
        str(unity_exe),
        "-batchmode",
        "-quit",
        # Add your custom flags here
    ]
```

Common Unity batch flags:
- `-playerPackageConfig` — Custom input/graphics settings
- `-scriptingDefineSymbols` — Preprocessor directives
- `-buildNumber` — Override build ID
- `-scriptingBackend` — IL2CPP or Mono

---

**Questions?** Check `ROADMAP.md` for project status or examine the build scripts directly.
