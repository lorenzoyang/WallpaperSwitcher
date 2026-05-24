using WallpaperSwitcher.Core;
using WallpaperSwitcher.Core.GlobalHotkey;
using WallpaperSwitcher.Core.Persistence;

namespace WallpaperSwitcher.Desktop;

public partial class SettingsForm : Form
{
    private readonly HotkeyService _hotkeyService;
    private readonly IAppSettingsStorage _appSettingsStorage;
    private readonly AppSettings _appSettings;

    // Dictionary to hold folder hotkeys, where the key is the folder path and the value is the HotkeyInfo
    private readonly Dictionary<string, HotkeyInfo?> _folderHotkeys;

    public SettingsForm(
        HotkeyService hotkeyService,
        List<string> folders,
        IAppSettingsStorage appSettingsStorage,
        AppSettings appSettings)
    {
        InitializeComponent();

        // GlobalHotkeyManager passed from the main form that is already initialized
        _hotkeyService = hotkeyService;
        _appSettingsStorage = appSettingsStorage;
        _appSettings = appSettings;
        // Initialize the folder hotkeys dictionary with the provided folders
        _folderHotkeys = folders.ToDictionary(folder => folder, HotkeyInfo? (_) => null);
    }

    private void LoadInitialSettings()
    {
        // Display Next Wallpaper Hotkey
        nextWallpaperHkTextBox.Text =
            _hotkeyService.GetHotKeyInfoBy(h => h.Name, Default.NextWallpaperHotkeyName)?.ToString() ??
            string.Empty;

        var hotkeyInfosWithoutNextWallpaper = _hotkeyService
            .GetRegisteredHotkeys()
            .Where(hotkeyInfo => hotkeyInfo.Name != Default.NextWallpaperHotkeyName)
            .ToList();
        // Populate the folder hotkeys dictionary with existing hotkeys, excluding the next wallpaper hotkey
        foreach (var hotkeyInfo in hotkeyInfosWithoutNextWallpaper.Where(hotkeyInfo =>
                     _folderHotkeys.ContainsKey(hotkeyInfo.Name)))
        {
            _folderHotkeys[hotkeyInfo.Name] = hotkeyInfo;
        }

        // Display Folder Hotkeys in the ComboBox
        foreach (var folder in _folderHotkeys.Keys)
        {
            folderHkComboBox.Items.Add(folder);
        }

        launchStartupCheckBox.Checked = _appSettings.LaunchAtStartup;
    }

    private void SetNextWallpaperHkEditMode(bool isEditing)
    {
        SetHotkeyEditMode(
            nextWallpaperHkTextBox,
            nextWallpaperHkSaveButton,
            nextWallpaperHkModifyButton,
            folderHkModifyButton,
            isEditing
        );
    }

    private void SetFolderHkEditMode(bool isEditing)
    {
        SetHotkeyEditMode(
            folderHkTextBox,
            folderHkSaveButton,
            folderHkModifyButton,
            nextWallpaperHkModifyButton,
            isEditing
        );
    }

    private void SetHotkeyEditMode(
        TextBox textBox,
        Button saveButton,
        Button modifyButton,
        Button otherModifyButton,
        bool isEditing)
    {
        textBox.ReadOnly = !isEditing;
        saveButton.Enabled = isEditing;
        modifyButton.Enabled = !isEditing;
        otherModifyButton.Enabled = !isEditing;
        OriginalValue = isEditing ? OriginalValue : string.Empty;
        settingsFormOkButton.Enabled = !isEditing;
    }

    private void SaveSettings()
    {
        _appSettings.LaunchAtStartup = launchStartupCheckBox.Checked;
        _appSettingsStorage.Save(_appSettings);
    }

    // *********************************
    // Event handlers for Form events  *
    // *********************************
    private string OriginalValue { get; set; } = string.Empty;

    private void SettingsForm_Load(object sender, EventArgs e)
    {
        // To prevent any control from being focused when the form loads
        ActiveControl = nextWallpaperHkLabel;
        LoadInitialSettings();
    }

    //
    // Event handlers for Next Wallpaper Hotkey 
    //
    private void nextWallpaperHkModifyButton_Click(object sender, EventArgs e)
    {
        OriginalValue = nextWallpaperHkTextBox.Text;
        nextWallpaperHkTextBox.Focus();
        SetNextWallpaperHkEditMode(true);
    }

