using WallpaperSwitcher.Core;
using WallpaperSwitcher.Core.GlobalHotkey;
using WallpaperSwitcher.Core.Persistence;
using WallpaperSwitcher.Core.Updates;

namespace WallpaperSwitcher.Desktop;

/// <summary>
/// Dialog for editing application startup behavior and global hotkey bindings.
/// </summary>
public partial class SettingsForm : Form
{
    private const string CheckForUpdatesButtonDefaultText = "Check for Updates";
    private const string CheckForUpdatesButtonBusyText = "Checking...";

    private readonly HotkeyService _hotkeyService;
    private readonly IAppSettingsStorage _appSettingsStorage;
    private readonly AppSettings _appSettings;
    private readonly IUpdateChecker _updateChecker;

    // Caches folder hotkeys while the dialog is open so combo-box changes can update quickly.
    private readonly Dictionary<string, HotkeyInfo?> _folderHotkeys;

    /// <summary>
    /// Initializes a settings dialog backed by the running hotkey service and shared app settings.
    /// </summary>
    /// <param name="hotkeyService">The already initialized hotkey service owned by the main form.</param>
    /// <param name="folders">The currently configured wallpaper folders.</param>
    /// <param name="appSettingsStorage">The storage provider used to persist setting changes.</param>
    /// <param name="appSettings">The mutable settings instance shared with the main form.</param>
    public SettingsForm(
        HotkeyService hotkeyService,
        List<string> folders,
        IAppSettingsStorage appSettingsStorage,
        AppSettings appSettings,
        IUpdateChecker? updateChecker = null)
    {
        InitializeComponent();

        _hotkeyService = hotkeyService;
        _appSettingsStorage = appSettingsStorage;
        _appSettings = appSettings;
        _updateChecker = updateChecker ?? new GitHubUpdateChecker();
        _folderHotkeys = folders.ToDictionary(folder => folder, HotkeyInfo? (_) => null);
    }

    private void LoadInitialSettings()
    {
        nextWallpaperHkTextBox.Text =
            _hotkeyService.GetHotKeyInfoBy(h => h.Name, Default.NextWallpaperHotkeyName)?.ToString() ??
            string.Empty;

        var hotkeyInfosWithoutNextWallpaper = _hotkeyService
            .GetRegisteredHotkeys()
            .Where(hotkeyInfo => hotkeyInfo.Name != Default.NextWallpaperHotkeyName)
            .ToList();

        foreach (var hotkeyInfo in hotkeyInfosWithoutNextWallpaper.Where(hotkeyInfo =>
                     _folderHotkeys.ContainsKey(hotkeyInfo.Name)))
        {
            _folderHotkeys[hotkeyInfo.Name] = hotkeyInfo;
        }

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

    private string OriginalValue { get; set; } = string.Empty;

    private void SettingsForm_Load(object sender, EventArgs e)
    {
        // Keep the first editable hotkey box unfocused until the user chooses to modify it.
        ActiveControl = nextWallpaperHkLabel;
        LoadInitialSettings();
    }

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

    private void folderHkComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Folder hotkey editing starts disabled until the user selects a folder.
        if (!folderHkModifyButton.Enabled) folderHkModifyButton.Enabled = true;

        if (!TryGetSelectedFolder(out var selectedFolder)) return;
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

    private void settingsFormOkButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
    }

    private async void checkForUpdatesButton_Click(object sender, EventArgs e)
    {
        await CheckForUpdatesAsync();
    }

    private async Task CheckForUpdatesAsync()
    {
        SetUpdateCheckInProgress(true);

        try
        {
            var result = await _updateChecker.CheckForUpdatesAsync(GetCurrentApplicationVersion());
            if (result.IsUpdateAvailable)
            {
                ShowUpdateAvailableMessage(result);
                return;
            }

            FormHelper.ShowSuccessMessage(
                $"You are using the latest version.\n\nCurrent version: {FormatVersion(result.CurrentVersion)}",
                "No Updates Found"
            );
        }
        catch (UpdateCheckException exception)
        {
            FormHelper.ShowWarningMessage(
                $"Unable to check for updates: {exception.Message}\n\n" +
                "Please check your internet connection and try again.",
                "Update Check Failed"
            );
        }
        finally
        {
            SetUpdateCheckInProgress(false);
        }
    }

    private void ShowUpdateAvailableMessage(UpdateCheckResult result)
    {
        var dialogResult = MessageBox.Show(
            this,
            "A new version of Wallpaper Switcher is available.\n\n" +
            $"Current version: {FormatVersion(result.CurrentVersion)}\n" +
            $"Latest version: {FormatVersion(result.LatestVersion)}\n\n" +
            "Open the GitHub release page?",
            "Update Available",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Information
        );

        if (dialogResult != DialogResult.Yes)
        {
            return;
        }

        try
        {
            FormHelper.OpenUrl(result.ReleaseUri);
        }
        catch (Exception exception)
        {
            FormHelper.ShowWarningMessage(
                $"Unable to open the release page: {exception.Message}",
                "Open Release Page Failed"
            );
        }
    }

    private void SetUpdateCheckInProgress(bool isChecking)
    {
        if (isChecking)
        {
            // Move focus before disabling the clicked button so WinForms does not select the hotkey textbox.
            settingsFormOkButton.Focus();
        }

        checkForUpdatesButton.Enabled = !isChecking;
        checkForUpdatesButton.Text = isChecking
            ? CheckForUpdatesButtonBusyText
            : CheckForUpdatesButtonDefaultText;
    }

    private static Version GetCurrentApplicationVersion()
    {
        return typeof(SettingsForm).Assembly.GetName().Version ?? new Version(0, 0, 0);
    }

    private static string FormatVersion(Version version)
    {
        return $"{version.Major}.{Math.Max(version.Minor, 0)}.{Math.Max(version.Build, 0)}";
    }

    private void launchStartupCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        var requestedStartupState = launchStartupCheckBox.Checked;
        var actionMessage = requestedStartupState ? "enable" : "disable";

        try
        {
            var success = StartupManager.SetStartupEnabled(
                requestedStartupState,
                startMinimized: true // Startup launches to the tray instead of showing the main window.
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
