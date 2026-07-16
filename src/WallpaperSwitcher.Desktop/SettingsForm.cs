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
    private const string HotkeyInputDefaultHint =
        "Click Modify, then press Ctrl/Alt/Shift/Win + A–Z or type a hotkey.";
    private const string HotkeyInputEditingHint =
        "Press Ctrl/Alt/Shift/Win + A–Z, or type a hotkey. Esc cancels; clear the field to disable.";

    private static readonly Color HotkeyInputErrorColor = Color.FromArgb(185, 28, 28);
    private static readonly Color HotkeyInputSuccessColor = Color.FromArgb(21, 128, 61);

    private readonly HotkeyService _hotkeyService;
    private readonly IAppSettingsStorage _appSettingsStorage;
    private readonly AppSettings _appSettings;
    private readonly IUpdateChecker _updateChecker;
    private CancellationTokenSource? _updateCheckCancellationTokenSource;

    // Tracks the single hotkey editor currently active in this dialog.
    private TextBox? _activeHotkeyTextBox;
    private Button? _activeHotkeySaveButton;
    private string _activeHotkeyName = string.Empty;
    private string _originalHotkeyValue = string.Empty;
    private bool _isClosing;

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

    private void BeginHotkeyEdit(TextBox textBox, Button saveButton, string hotkeyName)
    {
        _activeHotkeyTextBox = textBox;
        _activeHotkeySaveButton = saveButton;
        _activeHotkeyName = hotkeyName;
        _originalHotkeyValue = textBox.Text;

        textBox.ReadOnly = false;
        nextWallpaperHkModifyButton.Enabled = false;
        folderHkModifyButton.Enabled = false;
        folderHkComboBox.Enabled = false;
        settingsFormOkButton.Enabled = false;

        ValidateActiveHotkeyText();
        textBox.Focus();
        textBox.SelectAll();
    }

    private void EndHotkeyEdit(bool restoreOriginalValue)
    {
        if (_activeHotkeyTextBox is null)
        {
            return;
        }

        if (restoreOriginalValue)
        {
            _activeHotkeyTextBox.Text = _originalHotkeyValue;
        }

        _activeHotkeyTextBox.ReadOnly = true;
        _activeHotkeySaveButton!.Enabled = false;
        _activeHotkeyTextBox = null;
        _activeHotkeySaveButton = null;
        _activeHotkeyName = string.Empty;
        _originalHotkeyValue = string.Empty;

        nextWallpaperHkModifyButton.Enabled = true;
        folderHkModifyButton.Enabled = folderHkComboBox.SelectedItem is not null;
        folderHkComboBox.Enabled = true;
        settingsFormOkButton.Enabled = true;
        SetHotkeyInputHint(HotkeyInputDefaultHint);
        ActiveControl = nextWallpaperHkLabel;
    }

    private void SaveSettings()
    {
        _appSettings.LaunchAtStartup = launchStartupCheckBox.Checked;
        _appSettingsStorage.Save(_appSettings);
    }

    private void SettingsForm_Load(object sender, EventArgs e)
    {
        // Keep the first editable hotkey box unfocused until the user chooses to modify it.
        ActiveControl = nextWallpaperHkLabel;
        LoadInitialSettings();

        // Registered combinations can arrive through WM_HOTKEY instead of a text-box KeyDown.
        _hotkeyService.HotkeyPressed += hotkeyService_HotkeyPressed;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _isClosing = true;
        _updateCheckCancellationTokenSource?.Cancel();
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        // The shared service outlives this dialog, so it must not retain the closed form.
        _hotkeyService.HotkeyPressed -= hotkeyService_HotkeyPressed;
        base.OnFormClosed(e);
    }

    private void nextWallpaperHkModifyButton_Click(object sender, EventArgs e)
    {
        BeginHotkeyEdit(
            nextWallpaperHkTextBox,
            nextWallpaperHkSaveButton,
            Default.NextWallpaperHotkeyName
        );
    }

    private async void nextWallpaperHkSaveButton_Click(object sender, EventArgs e)
    {
        try
        {
            if (!TryNormalizeHotkeyText(nextWallpaperHkTextBox.Text, out var newHotkeyText))
            {
                return;
            }

            nextWallpaperHkTextBox.Text = newHotkeyText;
            if (newHotkeyText == _originalHotkeyValue)
            {
                EndHotkeyEdit(restoreOriginalValue: false);
                return;
            }

            nextWallpaperHkSaveButton.Enabled = false;
            await SaveHotkeyChangeAsync(Default.NextWallpaperHotkeyName, newHotkeyText);
            RefreshHotkeyText(Default.NextWallpaperHotkeyName, nextWallpaperHkTextBox);
            EndHotkeyEdit(restoreOriginalValue: false);
        }
        catch (Exception exception)
        {
            FormHelper.ShowErrorMessage(
                $"Failed to save the hotkey for '{Default.NextWallpaperHotkeyName}': {exception.Message}, please try again.",
                "Error Saving Hotkey"
            );
            RestoreHotkeyEditorAfterSaveFailure(nextWallpaperHkTextBox);
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
        if (!TryGetSelectedFolder(out var selectedFolder))
        {
            return;
        }

        BeginHotkeyEdit(folderHkTextBox, folderHkSaveButton, selectedFolder);
    }

    private async void folderHkSaveButton_Click(object sender, EventArgs e)
    {
        try
        {
            if (!TryNormalizeHotkeyText(folderHkTextBox.Text, out var newHotkeyText))
            {
                return;
            }

            folderHkTextBox.Text = newHotkeyText;
            if (newHotkeyText == _originalHotkeyValue)
            {
                EndHotkeyEdit(restoreOriginalValue: false);
                return;
            }

            var selectedFolder = _activeHotkeyName;
            folderHkSaveButton.Enabled = false;
            await SaveHotkeyChangeAsync(selectedFolder, newHotkeyText);
            RefreshFolderHotkey(selectedFolder);
            EndHotkeyEdit(restoreOriginalValue: false);
        }
        catch (Exception exception)
        {
            FormHelper.ShowErrorMessage(
                $"Failed to save the hotkey for the selected folder: {exception.Message}, please try again.",
                "Error Saving Hotkey"
            );
            RestoreHotkeyEditorAfterSaveFailure(folderHkTextBox);
        }
    }

    private void hotkeyTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        // KeyPreview also routes form-level events here; ignore unrelated text boxes.
        if (_activeHotkeyTextBox is null ||
            sender is TextBox textBox && !ReferenceEquals(textBox, _activeHotkeyTextBox))
        {
            return;
        }

        if (e.KeyCode == Keys.Escape)
        {
            SuppressKey(e);
            EndHotkeyEdit(restoreOriginalValue: true);
            return;
        }

        var captureResult = HotkeyCaptureInterpreter.Interpret(
            e.KeyCode,
            e.Modifiers,
            HotkeyCaptureInterpreter.IsWindowsKeyPressed()
        );

        switch (captureResult.Status)
        {
            case HotkeyCaptureStatus.ManualInput:
                return;
            case HotkeyCaptureStatus.WaitingForPrimaryKey:
                SuppressKey(e);
                SetHotkeyInputHint(captureResult.Message);
                return;
            case HotkeyCaptureStatus.Recorded when captureResult.Hotkey is { } hotkey:
                SuppressKey(e);
                RecordHotkey(hotkey);
                return;
            case HotkeyCaptureStatus.Unsupported:
                SuppressKey(e);
                SetHotkeyInputHint(captureResult.Message, isError: true);
                return;
        }
    }

    private void hotkeyTextBox_TextChanged(object sender, EventArgs e)
    {
        if (ReferenceEquals(sender, _activeHotkeyTextBox))
        {
            ValidateActiveHotkeyText();
        }
    }

    private void hotkeyService_HotkeyPressed(object? sender, HotkeyPressedEventArgs e)
    {
        if (_activeHotkeyTextBox is null || _isClosing)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(() => RecordHotkey(e.HotkeyInfo.Hotkey));
            return;
        }

        RecordHotkey(e.HotkeyInfo.Hotkey);
    }

    private void RecordHotkey(Hotkey hotkey)
    {
        if (_activeHotkeyTextBox is null)
        {
            return;
        }

        _activeHotkeyTextBox.Text = hotkey.ToString();
        _activeHotkeyTextBox.SelectAll();
        SetHotkeyInputHint(
            $"Recorded {_activeHotkeyTextBox.Text}. Click Save to apply it.",
            isSuccess: true
        );
    }

    private void ValidateActiveHotkeyText()
    {
        if (_activeHotkeyTextBox is null || _activeHotkeySaveButton is null)
        {
            return;
        }

        var hotkeyText = _activeHotkeyTextBox.Text.Trim();
        if (string.IsNullOrEmpty(hotkeyText))
        {
            // A blank value intentionally disables the selected binding.
            _activeHotkeySaveButton.Enabled = true;
            SetHotkeyInputHint("Click Save to disable this hotkey, or press Esc to cancel.");
            return;
        }

        if (Hotkey.TryParseFrom(hotkeyText, out _, out var errorMessage))
        {
            _activeHotkeySaveButton.Enabled = true;
            SetHotkeyInputHint(HotkeyInputEditingHint);
            return;
        }

        _activeHotkeySaveButton.Enabled = false;
        SetHotkeyInputHint($"{errorMessage} Use Ctrl/Alt/Shift/Win + A–Z.", isError: true);
    }

    private void SetHotkeyInputHint(string message, bool isError = false, bool isSuccess = false)
    {
        hotkeyInputHintLabel.Text = message;
        hotkeyInputHintLabel.ForeColor = isError
            ? HotkeyInputErrorColor
            : isSuccess
                ? HotkeyInputSuccessColor
                : ModernTheme.TextPrimary;
    }

    private static void SuppressKey(KeyEventArgs e)
    {
        e.Handled = true;
        e.SuppressKeyPress = true;
    }

    private static bool TryNormalizeHotkeyText(string value, out string normalizedValue)
    {
        // Canonical text prevents equivalent spellings from causing a needless rebind.
        var trimmedValue = value.Trim();
        if (string.IsNullOrEmpty(trimmedValue))
        {
            normalizedValue = string.Empty;
            return true;
        }

        if (Hotkey.TryParseFrom(trimmedValue, out var hotkey, out _))
        {
            normalizedValue = hotkey.ToString();
            return true;
        }

        normalizedValue = trimmedValue;
        return false;
    }

    private void RestoreHotkeyEditorAfterSaveFailure(TextBox textBox)
    {
        if (!ReferenceEquals(textBox, _activeHotkeyTextBox))
        {
            return;
        }

        ValidateActiveHotkeyText();
        textBox.Focus();
        textBox.SelectAll();
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
        using var updateCheckCancellationTokenSource = new CancellationTokenSource();
        _updateCheckCancellationTokenSource = updateCheckCancellationTokenSource;

        try
        {
            var result = await _updateChecker.CheckForUpdatesAsync(
                GetCurrentApplicationVersion(),
                updateCheckCancellationTokenSource.Token);
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
        catch (OperationCanceledException) when (updateCheckCancellationTokenSource.IsCancellationRequested)
        {
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
            if (ReferenceEquals(_updateCheckCancellationTokenSource, updateCheckCancellationTokenSource))
            {
                _updateCheckCancellationTokenSource = null;
            }

            if (!_isClosing && !IsDisposed && !Disposing)
            {
                SetUpdateCheckInProgress(false);
            }
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
        // Snapshot the runtime registration so any failed change can be rolled back.
        var previousHotkey = _hotkeyService.GetHotKeyInfoBy(h => h.Name, name);

        try
        {
            ApplyHotkeyChange(name, newHotkeyText);
            await _hotkeyService.SaveHotkeysAsync();
        }
        catch (Exception saveException)
        {
            try
            {
                RestoreHotkeyBinding(name, previousHotkey);
            }
            catch (Exception restoreException)
            {
                throw new AggregateException(
                    $"Failed to save hotkey '{name}' and restore its previous binding.",
                    saveException,
                    restoreException
                );
            }

            throw;
        }
    }

    private void ApplyHotkeyChange(string name, string newHotkeyText)
    {
        // Use live service state so retrying after a persistence failure remains safe.
        var existingHotkey = _hotkeyService.GetHotKeyInfoBy(h => h.Name, name);

        if (string.IsNullOrEmpty(newHotkeyText))
        {
            if (existingHotkey is not null && !_hotkeyService.UnregisterHotkey(name))
            {
                throw new InvalidOperationException($"Failed to unregister hotkey '{name}'.");
            }

            return;
        }

        if (existingHotkey is null)
        {
            _ = _hotkeyService.RegisterHotkey(newHotkeyText, name);
            return;
        }

        if (!string.Equals(existingHotkey.Hotkey.ToString(), newHotkeyText, StringComparison.Ordinal))
        {
            _hotkeyService.ChangeHotkeyBinding(name, newHotkeyText);
        }
    }

    private void RestoreHotkeyBinding(string name, HotkeyInfo? previousHotkey)
    {
        // Restore only the runtime binding; the candidate text stays available for retry.
        var currentHotkey = _hotkeyService.GetHotKeyInfoBy(h => h.Name, name);
        if (previousHotkey is null)
        {
            if (currentHotkey is not null && !_hotkeyService.UnregisterHotkey(name))
            {
                throw new InvalidOperationException($"Failed to remove the unsaved hotkey '{name}'.");
            }

            return;
        }

        var previousHotkeyText = previousHotkey.Hotkey.ToString();
        if (currentHotkey is null)
        {
            _ = _hotkeyService.RegisterHotkey(previousHotkeyText, name, previousHotkey.Id);
            return;
        }

        if (currentHotkey.Hotkey != previousHotkey.Hotkey)
        {
            _hotkeyService.ChangeHotkeyBinding(name, previousHotkeyText);
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

    private void RefreshHotkeyText(string name, TextBox textBox)
    {
        textBox.Text = _hotkeyService.GetHotKeyInfoBy(h => h.Name, name)?.ToString() ?? string.Empty;
    }
}
