using System.ComponentModel;
using System.Reflection;
using WallpaperSwitcher.Core.GlobalHotkey;
using WallpaperSwitcher.Core.Persistence;
using WallpaperSwitcher.Core.Wallpaper;

namespace WallpaperSwitcher.Desktop.Tests;

[Apartment(ApartmentState.STA)]
public class MainFormTests
{
    private string _directory = null!;
    private string _first = null!;
    private string _second = null!;
    private JsonAppSettingsStorage _settings = null!;
    private JsonHotkeyStorage _hotkeys = null!;

    [SetUp]
    public void SetUp()
    {
        _directory = Directory.CreateTempSubdirectory("WallpaperSwitcher-").FullName;
        _first = Path.Combine(_directory, "A");
        _second = Path.Combine(_directory, "B");
        _settings = new JsonAppSettingsStorage(Path.Combine(_directory, "settings.json"));
        _settings.Save(new AppSettings { WallpaperFolders = [_first, _second], LastSelectedFolder = _first });
        _hotkeys = new JsonHotkeyStorage(Path.Combine(_directory, "hotkeys.json"));
        _hotkeys.Save([]);
    }

    [TearDown]
    public void TearDown() => Directory.Delete(_directory, true);

    [Test]
    public void Startup_WhenNativeFolderIsOffline_KeepsPreferenceAcrossExitAndRestart()
    {
        using (var form = OpenForm(new NativeWallpaperManager(), out var hotkeys))
        {
            Assert.That(form.Folders.Items.Cast<string>(), Is.EqualTo(new[] { _first, _second }));
            Assert.That(form.Folders.SelectedIndex, Is.EqualTo(-1));
            Assert.That(form.NextButton.Enabled, Is.False);
            Assert.That(form.TrayMenu().Items[1].Enabled, Is.False);
            form.DispatchHotkey(hotkeys.RegisterHotkey("Ctrl+Alt+N", Default.NextWallpaperHotkeyName));
            form.SaveOnExit();
        }

        Assert.That(_settings.Load().LastSelectedFolder, Is.EqualTo(_first));
        using var restarted = OpenForm(new RecordingWallpaperManager(), out _);
        Assert.That(restarted.Folders.SelectedItem, Is.EqualTo(_first));
        Assert.That(restarted.NextButton.Enabled, Is.True);
    }

    [TestCase("window")]
    [TestCase("tray")]
    [TestCase("hotkey")]
    public void Switch_WhenFolderIsRejected_KeepsPreviousFolderForAllCommands(string source)
    {
        var manager = new RecordingWallpaperManager { UnavailableFolder = _second };
        using var form = OpenForm(manager, out var hotkeys);
        switch (source)
        {
            case "window": form.Folders.SelectedItem = _second; break;
            case "tray": form.TrayFolders().Last().PerformClick(); break;
            case "hotkey": form.DispatchHotkey(hotkeys.RegisterHotkey("Ctrl+Alt+B", _second)); break;
        }

        Assert.That(form.Folders.SelectedItem, Is.EqualTo(_first));
        Assert.That(form.TrayFolders().Select(item => item.Checked), Is.EqualTo(new[] { true, false }));
        form.DispatchHotkey(hotkeys.RegisterHotkey("Ctrl+Alt+N", Default.NextWallpaperHotkeyName));
        Assert.That(manager.LastAdvancedFolder, Is.EqualTo(_first));
        form.SaveOnExit();
        Assert.That(_settings.Load().LastSelectedFolder, Is.EqualTo(_first));

        manager.UnavailableFolder = null;
        form.TrayFolders().Last().PerformClick();
        Assert.That(form.Folders.SelectedItem, Is.EqualTo(_second));
        Assert.That(form.TrayFolders().Last().Checked, Is.True);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Remove_WhenFolderIsActiveOrRemembered_ClearsSelectionAndPreference(bool unavailable)
    {
        var manager = new RecordingWallpaperManager { UnavailableFolder = unavailable ? _first : null };
        using var form = OpenForm(manager, out var hotkeys);
        await form.RemoveFolderAsync(_first);
        Assert.That(form.NextButton.Enabled, Is.False);
        form.DispatchHotkey(hotkeys.RegisterHotkey("Ctrl+Alt+N", Default.NextWallpaperHotkeyName));
        Assert.That(manager.LastAdvancedFolder, Is.Null);
        Assert.That(_settings.Load().LastSelectedFolder, Is.Empty);
        Assert.That(_settings.Load().WallpaperFolders, Is.EqualTo(new[] { _second }));
    }

    private TestMainForm OpenForm(WallpaperManager manager, out HotkeyService hotkeys)
    {
        hotkeys = new HotkeyService(new TestRegistrar(), _hotkeys);
        return new TestMainForm(_settings, manager, hotkeys);
    }

    private sealed class TestMainForm(IAppSettingsStorage settings, WallpaperManager manager, HotkeyService hotkeys)
        : MainForm(true, settings, manager, hotkeys)
    {
        public ComboBox Folders => (ComboBox)Controls.Find("currentFolderComboBox", true).Single();
        public Button NextButton => (Button)Controls.Find("nextWallpaperButton", true).Single();
        public void SaveOnExit() => OnFormClosing(new FormClosingEventArgs(CloseReason.ApplicationExitCall, false));

        public void DispatchHotkey(int id)
        {
            var message = Message.Create(Handle, HotkeyService.WmHotkey, id, 0);
            WndProc(ref message);
        }

        public ContextMenuStrip TrayMenu()
        {
            var icon = (NotifyIcon)typeof(MainForm).GetField("_trayIcon", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(this)!;
            var menu = icon.ContextMenuStrip!;
            typeof(ToolStripDropDown).GetMethod("OnOpening", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(menu, [new CancelEventArgs()]);
            return menu;
        }

        public IEnumerable<ToolStripMenuItem> TrayFolders() =>
            ((ToolStripMenuItem)TrayMenu().Items[0]).DropDownItems.OfType<ToolStripMenuItem>();
    }

    private sealed class TestRegistrar() : HotkeyRegistrar(new IntPtr(1))
    {
        public override bool RegisterHotKey(int id, Core.GlobalHotkey.ModifierKeys modifiers, VirtualKeys key) => true;
        public override bool UnregisterHotKey(int id) => true;
    }

    private sealed class RecordingWallpaperManager : WallpaperManager
    {
        public string? UnavailableFolder { get; set; }
        public string? LastAdvancedFolder { get; private set; }
        public override void SetSlideShow(string folder) => SlideShowFolder = folder;
        public override void SetWallpaper(string wallpaper) { }
        public override void AdvanceForwardSlideshow() => LastAdvancedFolder = SlideShowFolder;

        public override bool TrySetSlideShow(string folder, out string errorMessage)
        {
            if (folder != UnavailableFolder) return base.TrySetSlideShow(folder, out errorMessage);
            errorMessage = "Folder is unavailable.";
            return false;
        }
    }
}
