using Windows.Win32.UI.Shell;


namespace WallpaperSwitcher.Core;

public sealed class NativeWallpaperManager : WallpaperManager
{
    public override void SetSlideShow(string folder)
    {
        if (!WallpaperHelper.IsValidWallpaperFolder(folder, out _)) return;
        SlideShowFolder = folder;
        // Windows Slide Show default behavior: if the folder is the same as the current slideshow folder,
        // do nothing, will not start from the first image.
        DesktopWallpaper.SetSlideshow(WallpaperHelper.CreateShellItemArrayFromFolder(SlideShowFolder));
    }

    public override void AdvanceForwardSlideshow()
    {
        if (string.IsNullOrEmpty(SlideShowFolder)) return;
        DesktopWallpaper.AdvanceSlideshow(null, DESKTOP_SLIDESHOW_DIRECTION.DSD_FORWARD);
    }
}