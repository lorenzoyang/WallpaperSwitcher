using Windows.Win32.Foundation;

namespace WallpaperSwitcher.Core.Wallpaper;

/// <summary>
/// Provides a custom slideshow implementation by cycling through wallpaper files directly.
/// </summary>
public sealed class CustomWallpaperManager : WallpaperManager
{
    private string _slideShowFolder = string.Empty;
    private List<string> _slideShowWallpapers = [];
    private int _currentIndex;

    /// <summary>
    /// Gets or sets the folder containing top-level images used in the custom slideshow.
    /// </summary>
    /// <remarks>
    /// Setting this property rebuilds the ordered wallpaper list and resets the slideshow index.
    /// </remarks>
    protected override string SlideShowFolder
    {
        get => _slideShowFolder;
        set
        {
            _slideShowFolder = value;
            _slideShowWallpapers = WallpaperHelper.EnumerateWallpaperFiles(value)
                .OrderBy(Path.GetFileName)
                .ToList();
            CurrentIndex = 0;
        }
    }

    private int CurrentIndex
    {
        get => _currentIndex;
        set => _currentIndex = (value >= _slideShowWallpapers.Count || value < 0) ? 0 : value;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// If the specified folder is already the active slideshow folder and the
    /// current wallpaper is one of its images, the slideshow is not restarted
    /// and the current index is preserved.
    /// </remarks>
    public override void SetSlideShow(string folder)
    {
        if (!WallpaperHelper.IsValidWallpaperFolder(folder, out _))
        {
            ClearSlideShow();
            return;
        }

        SlideShowFolder = folder;
        var currentWallpaper = GetCurrentWallpaper();
        var index = _slideShowWallpapers.IndexOf(currentWallpaper);

        // Preserve the user's current wallpaper when it is already part of the selected folder.
        if (index >= 0)
        {
            CurrentIndex = index;
            return;
        }

        CurrentIndex = 0;
        SetWallpaper(_slideShowWallpapers[CurrentIndex]);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// If the slideshow folder is empty or contains only one image, no action is taken.
    /// </remarks>
    public override void AdvanceForwardSlideshow()
    {
        if (string.IsNullOrEmpty(SlideShowFolder) || _slideShowWallpapers.Count <= 1) return;
        CurrentIndex++;
        SetWallpaper(_slideShowWallpapers[CurrentIndex]);
    }

    private void ClearSlideShow()
    {
        _slideShowFolder = string.Empty;
        _slideShowWallpapers = [];
        _currentIndex = 0;
    }

    /// <summary>
    /// Retrieves the full path of the current desktop wallpaper.
    /// </summary>
    /// <returns>The absolute path to the current wallpaper image, or an empty string if unavailable.</returns>
    private unsafe string GetCurrentWallpaper()
    {
        PWSTR pWallpaperPath = default;
        DesktopWallpaper.GetWallpaper(null, &pWallpaperPath);
        var result = pWallpaperPath.ToString();
        return result ?? string.Empty;
    }
}
