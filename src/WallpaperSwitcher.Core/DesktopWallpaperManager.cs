using System.Runtime.Versioning;
using Windows.Win32.UI.Shell;
using Windows.Win32;


namespace WallpaperSwitcher.Core;

public sealed class DesktopWallpaperManager
{
    public static readonly string[] SupportedExtensions =
    [
        ".jpg", ".jpeg", ".png", ".bmp", "dib", ".gif", ".tif", ".tiff", "jfif"
    ];

    /// <summary>
    /// A wrapper class for the IDesktopWallpaper interface,
    /// generated via the 'CsWin32' source code generation tool.
    /// </summary>
    private readonly IDesktopWallpaper _desktopWallpaper = new DesktopWallpaper() as IDesktopWallpaper;

    private string _folderPath = string.Empty;
    private List<string> _wallpaperPaths = [];
    private int _currentIndex;

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

    private int CurrentIndex
    {
        get => _currentIndex;
        set
        {
            if (_wallpaperPaths.Count == 0)
            {
                _currentIndex = 0;
                return;
            }

            if (value >= _wallpaperPaths.Count)
            {
                _currentIndex = 0;
            }
            else if (value < 0)
            {
                _currentIndex = _wallpaperPaths.Count - 1;
            }
            else
            {
                _currentIndex = value;
            }
        }
    }

    public void ChangeWallpaperFolder(string newFolderPath) => FolderPath = newFolderPath;

    public void NextWallpaper()
    {
        if (_wallpaperPaths.Count is 0 or 1) return;
        CurrentIndex++;
        _desktopWallpaper.SetWallpaper(null, _wallpaperPaths[CurrentIndex]);
    }

    public void PreviousWallpaper()
    {
        if (_wallpaperPaths.Count is 0 or 1) return;
        CurrentIndex--;
        _desktopWallpaper.SetWallpaper(null, _wallpaperPaths[CurrentIndex]);
    }

    /// <summary>
    /// Start "playing" from the current desktop wallpaper.
    /// If the current desktop wallpaper is not in the currently selected wallpaper folder,
    /// start from the first wallpaper (sorted by name) in the current folder.
    /// </summary>
    public void Start()
    {
        if (_wallpaperPaths.Count is 0) return;

        _desktopWallpaper.GetWallpaper(null, out var nativeWallpaperPath);
        var currentWallpaperPath = nativeWallpaperPath.ToString() ?? string.Empty;

        CurrentIndex = _wallpaperPaths.IndexOf(currentWallpaperPath);
        if (CurrentIndex < 0)
        {
            CurrentIndex = 0;
        }

        _desktopWallpaper.SetWallpaper(null, _wallpaperPaths[CurrentIndex]);
    }

    [SupportedOSPlatform("windows8.0")]
    public void SetSlideShow()
    {
        _desktopWallpaper.SetSlideshow(DesktopWallpaperHelper.CreateShellItemArrayFromFolder(_folderPath));
    }
}