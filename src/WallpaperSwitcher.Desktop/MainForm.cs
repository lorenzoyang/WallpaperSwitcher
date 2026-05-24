using WallpaperSwitcher.Core.GlobalHotkey;
using WallpaperSwitcher.Core.Persistence;
using WallpaperSwitcher.Core.Wallpaper;

namespace WallpaperSwitcher.Desktop;

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

    // When the user closes the form, if this is true the program exits completely.
    // If false, it minimizes to the system tray.
    private bool IsExiting { get; set; }

    // Allows the form to start hidden while still supporting normal visibility later.
    private bool AllowVisible { get; set; } = true;

    private bool HasLoadedInitialSettings { get; set; }

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
