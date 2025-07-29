using Windows.Win32;
using Windows.Win32.UI.Shell;


namespace WallpaperSwitcher.Core;

public sealed class DesktopWallpaperManager
{
    public static readonly string[] SupportedExtensions =
    [
        ".jpg", ".jpeg", ".png", ".bmp", "dib", ".gif", ".tif", ".tiff", "jfif"
    ];

    public static readonly string DefaultWallpaperPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Web\Wallpaper\Windows\img0.jpg"
    );

    /// <summary>
    /// A wrapper class for the IDesktopWallpaper interface,
    /// generated via the 'CsWin32' source code generation tool.
    /// </summary>
    private readonly IDesktopWallpaper _desktopWallpaper =
        // ReSharper disable once SuspiciousTypeConversion.Global
        new DesktopWallpaper() as IDesktopWallpaper ?? throw new InvalidOperationException();

    private string SlideShowFolder { get; set; } = string.Empty;

    public void SetSlideShow(string folder, bool restart = true)
    {
        if (!DesktopWallpaperHelper.IsValidWallpaperFolderPath(folder, out _)) return;
        SlideShowFolder = folder;
        if (!restart) return;
        _desktopWallpaper.SetSlideshow(DesktopWallpaperHelper.CreateShellItemArrayFromFolder(SlideShowFolder));
    }

    public void AdvanceForwardSlideshow()
    {
        if (string.IsNullOrEmpty(SlideShowFolder)) return;
        _desktopWallpaper.AdvanceSlideshow(null, DESKTOP_SLIDESHOW_DIRECTION.DSD_FORWARD);
    }

    public void SetWallpaper(string wallpaperPath)
    {
        if (!DesktopWallpaperHelper.IsValidWallpaperPath(wallpaperPath)) return;
        _desktopWallpaper.SetWallpaper(null, wallpaperPath);
    }

    public string GetSlideShowFolderPath()
    {
        _desktopWallpaper.GetSlideshow(out var shellItemArray);
        if (shellItemArray is null) return string.Empty;
        shellItemArray.GetCount(out var count);
        if (count == 0) return string.Empty; // No items in the slideshow or no slideshow set
        shellItemArray.GetItemAt(0, out var shellItem);
        if (shellItem is null) return string.Empty;
        shellItem.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out var pathPtr);
        // Convert the PWSTR to string
        var path = pathPtr.ToString();
        return path ?? string.Empty;
    }
}