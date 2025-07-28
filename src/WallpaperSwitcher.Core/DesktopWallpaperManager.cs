using System.Runtime.Versioning;
using Windows.Win32.UI.Shell;


namespace WallpaperSwitcher.Core;

public sealed class DesktopWallpaperManager
{
    public static readonly string[] SupportedExtensions =
    [
        ".jpg", ".jpeg", ".png", ".bmp", "dib", ".gif", ".tif", ".tiff", "jfif"
    ];

    /// <summary>
    /// A wrapper class for the IDesktopWallpaper interface,
    /// generated via the 'CsWin32' source code generation tool.
    /// </summary>
    private readonly IDesktopWallpaper _desktopWallpaper = new DesktopWallpaper() as IDesktopWallpaper;

    [SupportedOSPlatform("windows8.0")]
    public void SetSlideShow(string folder)
    {
        _desktopWallpaper.SetSlideshow(DesktopWallpaperHelper.CreateShellItemArrayFromFolder(folder));
    }

    [SupportedOSPlatform("windows8.0")]
    public void AdvanceForwardSlideshow()
    {
        _desktopWallpaper.AdvanceSlideshow(null, DESKTOP_SLIDESHOW_DIRECTION.DSD_FORWARD);
    }

    [SupportedOSPlatform("windows8.0")]
    public void AdvanceBackwardSlideshow()
    {
        _desktopWallpaper.AdvanceSlideshow(null, DESKTOP_SLIDESHOW_DIRECTION.DSD_BACKWARD);
    }
}