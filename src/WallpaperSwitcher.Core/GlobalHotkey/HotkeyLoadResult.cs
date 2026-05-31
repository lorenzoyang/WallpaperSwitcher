namespace WallpaperSwitcher.Core.GlobalHotkey;

/// <summary>
/// Reports non-fatal failures that occurred while loading persisted hotkeys.
/// </summary>
/// <param name="Failures">The hotkeys that were skipped during startup registration.</param>
public sealed record HotkeyLoadResult(IReadOnlyList<HotkeyLoadFailure> Failures)
{
    /// <summary>
    /// Gets an empty successful load result.
    /// </summary>
    public static HotkeyLoadResult Success { get; } = new(Array.Empty<HotkeyLoadFailure>());

    /// <summary>
    /// Gets a value indicating whether any persisted hotkeys were skipped.
    /// </summary>
    public bool HasFailures => Failures.Count > 0;
}
