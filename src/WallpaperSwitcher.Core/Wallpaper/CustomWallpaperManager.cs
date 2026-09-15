using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace WallpaperSwitcher.Core.Wallpaper;

/// <summary>
/// Provides a custom slideshow implementation by cycling through wallpaper files directly.
/// </summary>
public sealed class CustomWallpaperManager : WallpaperManager
{
    private readonly CustomSlideshow _slideshow;

    /// <summary>
    /// Initializes the custom slideshow with the Windows wallpaper operations.
    /// </summary>
    public CustomWallpaperManager()
    {
        _slideshow = new CustomSlideshow(
            WallpaperHelper.EnumerateWallpaperFiles,
            GetCurrentWallpaper,
            TrySetWallpaper);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Preserves the current wallpaper when it belongs to the selected folder.
    /// Empty or temporarily inaccessible folders are retried on the next advance.
    /// </remarks>
    public override void SetSlideShow(string folder)
    {
        _slideshow.SetSlideShow(folder);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Refreshes the current folder before advancing, including newly added images and
    /// skipping deleted or unavailable images without resetting the current position.
    /// </remarks>
    public override void AdvanceForwardSlideshow()
    {
        _slideshow.AdvanceForwardSlideshow();
    }

    private bool TrySetWallpaper(string wallpaper)
    {
        try
        {
            if (!WallpaperHelper.IsValidWallpaper(wallpaper))
            {
                return false;
            }

            // The generated COM signature throws on a failed HRESULT.
            DesktopWallpaper.SetWallpaper(null, wallpaper);
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or COMException)
        {
            return false;
        }
        catch (ArgumentException exception) when (exception.HResult == unchecked((int)0x80070057))
        {
            // Windows reports invalid image contents as E_INVALIDARG, mapped to ArgumentException.
            return false;
        }
    }

    /// <summary>
    /// Retrieves the full path of the current desktop wallpaper.
    /// </summary>
    /// <returns>The absolute path to the current wallpaper image, or an empty string if unavailable.</returns>
    private unsafe string GetCurrentWallpaper()
    {
        PWSTR pWallpaperPath = default;
        try
        {
            DesktopWallpaper.GetWallpaper(null, &pWallpaperPath);
            return pWallpaperPath.ToString() ?? string.Empty;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or COMException)
        {
            return string.Empty;
        }
        finally
        {
            Marshal.FreeCoTaskMem((nint)pWallpaperPath.Value);
        }
    }
}
