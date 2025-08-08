using Windows.Win32;
using Windows.Win32.UI.Shell;

namespace WallpaperSwitcher.Core;

/// <summary>
/// Provides helper methods for managing wallpapers and wallpaper folders,
/// including validation and integration with Windows shell APIs.
/// </summary>
public static class WallpaperHelper
{
    /// <summary>
    /// Gets the number of supported image files in the specified folder.
    /// </summary>
    /// <param name="folder">The full path to the folder to scan for images.</param>
    /// <returns>
    /// The number of image files with supported extensions in the folder.
    /// Returns <c>0</c> if the folder is invalid or contains no valid images.
    /// </returns>
    public static int GetImageCount(string folder)
    {
        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
        {
            return 0;
        }

        return Directory
            .GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
            .Count(IsValidWallpaperExtension);
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
    /// <c>true</c> if the folder exists and contains at least one supported image file; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
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

        // Check if folder contains any image files
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
    /// Creates an <see cref="IShellItemArray"/> from the contents of the specified folder.
    /// Useful for interacting with Windows Shell APIs such as slideshow management.
    /// </summary>
    /// <param name="folder">The full path to the folder.</param>
    /// <returns>An <see cref="IShellItemArray"/> representing the folder's contents.</returns>
    /// <exception cref="Exception">
    /// Thrown if shell item or shell item array creation fails.
    /// </exception>
    internal static IShellItemArray CreateShellItemArrayFromFolder(string folder)
    {
        // Create shell item from folder path
        var hr = PInvoke.SHCreateItemFromParsingName(
            folder,
            null,
            typeof(IShellItem).GUID,
            out var shellItemObj
        );
        hr.ThrowOnFailure();
        var shellItem = (IShellItem)shellItemObj;

        // Create shell item array from shell item
        hr = PInvoke.SHCreateShellItemArrayFromShellItem(
            shellItem,
            typeof(IShellItemArray).GUID,
            out var shellItemArrayObj
        );
        hr.ThrowOnFailure();

        return (IShellItemArray)shellItemArrayObj;
    }

    /// <summary>
    /// Checks whether the specified file has a supported wallpaper file extension.
    /// </summary>
    /// <param name="wallpaper">The full path or filename of the image file.</param>
    /// <returns>
    /// <c>true</c> if the file has a supported extension (e.g., <c>.jpg</c>, <c>.png</c>); otherwise, <c>false</c>.
    /// </returns>
    private static bool IsValidWallpaperExtension(string wallpaper)
    {
        return WallpaperManager.SupportedExtensions.Contains(Path.GetExtension(wallpaper)
            .ToLowerInvariant());
    }
}