namespace WallpaperSwitcher.Core.Wallpaper;

/// <summary>
/// Refreshes and advances a folder's ordered wallpaper list without depending on Windows APIs.
/// </summary>
internal sealed class CustomSlideshow
{
    private readonly Func<string, IEnumerable<string>> _enumerateWallpapers;
    private readonly Func<string> _getCurrentWallpaper;
    private readonly Func<string, bool> _trySetWallpaper;

    private string _folder = string.Empty;
    private List<string> _wallpapers = [];
    private string _currentWallpaper = string.Empty;

    public CustomSlideshow(
        Func<string, IEnumerable<string>> enumerateWallpapers,
        Func<string> getCurrentWallpaper,
        Func<string, bool> trySetWallpaper)
    {
        _enumerateWallpapers = enumerateWallpapers;
        _getCurrentWallpaper = getCurrentWallpaper;
        _trySetWallpaper = trySetWallpaper;
    }

    /// <summary>
    /// Selects a folder, preserving the desktop wallpaper if it belongs to that folder.
    /// Empty or temporarily unavailable folders remain selected so Next can retry them.
    /// </summary>
    public void SetSlideShow(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            _folder = string.Empty;
            _wallpapers = [];
            _currentWallpaper = string.Empty;
            return;
        }

        _folder = folder;
        _currentWallpaper = _getCurrentWallpaper();

        if (!TryRefreshWallpapers() || FindCurrentWallpaperIndex() >= 0)
        {
            return;
        }

        TryAdvanceFrom(0);
    }

    /// <summary>
    /// Rescans the selected folder and tries each following image at most once.
    /// </summary>
    public void AdvanceForwardSlideshow()
    {
        if (string.IsNullOrEmpty(_folder) || !TryRefreshWallpapers())
        {
            return;
        }

        var currentIndex = FindCurrentWallpaperIndex();
        var nextIndex = currentIndex < 0 ? 0 : (currentIndex + 1) % _wallpapers.Count;
        TryAdvanceFrom(nextIndex);
    }

    private bool TryRefreshWallpapers()
    {
        try
        {
            // Publish the snapshot only after enumeration and sorting both complete.
            _wallpapers = _enumerateWallpapers(_folder)
                .OrderBy(Path.GetFileName)
                .ToList();
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private int FindCurrentWallpaperIndex()
    {
        return _wallpapers.FindIndex(path =>
            StringComparer.OrdinalIgnoreCase.Equals(path, _currentWallpaper));
    }

    private void TryAdvanceFrom(int startIndex)
    {
        for (var offset = 0; offset < _wallpapers.Count; offset++)
        {
            var wallpaper = _wallpapers[(startIndex + offset) % _wallpapers.Count];
            if (StringComparer.OrdinalIgnoreCase.Equals(wallpaper, _currentWallpaper))
            {
                continue;
            }

            // A failed image is skipped for this request only, allowing future retries.
            if (_trySetWallpaper(wallpaper))
            {
                _currentWallpaper = wallpaper;
                return;
            }
        }
    }
}
