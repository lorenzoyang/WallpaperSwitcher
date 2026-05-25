namespace WallpaperSwitcher.Core.Updates;

/// <summary>
/// Represents the result of comparing the current app version with the latest release version.
/// </summary>
/// <param name="CurrentVersion">The normalized current application version.</param>
/// <param name="LatestVersion">The normalized latest release version.</param>
/// <param name="LatestTagName">The release tag returned by the release provider.</param>
/// <param name="ReleaseUri">The public release page URI.</param>
public sealed record UpdateCheckResult(
    Version CurrentVersion,
    Version LatestVersion,
    string LatestTagName,
    Uri ReleaseUri)
{
    /// <summary>
    /// Gets a value indicating whether the latest release is newer than the current version.
    /// </summary>
    public bool IsUpdateAvailable => LatestVersion.CompareTo(CurrentVersion) > 0;
}
