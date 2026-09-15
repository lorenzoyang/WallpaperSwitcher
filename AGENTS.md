# Repository Guidelines

## Project Structure & Module Organization

- `WallpaperSwitcher.slnx` contains the application and test projects.
- `src/WallpaperSwitcher.Core/` holds wallpaper switching, global hotkeys, JSON persistence, startup registration, and update checking.
- `src/WallpaperSwitcher.Desktop/` contains the WinForms entry point, forms, tray integration, and theme. Keep main-window behavior in the matching `MainForm.*.cs` partial file.
- `tests/WallpaperSwitcher.Core.Tests/` and `tests/WallpaperSwitcher.Desktop.Tests/` mirror the application layers.
- `assets/` stores screenshots and demo GIFs; form resources and the application icon live in Desktop. `.github/workflows/` defines CI and releases.

## Build, Test, and Development Commands

Use Windows with the .NET 10 SDK. Run these commands from the repository root:

```powershell
dotnet restore WallpaperSwitcher.slnx
dotnet format WallpaperSwitcher.slnx whitespace --verify-no-changes --no-restore
dotnet build WallpaperSwitcher.slnx --configuration Release --no-restore
dotnet test WallpaperSwitcher.slnx --configuration Release --no-build --verbosity normal
dotnet run --project src/WallpaperSwitcher.Desktop/WallpaperSwitcher.Desktop.csproj
```

These restore dependencies, verify whitespace, build, run NUnit tests, and launch the app, respectively. The first four match CI. Remove `--verify-no-changes` to apply whitespace fixes.

## Coding Style & Naming Conventions

Follow existing C# style: four-space indentation, braces on separate lines, file-scoped namespaces, and nullable reference types. Use PascalCase for types and public members, camelCase for parameters and locals, and `_camelCase` for private fields. Match filenames to their primary types.

Keep reusable behavior in Core and UI coordination in Desktop. Use `ModernTheme.cs` for shared styling. Avoid hand-editing generated settings code. Production package versions belong in `Directory.Packages.props`; test projects manage their own versions. Formatting uses `dotnet format`; tests also include NUnit analyzers.

## Testing Guidelines

Use NUnit `[Test]` and `[TestCase]`, with files named `<Type>Tests.cs` and methods such as `Load_WhenFileDoesNotExist_ReturnsDefaultSettings`. Cover changed behavior and failure paths. Isolate persistence tests in temporary directories and clean up afterward. Coverlet is available; no coverage threshold is enforced in CI. Manually verify affected WinForms, tray, and hotkey interactions on Windows.

## Commit & Pull Request Guidelines

History mixes concise imperative summaries with `feat:` and `fix:` prefixes; no strict format is enforced. Use a focused summary describing the change. PRs should explain behavior changes, link relevant issues, report validation, and include screenshots for UI changes. Update both English and Chinese READMEs when user-facing instructions change.
