using System.ComponentModel;
using System.Runtime.InteropServices;

// ReSharper disable CommentTypo

// ReSharper disable IdentifierTypo

// ReSharper disable InconsistentNaming

namespace WallpaperSwitcher.Core.Interop;

internal static partial class WallpaperNativeApi
{
    // Windows API constants
    private const int SPI_SETDESKWALLPAPER = 20; // Sets the desktop wallpaper
    private const int SPIF_UPDATEINIFILE = 0x01; // Saves the change to registry (legacy: win.ini)
    private const int SPIF_SENDCHANGE = 0x02; // Broadcasts a system message to notify all applications

    /// <summary>
    /// Wraps the Windows API <c>SystemParametersInfo</c> function to get or set system-wide parameters.
    /// Commonly used to set the desktop wallpaper or retrieve system metrics.
    /// </summary>
    /// <param name="uAction">
    /// The system parameter to query or set. Use SPI_* constants like
    /// SPI_SETDESKWALLPAPER (0x0014) or SPI_GETDESKWALLPAPER (0x0073).
    /// </param>
    /// <param name="uParam">
    /// Additional parameter whose meaning depends on <paramref name="uAction"/>.
    /// For SPI_SETDESKWALLPAPER, this must be 0. For retrieval operations, this often specifies buffer size.
    /// </param>
    /// <param name="lpvParam">
    /// When setting parameters: The new value as a string (e.g., wallpaper path).
    /// When retrieving data: Caller-allocated buffer to receive the result.
    /// </param>
    /// <param name="fuWinIni">
    /// Flags specifying change behavior. Combine:
    /// SPIF_UPDATEINIFILE (0x01) to save change, and SPIF_SENDCHANGE (0x02) to broadcast settings change.
    /// </param>
    /// <returns>
    /// Returns non-zero if successful; zero if the call fails.
    /// </returns>
    [LibraryImport(
        "user32.dll",
        EntryPoint = "SystemParametersInfoW",
        StringMarshalling = StringMarshalling.Utf16,
        SetLastError = true
    )]
    private static partial int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    public static int SetWallpaper(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Wallpaper path cannot be null or empty.", nameof(path));
        }

        // Call the Windows API to set the wallpaper
        int result = SystemParametersInfo(
            SPI_SETDESKWALLPAPER,
            0,
            path,
            SPIF_UPDATEINIFILE | SPIF_SENDCHANGE
        );

        // Check for errors
        if (result == 0)
        {
            int errorCode = Marshal.GetLastWin32Error();
            throw new Win32Exception(errorCode, "Failed to set wallpaper.");
        }

        return result;
    }
}