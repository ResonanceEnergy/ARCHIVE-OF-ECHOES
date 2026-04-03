#!/bin/bash
# build.sh — Quick wrapper for iOS build automation
#
# Usage:
#   ./build.sh              # Full build + open Xcode
#   ./build.sh --no-open    # Build but don't open
#   ./build.sh --clean      # Clean build
#   ./build.sh --dev        # Development build
#   ./build.sh --dry-run    # Show what would happen

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

python3 build_ios.py "$@"
