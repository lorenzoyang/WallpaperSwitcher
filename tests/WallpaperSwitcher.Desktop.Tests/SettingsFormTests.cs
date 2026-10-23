using System.Reflection;
using WallpaperSwitcher.Core;
using WallpaperSwitcher.Core.GlobalHotkey;
using WallpaperSwitcher.Core.Persistence;

namespace WallpaperSwitcher.Desktop.Tests;

[Apartment(ApartmentState.STA)]
public class SettingsFormTests
{
    private const string FolderPath = @"C:\Wallpapers";
    private FakeHotkeyStorage _hotkeyStorage = null!;
    private FakeAppSettingsStorage _appSettingsStorage = null!;
    private HotkeyService _hotkeyService = null!;
    private AppSettings _appSettings = null!;
    private List<(bool Enabled, bool StartMinimized)> _startupChanges = null!;

    [SetUp]
    public void SetUp()
    {
        _hotkeyStorage = new FakeHotkeyStorage();
        _appSettingsStorage = new FakeAppSettingsStorage();
        _appSettings = new AppSettings();
        _startupChanges = [];
        _hotkeyService = new HotkeyService(new FakeHotkeyRegistrar(), _hotkeyStorage);
        _hotkeyService.RegisterHotkey("Ctrl+Alt+N", Default.NextWallpaperHotkeyName);
        _hotkeyService.RegisterHotkey("Ctrl+Alt+F", FolderPath);
    }

