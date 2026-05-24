using Windows.Win32;
using Windows.Win32.UI.Shell;

namespace WallpaperSwitcher.Core.Wallpaper;

/// <summary>
/// Provides helper methods for managing wallpapers and wallpaper folders,
/// including top-level folder validation and Windows Shell API integration.
/// </summary>
public static class WallpaperHelper
{
    private static readonly HashSet<string> SupportedExtensions = new(
        WallpaperManager.SupportedExtensions,
        StringComparer.OrdinalIgnoreCase
    );

    /// <summary>
    /// Gets the number of supported image files in the specified folder.
    /// </summary>
    /// <param name="folder">The full path to the folder to scan.</param>
    /// <returns>
    /// The number of top-level image files with supported extensions in the folder.
    /// Returns <c>0</c> if the folder is invalid or contains no valid images.
    /// </returns>
    public static int GetImageCount(string folder)
    {
        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
        {
            return 0;
        }

        return EnumerateWallpaperFiles(folder).Count();
    }

    /// <summary>
    /// Determines whether a folder is valid for use as a wallpaper slideshow source.
    /// </summary>
    /// <param name="folder">The full path to the folder to validate.</param>
    /// <param name="errorMessage">
    /// When the method returns <c>false</c>,
    /// contains a message describing the validation failure.
    /// </param>
    /// <returns>
    /// <c>true</c> if the folder exists and contains at least one supported top-level image file;
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool IsValidWallpaperFolder(string folder, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(folder))
        {
            errorMessage = "Please select a folder.";
            return false;
        }

        if (!Directory.Exists(folder))
        {
            errorMessage = "The selected folder does not exist.";
            return false;
        }

        // Directory enumeration can fail for inaccessible or transient folders.
        try
        {
            if (GetImageCount(folder) == 0)
            {
                errorMessage = "The selected folder does not contain any supported image files.";
                return false;
            }
        }
        catch (UnauthorizedAccessException)
        {
            errorMessage = "Access denied to the selected folder.";
            return false;
        }
        catch (Exception ex)
        {
            errorMessage = $"Error accessing folder: {ex.Message}";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified file is a valid wallpaper image.
    /// </summary>
    /// <param name="wallpaper">The full path to the image file to validate.</param>
    /// <returns>
    /// <c>true</c> if the file exists and has a supported image extension; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsValidWallpaper(string wallpaper)
    {
        return !string.IsNullOrEmpty(wallpaper) && File.Exists(wallpaper) &&
               IsValidWallpaperExtension(wallpaper);
    }

    /// <summary>
    /// Creates a Windows Shell item array for the specified folder.
    /// </summary>
    /// <param name="folder">The full path to the folder.</param>
    /// <returns>An <see cref="IShellItemArray"/> suitable for <c>IDesktopWallpaper.SetSlideshow</c>.</returns>
    /// <exception cref="Exception">
    /// Thrown if shell item or shell item array creation fails.
    /// </exception>
    internal static IShellItemArray CreateShellItemArrayFromFolder(string folder)
    {
        var hr = PInvoke.SHCreateItemFromParsingName(
            folder,
            null,
            typeof(IShellItem).GUID,
            out var shellItemObj
        );
        hr.ThrowOnFailure();
        var shellItem = (IShellItem)shellItemObj;

        hr = PInvoke.SHCreateShellItemArrayFromShellItem(
            shellItem,
            typeof(IShellItemArray).GUID,
            out var shellItemArrayObj
        );
        hr.ThrowOnFailure();

        return (IShellItemArray)shellItemArrayObj;
    }

    /// <summary>
    /// Determines whether a path has an extension supported by the wallpaper managers.
    /// </summary>
    /// <param name="wallpaper">The path whose extension should be checked.</param>
    /// <returns><c>true</c> when the extension is supported; otherwise, <c>false</c>.</returns>
    internal static bool IsValidWallpaperExtension(string wallpaper)
    {
        return SupportedExtensions.Contains(Path.GetExtension(wallpaper));
    }

    /// <summary>
    /// Enumerates supported wallpaper files directly inside a folder.
    /// </summary>
    /// <remarks>
    /// This method does not recurse into subfolders and expects callers to validate that the folder exists.
    /// </remarks>
    /// <param name="folder">The folder to enumerate.</param>
    /// <returns>A lazy sequence of matching wallpaper file paths.</returns>
    internal static IEnumerable<string> EnumerateWallpaperFiles(string folder)
    {
        return Directory
            .EnumerateFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
            .Where(IsValidWallpaperExtension);
    }
}
