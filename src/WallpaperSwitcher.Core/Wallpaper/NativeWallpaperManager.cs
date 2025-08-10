using Windows.Win32.UI.Shell;

namespace WallpaperSwitcher.Core.Wallpaper;

/// <summary>
/// Manages wallpapers and slideshows using Windows' built-in slideshow feature.
/// </summary>
/// <remarks>
/// This class uses the native Windows <c>IDesktopWallpaper</c> COM interface
/// to set wallpapers and control slideshow progression.
/// </remarks>
public sealed class NativeWallpaperManager : WallpaperManager
{
    /// <inheritdoc/>
    public override void SetSlideShow(string folder)
    {
        if (!WallpaperHelper.IsValidWallpaperFolder(folder, out _)) return;
        SlideShowFolder = folder;
        // Windows Slide Show default behavior: if the folder is the same as the current slideshow folder,
        // do nothing, will not start from the first image.
        DesktopWallpaper.SetSlideshow(WallpaperHelper.CreateShellItemArrayFromFolder(SlideShowFolder));
    }

    /// <inheritdoc/>
    public override void AdvanceForwardSlideshow()
    {
        if (string.IsNullOrEmpty(SlideShowFolder)) return;
        DesktopWallpaper.AdvanceSlideshow(null, DESKTOP_SLIDESHOW_DIRECTION.DSD_FORWARD);
    }
}