    [TearDown]
    public void TearDown()
    {
        _hotkeyService.Dispose();
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Confirm_WithoutChanges_DoesNotRecordOrSaveChanges(bool launchAtStartup)
    {
        using var form = CreateForm(launchAtStartup);

        ClickButton(form, "settingsFormOkButton");

        Assert.Multiple(() =>
        {
            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.OK));
            Assert.That(form.HasSavedChanges, Is.False);
            Assert.That(GetControl<CheckBox>(form, "launchStartupCheckBox").Checked,
                Is.EqualTo(launchAtStartup));
            Assert.That(_appSettings.LaunchAtStartup, Is.EqualTo(launchAtStartup));
            Assert.That(_startupChanges, Is.Empty);
            Assert.That(_appSettingsStorage.SaveCount, Is.Zero);
            Assert.That(_hotkeyStorage.SaveCount, Is.Zero);
        });
    }

    [TestCase("nextWallpaperHk")]
    [TestCase("folderHk")]
    public void ModifyHotkey_WhenCancelled_DoesNotRecordOrSaveChanges(string editorPrefix)
    {
        using var form = CreateForm();
        SelectEditor(form, editorPrefix);
        var textBox = GetControl<TextBox>(form, $"{editorPrefix}TextBox");
        var originalText = textBox.Text;
        ClickButton(form, $"{editorPrefix}ModifyButton");
        textBox.Text = "Ctrl+Alt+B";

        RaiseControlEvent(textBox, "OnKeyDown", new KeyEventArgs(Keys.Escape));
        ClickButton(form, "settingsFormOkButton");

        Assert.Multiple(() =>
        {
            Assert.That(form.HasSavedChanges, Is.False);
            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.OK));
            Assert.That(textBox.Text, Is.EqualTo(originalText));
            Assert.That(textBox.ReadOnly, Is.True);
            Assert.That(_hotkeyStorage.SaveCount, Is.Zero);
        });
    }

    [TestCase("nextWallpaperHk", " Ctrl+Alt+N ")]
    [TestCase("folderHk", " Ctrl+Alt+F ")]
    public void SaveHotkey_WhenValueIsEquivalent_DoesNotRecordOrSaveChanges(
        string editorPrefix, string hotkeyText)
    {
        using var form = CreateForm();

        EditAndSaveHotkey(form, editorPrefix, hotkeyText);

        Assert.Multiple(() =>
        {
            Assert.That(form.HasSavedChanges, Is.False);
            Assert.That(_hotkeyStorage.SaveCount, Is.Zero);
            Assert.That(GetControl<TextBox>(form, $"{editorPrefix}TextBox").ReadOnly, Is.True);
            Assert.That(GetControl<Button>(form, "settingsFormOkButton").Enabled, Is.True);
        });
    }

    [TestCase("nextWallpaperHk", "Ctrl+Alt+B")]
    [TestCase("folderHk", "Ctrl+Alt+B")]
    [TestCase("nextWallpaperHk", "")]
    [TestCase("folderHk", "")]
    public void SaveHotkey_WhenBindingChanges_RecordsSuccessfullySavedChanges(
        string editorPrefix, string hotkeyText)
    {
        using var form = CreateForm();

        EditAndSaveHotkey(form, editorPrefix, hotkeyText);
        ClickButton(form, "settingsFormOkButton");

        var savedHotkey = _hotkeyStorage.SavedHotkeys
            .SingleOrDefault(hotkey => hotkey.Name == GetHotkeyName(editorPrefix));
        Assert.Multiple(() =>
        {
            Assert.That(form.HasSavedChanges, Is.True);
            Assert.That(form.DialogResult, Is.EqualTo(DialogResult.OK));
            Assert.That(_hotkeyStorage.SaveCount, Is.EqualTo(1));
            Assert.That(savedHotkey?.ToString() ?? string.Empty, Is.EqualTo(hotkeyText));
            Assert.That(GetControl<TextBox>(form, $"{editorPrefix}TextBox").ReadOnly, Is.True);
        });
    }

    [TestCase("nextWallpaperHk", "Ctrl+Alt+N")]
    [TestCase("folderHk", "Ctrl+Alt+F")]
    public void SaveHotkey_WhenChangedBackToOriginal_StillRecordsSavedChanges(
        string editorPrefix, string originalHotkeyText)
    {
        using var form = CreateForm();

        EditAndSaveHotkey(form, editorPrefix, "Ctrl+Alt+B");
        EditAndSaveHotkey(form, editorPrefix, originalHotkeyText);

        Assert.Multiple(() =>
        {
            Assert.That(form.HasSavedChanges, Is.True);
            Assert.That(_hotkeyStorage.SaveCount, Is.EqualTo(2));
            Assert.That(GetControl<TextBox>(form, $"{editorPrefix}TextBox").Text,
                Is.EqualTo(originalHotkeyText));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ChangeStartup_WhenSaveSucceeds_RecordsSuccessfullySavedChanges(bool launchAtStartup)
    {
        using var form = CreateForm(launchAtStartup);

        GetControl<CheckBox>(form, "launchStartupCheckBox").Checked = !launchAtStartup;

        Assert.Multiple(() =>
        {
            Assert.That(form.HasSavedChanges, Is.True);
            Assert.That(_startupChanges, Is.EqualTo(new[] { (!launchAtStartup, true) }));
            Assert.That(_appSettingsStorage.SaveCount, Is.EqualTo(1));
            Assert.That(_appSettingsStorage.SavedLaunchAtStartup, Is.EqualTo(!launchAtStartup));
            Assert.That(_appSettings.LaunchAtStartup, Is.EqualTo(!launchAtStartup));
        });
    }

    [TestCase("nextWallpaperHk", "Ctrl+Alt+N")]
    [TestCase("folderHk", "Ctrl+Alt+F")]
    public void SaveHotkey_WhenStorageFails_DoesNotRecordChangesAndRestoresBinding(
        string editorPrefix, string originalHotkeyText)
    {
        using var form = CreateForm();
        _hotkeyStorage.SaveException = new IOException("Simulated save failure.");
        var hotkeyName = GetHotkeyName(editorPrefix);

        // Exercise the save operation directly so the error message box does not block the test.
        Assert.ThrowsAsync<IOException>(() => SaveHotkeyAsync(form, hotkeyName, "Ctrl+Alt+B"));

        Assert.Multiple(() =>
        {
            Assert.That(form.HasSavedChanges, Is.False);
            Assert.That(_hotkeyService.GetHotKeyInfoBy(hotkey => hotkey.Name, hotkeyName)?.ToString(),
                Is.EqualTo(originalHotkeyText));
            Assert.That(_hotkeyStorage.SavedHotkeys, Is.Empty);
        });
    }

    [Test]
    public void OpenNewDialog_AfterSavingChanges_StartsWithoutSavedChanges()
    {
        using (var form = CreateForm())
        {
            EditAndSaveHotkey(form, "nextWallpaperHk", "Ctrl+Alt+B");
            Assert.That(form.HasSavedChanges, Is.True);
        }

        using var reopenedForm = CreateForm();

        Assert.Multiple(() =>
        {
            Assert.That(reopenedForm.HasSavedChanges, Is.False);
            Assert.That(GetControl<TextBox>(reopenedForm, "nextWallpaperHkTextBox").Text,
                Is.EqualTo("Ctrl+Alt+B"));
        });
    }

    private TestSettingsForm CreateForm(bool launchAtStartup = false)
    {
        _appSettings.LaunchAtStartup = launchAtStartup;
        var form = new TestSettingsForm(
            _hotkeyService, _appSettingsStorage, _appSettings,
            (enabled, startMinimized) =>
            {
                _startupChanges.Add((enabled, startMinimized));
                return true;
            });
        form.LoadSettings();
        return form;
    }

    private static void SelectEditor(SettingsForm form, string editorPrefix)
    {
        if (editorPrefix == "folderHk")
        {
            GetControl<ComboBox>(form, "folderHkComboBox").SelectedItem = FolderPath;
        }
    }

    private static void EditAndSaveHotkey(SettingsForm form, string editorPrefix, string hotkeyText)
    {
        SelectEditor(form, editorPrefix);
        ClickButton(form, $"{editorPrefix}ModifyButton");
        GetControl<TextBox>(form, $"{editorPrefix}TextBox").Text = hotkeyText;
        ClickButton(form, $"{editorPrefix}SaveButton");
    }

    private static string GetHotkeyName(string editorPrefix)
    {
        return editorPrefix == "nextWallpaperHk" ? Default.NextWallpaperHotkeyName : FolderPath;
    }

    private static T GetControl<T>(SettingsForm form, string name) where T : Control
    {
        return (T)form.Controls.Find(name, searchAllChildren: true).Single();
    }

    private static void ClickButton(SettingsForm form, string name)
    {
        var button = GetControl<Button>(form, name);
        Assert.That(button.Enabled, Is.True, $"{name} must be enabled before clicking it.");
        RaiseControlEvent(button, "OnClick", EventArgs.Empty);
    }

    private static void RaiseControlEvent(Control control, string methodName, EventArgs args)
    {
        // Raise real control events without displaying windows or needing a message loop.
        typeof(Control).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(control, [args]);
    }

    private static Task SaveHotkeyAsync(SettingsForm form, string name, string hotkeyText)
    {
        return (Task)typeof(SettingsForm)
            .GetMethod("SaveHotkeyChangeAsync", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(form, [name, hotkeyText])!;
    }

    private sealed class TestSettingsForm(
        HotkeyService hotkeyService,
        IAppSettingsStorage appSettingsStorage,
        AppSettings appSettings,
        Func<bool, bool, bool> setStartupEnabled)
        : SettingsForm(hotkeyService, [FolderPath], appSettingsStorage, appSettings, null, setStartupEnabled)
    {
        public void LoadSettings() => OnLoad(EventArgs.Empty);
    }

    private sealed class FakeHotkeyRegistrar() : HotkeyRegistrar(new IntPtr(1))
    {
        public override bool RegisterHotKey(int id, ModifierKeys modifiers, VirtualKeys key) => true;

        public override bool UnregisterHotKey(int id) => true;
    }

    private sealed class FakeHotkeyStorage : IHotkeyStorage
    {
        public bool Exists => true;

        public int SaveCount { get; private set; }

        public Exception? SaveException { get; set; }

        public List<HotkeyInfo> SavedHotkeys { get; private set; } = [];

        public IEnumerable<HotkeyInfo> Load() => [];

        public Task<IEnumerable<HotkeyInfo>> LoadAsync() => Task.FromResult(Load());

        public void Save(IEnumerable<HotkeyInfo> hotkeys)
        {
            SaveCount++;
            if (SaveException is not null)
            {
                throw SaveException;
            }

            SavedHotkeys = hotkeys.ToList();
        }

        public Task SaveAsync(IEnumerable<HotkeyInfo> hotkeys)
        {
            Save(hotkeys);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAppSettingsStorage : IAppSettingsStorage
    {
        public int SaveCount { get; private set; }

        public bool SavedLaunchAtStartup { get; private set; }

        public AppSettings Load() => new();

        public Task<AppSettings> LoadAsync() => Task.FromResult(Load());

        public void Save(AppSettings settings)
        {
            SaveCount++;
            SavedLaunchAtStartup = settings.LaunchAtStartup;
        }

        public Task SaveAsync(AppSettings settings)
        {
            Save(settings);
            return Task.CompletedTask;
        }
    }
}
