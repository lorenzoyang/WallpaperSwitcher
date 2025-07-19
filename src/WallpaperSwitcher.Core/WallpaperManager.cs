using System.ComponentModel;
using System.Runtime.InteropServices;
using WallpaperSwitcher.Core.Interop;

namespace WallpaperSwitcher.Core;

public sealed class WallpaperManager
{
    private static readonly string[] SupportedExtensions = [".jpg", ".jpeg", ".png", ".bmp"];
    private string _folderPath = string.Empty;
    private List<string> _wallpaperPaths = [];
    private int _currentIndex = 0;

    public WallpaperManager(string folderPath)
    {
        FolderPath = folderPath;
    }

    public string FolderPath
    {
        get => _folderPath;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Folder path cannot be null or empty.", nameof(value));
            }

            if (!Directory.Exists(value))
            {
                throw new DirectoryNotFoundException($"The specified folder does not exist: {value}");
            }

            _folderPath = value;
            _wallpaperPaths = Directory.GetFiles(value)
                .Where(file => SupportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                .ToList();
        }
    }

    public IReadOnlyList<string> WallpaperPaths => _wallpaperPaths.AsReadOnly();

    private int CurrentIndex
    {
        get => _currentIndex;
        set
        {
            if (value >= WallpaperPaths.Count)
            {
                _currentIndex = 0;
            }
            else if (value < 0)
            {
                _currentIndex = WallpaperPaths.Count - 1;
            }
            else
            {
                _currentIndex = value;
            }
        }
    }

    public void NextWallpaper() => WallpaperNativeApi.SetWallpaper(WallpaperPaths[CurrentIndex++]);

    public void PreviousWallpaper() => WallpaperNativeApi.SetWallpaper(WallpaperPaths[CurrentIndex--]);
}