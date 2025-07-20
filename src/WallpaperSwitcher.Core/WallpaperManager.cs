using WallpaperSwitcher.Core.Interop;

namespace WallpaperSwitcher.Core;

public sealed class WallpaperManager
{
    public static readonly string[] SupportedExtensions = [".jpg", ".jpeg", ".png", ".bmp"];

    private string _folderPath = string.Empty;
    private List<string> _wallpaperPaths = [];
    private int _currentIndex;

    public WallpaperManager()
    {
    }

    public WallpaperManager(string folderPath)
    {
        FolderPath = folderPath;
    }

    public string FolderPath
    {
        get => _folderPath;
        private set
        {
            if (string.IsNullOrEmpty(value))
            {
                _folderPath = string.Empty;
                _wallpaperPaths.Clear();
                CurrentIndex = 0;
                return;
            }

            if (!Directory.Exists(value))
            {
                throw new DirectoryNotFoundException($"The specified folder does not exist: {value}");
            }

            _folderPath = value;
            _wallpaperPaths = Directory.GetFiles(value)
                .Where(file => SupportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                .OrderBy(Path.GetFileName)
                .ToList();
            CurrentIndex = 0;
        }
    }

    private IReadOnlyList<string> WallpaperPaths => _wallpaperPaths.AsReadOnly();

    private int CurrentIndex
    {
        get => _currentIndex;
        set
        {
            if (WallpaperPaths.Count == 0)
            {
                _currentIndex = 0;
                return;
            }

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

    public void ChangeWallpaperFolder(string newFolderPath)
    {
        FolderPath = newFolderPath;
    }

    public void NextWallpaper()
    {
        if (WallpaperPaths.Count is 0 or 1) return;
        WallpaperNativeApi.SetWallpaper(WallpaperPaths[CurrentIndex++]);
    }

    public void PreviousWallpaper()
    {
        if (WallpaperPaths.Count is 0 or 1) return;
        WallpaperNativeApi.SetWallpaper(WallpaperPaths[CurrentIndex--]);
    }
}