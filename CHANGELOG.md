# Changelog

All notable changes to this project will be documented in this file.

## Unreleased

### Added
- Added JSON-backed application settings persistence for wallpaper folders, selected mode, tray hint state, and startup preference.
- Added documentation for Core persistence, startup, hotkey, and wallpaper components.
- Added focused Core tests for hotkey lifecycle behavior, parser validation, settings normalization, and wallpaper helper edge cases.

### Changed
- Improved global hotkey registration, removal, rebinding, loading, and saving reliability.
- Hardened hotkey input validation to reject ambiguous or unsafe shortcuts.
- Preserved explicit hotkey IDs during load and rebind operations.
- Normalized partially invalid settings loaded from JSON.
- Improved wallpaper extension validation and custom slideshow state handling.
- Removed stale commented-out code from Core and Desktop files.

### Fixed
- Fixed successfully unregistered hotkeys remaining in memory.
- Fixed string-based hotkey registration ignoring explicit IDs.
- Fixed possible hotkey ID reuse after loading saved hotkeys.
- Fixed hotkey rebinding behavior that could drop the old shortcut when the new shortcut failed.
- Fixed wallpaper extension matching for `.dib`, `.jfif`, and uppercase supported extensions.

### Validation
- `dotnet test --no-restore`
- `dotnet build --no-restore`
- `dotnet format --verify-no-changes --no-restore`
