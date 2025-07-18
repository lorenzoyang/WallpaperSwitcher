using System.ComponentModel;
using System.Runtime.InteropServices;

namespace WallpaperSwitcher.Core;

public class WallpaperManager
{
    private readonly List<string> _supportedExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp"];


    public static void Set(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Wallpaper path cannot be null or empty.", nameof(path));
        }

        // Attempt to set the wallpaper using the Windows API
        int result = Interop.WallpaperNativeApi.SetWallpaper(path);

        // Check for errors
        if (result == 0)
        {
            int errorCode = Marshal.GetLastWin32Error();
            throw new Win32Exception(errorCode, "Failed to set wallpaper.");
        }
    }
}

// TODO: using windows registry to set wallpaper style,
// SystemParametersInfo does not support setting wallpaper style directly
public enum WallpaperStyle
{
    Fill,
    Fit,
    Stretch,
    Tile,
    Center,
    Span
}