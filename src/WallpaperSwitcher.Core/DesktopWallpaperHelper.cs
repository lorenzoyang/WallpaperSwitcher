using Windows.Win32;
using Windows.Win32.UI.Shell;

namespace WallpaperSwitcher.Core;

public static class DesktopWallpaperHelper
{
    public static int GetImageCount(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
        {
            return 0;
        }

        return Directory
            .GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
            .Count(IsValidWallpaperFileExtension);
    }

    public static bool IsValidWallpaperFolderPath(string folderPath, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            errorMessage = "Please select a folder.";
            return false;
        }

        if (!Directory.Exists(folderPath))
        {
            errorMessage = "The selected folder does not exist.";
            return false;
        }

        // Check if folder contains any image files
        try
        {
            if (GetImageCount(folderPath) == 0)
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

    public static bool IsValidWallpaperPath(string wallpaperPath)
    {
        return !string.IsNullOrEmpty(wallpaperPath) && File.Exists(wallpaperPath) &&
               IsValidWallpaperFileExtension(wallpaperPath);
    }

    internal static IShellItemArray CreateShellItemArrayFromFolder(string folder)
    {
        // Create shell item from folder path
        var hr = PInvoke.SHCreateItemFromParsingName(
            folder,
            null,
            typeof(IShellItem).GUID,
            out var shellItemObj);
        hr.ThrowOnFailure();
        var shellItem = (IShellItem)shellItemObj;

        // Create shell item array from shell item
        hr = PInvoke.SHCreateShellItemArrayFromShellItem(
            shellItem,
            typeof(IShellItemArray).GUID,
            out var shellItemArrayObj);
        hr.ThrowOnFailure();

        return (IShellItemArray)shellItemArrayObj;
    }

    private static bool IsValidWallpaperFileExtension(string wallpaperPath)
    {
        return DesktopWallpaperManager.SupportedExtensions.Contains(Path.GetExtension(wallpaperPath)
            .ToLowerInvariant());
    }
}