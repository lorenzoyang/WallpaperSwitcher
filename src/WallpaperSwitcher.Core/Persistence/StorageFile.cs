namespace WallpaperSwitcher.Core.Persistence;

/// <summary>
/// Stages a configuration file alongside its destination until serialization succeeds.
/// </summary>
internal sealed class StorageFile : IDisposable
{
    private readonly string _destination;
    private bool _committed;

    public FileStream Stream { get; }

    public StorageFile(string location)
    {
        _destination = Path.GetFullPath(location);
        var directory = Path.GetDirectoryName(_destination)!;
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $"{Guid.NewGuid():N}.tmp");
        Stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
    }

    public void Commit()
    {
        Stream.Dispose();
        File.Move(Stream.Name, _destination, overwrite: true);
        _committed = true;
    }

    public void Dispose()
    {
        if (!_committed)
        {
            try
            {
                Stream.Dispose();
            }
            finally
            {
                File.Delete(Stream.Name);
            }
        }
    }

    /// <summary>
    /// Opens an existing file for reading without forcing callers to duplicate existence checks.
    /// </summary>
    /// <param name="location">The storage file path.</param>
    /// <returns>The opened stream, or <see langword="null"/> when the file does not exist.</returns>
    public static FileStream? OpenReadIfExists(string location)
    {
        return File.Exists(location) ? File.OpenRead(location) : null;
    }
}
