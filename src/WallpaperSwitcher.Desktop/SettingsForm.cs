using WallpaperSwitcher.Core.GlobalHotKey;

namespace WallpaperSwitcher.Desktop;

public partial class SettingsForm : Form
{
    private readonly GlobalHotkeyManager _globalHotkeyManager;
    private List<string> _folders = [];
    private readonly string DefaultNextWallpaperHotkey = "CTRL+SHIFT+N";

    public SettingsForm(GlobalHotkeyManager globalHotkeyManager, List<string> folders)
    {
        InitializeComponent();

        _globalHotkeyManager = globalHotkeyManager;
        _folders = folders;
    }

    private void LoadSettings()
    {
        // Load the Next Wallpaper Hotkey
    }

    // *********************************
    // Event handlers for Form events  *
    // *********************************
}