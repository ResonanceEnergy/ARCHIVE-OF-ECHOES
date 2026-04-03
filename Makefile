.PHONY: build-ios build-ios-clean build-ios-dev build-ios-prod build-ios-no-open build-ios-sim run-ios-sim

## Build iOS and open Xcode
build-ios:
	@python3 build_ios.py

## Build iOS (prod), don't open Xcode
build-ios-no-open:
	@python3 build_ios.py --no-open

## Export simulator-compatible iOS build
build-ios-sim:
	@python3 build_ios.py --simulator --no-open

## Export, build, install, and launch on booted simulator
run-ios-sim:
	@python3 build_ios.py --simulator --run --no-open

## Clean build (remove old artifacts and rebuild)
build-ios-clean:
	@python3 build_ios.py --clean

## Development build (debug symbols)
build-ios-dev:
	@python3 build_ios.py --dev

## Production build (optimized)
build-ios-prod:
	@python3 build_ios.py --prod

## Show what would happen (dry-run)
build-ios-dry:
	@python3 build_ios.py --dry-run

## Show build targets
help-build:
	@echo "iOS Build Targets:"
	@echo "  make build-ios             - Export to Xcode and open (default)"
	@echo "  make build-ios-no-open     - Export but don't open Xcode"
	@echo "  make build-ios-clean       - Clean build"
	@echo "  make build-ios-dev         - Development build (debug)"
	@echo "  make build-ios-prod        - Production build (optimized)"
	@echo "  make build-ios-sim         - Export simulator-compatible Xcode project"
	@echo "  make run-ios-sim           - Build/install/launch on booted simulator"
	@echo "  make build-ios-dry         - Show what would run"
