namespace WallpaperSwitcher.Core.Updates;

/// <summary>
/// Checks whether a newer Wallpaper Switcher release is available.
/// </summary>
public interface IUpdateChecker
{
    /// <summary>
    /// Compares the current application version against the latest available release.
    /// </summary>
    /// <param name="currentVersion">The version reported by the running application.</param>
    /// <param name="cancellationToken">A token used to cancel the update check.</param>
    /// <returns>The result of the update check.</returns>
    Task<UpdateCheckResult> CheckForUpdatesAsync(
        Version currentVersion,
        CancellationToken cancellationToken = default);
}
