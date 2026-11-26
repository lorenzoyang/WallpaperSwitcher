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
        ApplyFolder(folder, out _);
    }

    /// <inheritdoc/>
    public override bool TrySetSlideShow(string folder, out string errorMessage)
    {
        try
        {
            return ApplyFolder(folder, out errorMessage);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException or ArgumentException)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    private bool ApplyFolder(string folder, out string errorMessage)
    {
        if (string.IsNullOrEmpty(folder))
        {
            SlideShowFolder = string.Empty;
            errorMessage = string.Empty;
            return true;
        }

        if (!WallpaperHelper.IsValidWallpaperFolder(folder, out errorMessage)) return false;

        // Windows keeps its current slideshow position when the same folder is assigned again.
        DesktopWallpaper.SetSlideshow(WallpaperHelper.CreateShellItemArrayFromFolder(folder));
        SlideShowFolder = folder;
        return true;
    }

    /// <inheritdoc/>
    public override void AdvanceForwardSlideshow()
    {
        if (string.IsNullOrEmpty(SlideShowFolder)) return;
        DesktopWallpaper.AdvanceSlideshow(null, DESKTOP_SLIDESHOW_DIRECTION.DSD_FORWARD);
    }
}
