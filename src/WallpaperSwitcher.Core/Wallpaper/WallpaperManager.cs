using Windows.Win32;
using Windows.Win32.UI.Shell;

namespace WallpaperSwitcher.Core.Wallpaper;

/// <summary>
/// Provides an abstract base for managing desktop wallpapers and wallpaper slideshows
/// using the Windows <c>IDesktopWallpaper</c> COM interface.
/// </summary>
/// <remarks>
/// This class offers an abstraction over native Windows wallpaper management, allowing
/// derived classes to implement specific wallpaper and slideshow behaviors.
/// </remarks>
public abstract class WallpaperManager
{
    /// <summary>
    /// Gets the instance of the <see cref="IDesktopWallpaper"/> interface used to control the desktop wallpaper.
    /// This interface is generated via the CsWin32 source generator and provides access to Windows wallpaper APIs.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the <see cref="IDesktopWallpaper"/> instance cannot be created.
    /// </exception>
    private protected IDesktopWallpaper DesktopWallpaper { get; } = new DesktopWallpaper()
        // ReSharper disable once SuspiciousTypeConversion.Global
        as IDesktopWallpaper ?? throw new InvalidOperationException("Failed to create IDesktopWallpaper instance.");

    protected virtual string SlideShowFolder { get; set; } = string.Empty;


    /// <summary>
    /// File extensions supported by Windows for wallpapers.
    /// Includes common image formats such as JPEG, PNG, BMP, and GIF.
    /// </summary>
    public static readonly string[] SupportedExtensions =
    [
        ".jpg", ".jpeg", ".png", ".bmp", "dib", ".gif", ".tif", ".tiff", "jfif"
    ];

    /// <summary>
    /// The default wallpaper of Windows.
    /// Used as a temporary wallpaper when the user deletes the wallpaper folder currently in use.
    /// </summary>
    public static readonly string DefaultWallpaper = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Web\Wallpaper\Windows\img0.jpg"
    );

    /// <summary>
    /// Sets a specific image as the desktop wallpaper.
    /// </summary>
    /// <param name="wallpaper">The full path to the image file to be set as wallpaper.</param>
    public virtual void SetWallpaper(string wallpaper)
    {
        if (!WallpaperHelper.IsValidWallpaper(wallpaper)) return;
        DesktopWallpaper.SetWallpaper(null, wallpaper);
    }

    // /// <summary>
    // /// Gets the full path to the folder currently being used for the wallpaper slideshow.
    // /// </summary>
    // /// <returns>The absolute path to the slideshow folder, or an empty string if none is set.</returns>
    // public string GetSlideShowFolder()
    // {
    //     DesktopWallpaper.GetSlideshow(out var shellItemArray);
    //     if (shellItemArray is null) return string.Empty;
    //     shellItemArray.GetCount(out var count);
    //     if (count == 0) return string.Empty; // No items in the slideshow or no slideshow set
    //     shellItemArray.GetItemAt(0, out var shellItem);
    //     if (shellItem is null) return string.Empty;
    //     shellItem.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out var pathPtr);
    //     // Convert the PWSTR to string
    //     var path = pathPtr.ToString();
    //     return path ?? string.Empty;
    // }

    /// <summary>
    /// Sets a wallpaper slideshow using images from the specified folder.
    /// </summary>
    /// <param name="folder">The full path to the folder containing images to use in the slideshow.</param>
    public abstract void SetSlideShow(string folder);

    /// <summary>
    /// Advances the current wallpaper slideshow to the next image.
    /// </summary>
    public abstract void AdvanceForwardSlideshow();
}