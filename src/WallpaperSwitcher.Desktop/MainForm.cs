using WallpaperSwitcher.Core.GlobalHotkey;
using WallpaperSwitcher.Core.Persistence;
using WallpaperSwitcher.Core.Wallpaper;

namespace WallpaperSwitcher.Desktop;

/// <summary>
/// Main application window responsible for coordinating settings, tray behavior,
/// wallpaper switching, and global hotkey dispatch.
/// </summary>
public partial class MainForm : Form
{
    private const string ApplicationTitle = "Wallpaper Switcher";
    private const int NativeModeIndex = 0;
    private const int CustomModeIndex = 1;

    private readonly IAppSettingsStorage _appSettingsStorage = new JsonAppSettingsStorage();

    private readonly AppSettings _appSettings;

    private readonly HotkeyService _hotkeyService;

    private readonly WallpaperManager _wallpaperManager;

    private readonly ToolTip _toolTip = new()
    {
        AutoPopDelay = 10000,
        InitialDelay = 500,
        ReshowDelay = 100,
        ShowAlways = true
    };

    private readonly NotifyIcon _trayIcon;

    // Distinguishes an explicit exit from the default close-to-tray behavior.
    private bool IsExiting { get; set; }

    // Gates initial visibility so --minimized can start directly in the tray.
    private bool AllowVisible { get; set; } = true;

    private bool HasLoadedInitialSettings { get; set; }

    /// <summary>
    /// Initializes the main form and optionally starts it hidden in the system tray.
    /// </summary>
    /// <param name="startMinimized">
    /// <see langword="true"/> to load settings without showing the main window.
    /// </param>
    public MainForm(bool startMinimized = false)
    {
        _appSettings = _appSettingsStorage.Load();
        _wallpaperManager = CreateWallpaperManager(_appSettings.SelectedModeIndex);

        InitializeComponent();

        _trayIcon = CreateTrayIcon();
        InitializeSystemTray();

        _hotkeyService = CreateHotkeyService();
        _hotkeyService.HotkeyPressed += HandleHotkeyPressed;

        if (startMinimized)
        {
            AllowVisible = false;
            LoadInitialSettings();
        }
    }

    protected override void SetVisibleCore(bool value)
    {
        base.SetVisibleCore(AllowVisible && value);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == FormHelper.WmShowFirstInstanceMessage)
        {
            ShowMainForm();
        }

        base.WndProc(ref m);

        if (m.Msg == HotkeyService.WmHotkey)
        {
            _hotkeyService.ProcessWindowMessage(m.WParam.ToInt32());
        }
    }

    private HotkeyService CreateHotkeyService()
    {
        return new HotkeyService(
            new Win32HotkeyRegistrar(Handle),
            new JsonHotkeyStorage()
        );
    }

    private NotifyIcon CreateTrayIcon()
    {
        return new NotifyIcon
        {
            Icon = Icon,
            Visible = true,
            Text = ApplicationTitle
        };
    }
}
