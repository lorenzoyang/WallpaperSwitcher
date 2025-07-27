namespace WallpaperSwitcher.Core;

public static class WallpaperHelper
{
    public static int GetImageCount(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
        {
            return 0;
        }

        return Directory
            .GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
            .Count(file =>
                DesktopWallpaperManager.SupportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()));
    }

    public static bool ValidateWallpaperFolder(string folderPath, out string errorMessage)
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
}