    private async void nextWallpaperHkSaveButton_Click(object sender, EventArgs e)
    {
        try
        {
            var newHotkeyText = nextWallpaperHkTextBox.Text.Trim();
            if (newHotkeyText == OriginalValue)
            {
                SetNextWallpaperHkEditMode(false);
                return;
            }

            await SaveHotkeyChangeAsync(Default.NextWallpaperHotkeyName, newHotkeyText);

            SetNextWallpaperHkEditMode(false);
        }
        catch (Exception exception)
        {
            SetNextWallpaperHkEditMode(false);
            nextWallpaperHkTextBox.Text = string.Empty;
            FormHelper.ShowErrorMessage(
                $"Failed to save the hotkey for '{Default.NextWallpaperHotkeyName}': {exception.Message}, please try again.",
                "Error Saving Hotkey"
            );
        }
    }
    // End of Event handlers for Next Wallpaper Hotkey

    //
    // Event handlers for Folder Hotkeys
    //
    private void folderHkComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Di default no item is selected, so when the form loads, the button is disabled.
        // If the user selects an item, the button is enabled.
        if (!folderHkModifyButton.Enabled) folderHkModifyButton.Enabled = true;

        if (!TryGetSelectedFolder(out var selectedFolder)) return;
        // Update the TextBox to display the corresponding hotkey (if defined).
        if (_folderHotkeys.TryGetValue(selectedFolder, out var hotkeyInfo))
        {
            folderHkTextBox.Text = hotkeyInfo?.ToString() ?? string.Empty;
        }
    }

    private void folderHkModifyButton_Click(object sender, EventArgs e)
    {
        OriginalValue = folderHkTextBox.Text;
        folderHkTextBox.Focus();
        SetFolderHkEditMode(true);
    }

    private async void folderHkSaveButton_Click(object sender, EventArgs e)
    {
        try
        {
            var newHotkeyText = folderHkTextBox.Text.Trim();
            if (newHotkeyText == OriginalValue)
            {
                SetFolderHkEditMode(false);
                return;
            }

            if (!TryGetSelectedFolder(out var selectedFolder))
            {
                SetFolderHkEditMode(false);
                return;
            }

            await SaveHotkeyChangeAsync(selectedFolder, newHotkeyText);
            RefreshFolderHotkey(selectedFolder);

            SetFolderHkEditMode(false);
        }
        catch (Exception exception)
        {
            SetFolderHkEditMode(false);
            folderHkTextBox.Text = string.Empty;
            FormHelper.ShowErrorMessage(
                $"Failed to save the hotkey for the selected folder: {exception.Message}, please try again.",
                "Error Saving Hotkey"
            );
        }
    }
    // End of Event handlers for Folder Hotkeys

    private void settingsFormOkButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
    }

    private void launchStartupCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        var requestedStartupState = launchStartupCheckBox.Checked;
        var actionMessage = requestedStartupState ? "enable" : "disable";

        try
        {
            var success = StartupManager.SetStartupEnabled(
                requestedStartupState,
                startMinimized: true // Always start minimized to system tray
            );
            if (!success)
            {
                RevertCheckboxState();
                FormHelper.ShowErrorMessageWithLink(
                    $"Failed to {actionMessage} launch at startup. Please check your permissions and try again.",
                    "Startup Registration Error"
                );
            }

            SaveSettings();
        }
        catch (Exception exception)
        {
            RevertCheckboxState();
            FormHelper.ShowErrorMessageWithLink(
                $"An error occurred while updating startup settings: {exception.Message}",
                "Startup Registration Error"
            );
        }

        return;

        void RevertCheckboxState()
        {
            launchStartupCheckBox.CheckedChanged -= launchStartupCheckBox_CheckedChanged;
            launchStartupCheckBox.Checked = !launchStartupCheckBox.Checked;
            launchStartupCheckBox.CheckedChanged += launchStartupCheckBox_CheckedChanged;
        }
    }

    private async Task SaveHotkeyChangeAsync(string name, string newHotkeyText)
    {
        ApplyHotkeyChange(name, newHotkeyText);
        await _hotkeyService.SaveHotkeysAsync();
    }

    private void ApplyHotkeyChange(string name, string newHotkeyText)
    {
        if (string.IsNullOrEmpty(newHotkeyText))
        {
            _ = _hotkeyService.UnregisterHotkey(name);
        }
        else if (string.IsNullOrEmpty(OriginalValue))
        {
            _ = _hotkeyService.RegisterHotkey(newHotkeyText, name);
        }
        else
        {
            _hotkeyService.ChangeHotkeyBinding(name, newHotkeyText);
        }
    }

    private bool TryGetSelectedFolder(out string selectedFolder)
    {
        selectedFolder = folderHkComboBox.SelectedItem?.ToString() ?? string.Empty;
        return !string.IsNullOrEmpty(selectedFolder);
    }

    private void RefreshFolderHotkey(string folder)
    {
        var hotkeyInfo = _hotkeyService.GetHotKeyInfoBy(h => h.Name, folder);
        _folderHotkeys[folder] = hotkeyInfo;
        folderHkTextBox.Text = hotkeyInfo?.ToString() ?? string.Empty;
    }
}
