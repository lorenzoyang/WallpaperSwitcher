using Windows.Win32.Foundation;

namespace WallpaperSwitcher.Core;

public sealed class CustomWallpaperManager : WallpaperManager
{
    private string _slideShowFolder = string.Empty;
    private List<string> _slideShowWallpapers = [];
    private int _currentIndex;

    protected override string SlideShowFolder
    {
        get => _slideShowFolder;
        set
        {
            _slideShowFolder = value;
            _slideShowWallpapers = Directory.GetFiles(value)
                .Where(file => SupportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
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

    public override void SetSlideShow(string folder)
    {
        if (!WallpaperHelper.IsValidWallpaperFolder(folder, out _)) return;
        SlideShowFolder = folder;
        // If the folder is the same as the current "slideshow folder", do nothing
        // this happens only if the current wallpaper is contained in the slideshow folder.
        var currentWallpaper = GetCurrentWallpaper();
        var index = _slideShowWallpapers.IndexOf(currentWallpaper);
        // the current wallpaper is already in the slideshow folder,
        // so we don't need to set the slideshow again.
        if (index >= 0)
        {
            CurrentIndex = index;
            return;
        }

        CurrentIndex = 0;
        SetWallpaper(_slideShowWallpapers[CurrentIndex]);
    }

    public override void AdvanceForwardSlideshow()
    {
        if (string.IsNullOrEmpty(SlideShowFolder) || _slideShowWallpapers.Count == 1) return;
        CurrentIndex++;
        SetWallpaper(_slideShowWallpapers[CurrentIndex]);
    }

    private unsafe string GetCurrentWallpaper()
    {
        PWSTR pWallpaperPath = default;
        DesktopWallpaper.GetWallpaper(null, &pWallpaperPath);
        var result = pWallpaperPath.ToString();
        return result ?? string.Empty;
    }
}