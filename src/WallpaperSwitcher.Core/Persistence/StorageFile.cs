namespace WallpaperSwitcher.Core.Persistence;

/// <summary>
/// Provides small file-system helpers shared by JSON-backed storage implementations.
/// </summary>
internal static class StorageFile
{
    /// <summary>
    /// Creates the parent directory for a storage file when the location includes one.
    /// </summary>
    /// <param name="location">The storage file path.</param>
    public static void EnsureParentDirectoryExists(string location)
    {
        var directory = Path.GetDirectoryName(location);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    /// <summary>
    /// Opens an existing file for reading without forcing callers to duplicate existence checks.
    /// </summary>
    /// <param name="location">The storage file path.</param>
    /// <param name="fileStream">The opened stream, or <see langword="null"/> when the file does not exist.</param>
    /// <returns><see langword="true"/> when the file exists and was opened; otherwise, <see langword="false"/>.</returns>
    public static bool TryOpenRead(string location, out FileStream fileStream)
    {
        if (File.Exists(location))
        {
            fileStream = File.OpenRead(location);
            return true;
        }

        fileStream = null!;
        return false;
    }
}
