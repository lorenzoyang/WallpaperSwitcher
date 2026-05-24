using WallpaperSwitcher.Core.GlobalHotkey;
using WallpaperSwitcher.Core.Wallpaper;

namespace WallpaperSwitcher.Desktop;

public partial class MainForm
{
    private async Task LoadInitialSettingsAsync()
    {
        if (!TryBeginInitialSettingsLoad())
        {
            return;
        }

        PopulateComponentsFromInitialSettings();
        await _hotkeyService.LoadHotkeysAsync();
    }

    private void LoadInitialSettings()
    {
        if (!TryBeginInitialSettingsLoad())
        {
            return;
        }

        PopulateComponentsFromInitialSettings();
        _hotkeyService.LoadHotkeys();
    }

    private bool TryBeginInitialSettingsLoad()
    {
        if (HasLoadedInitialSettings)
        {
            return false;
        }

        HasLoadedInitialSettings = true;
        return true;
    }

    /// <summary>
    /// Creates the wallpaper manager implementation for the persisted mode index.
    /// </summary>
    /// <remarks>
    /// Invalid persisted values fall back to native Windows slideshow mode.
    /// </remarks>
    private static WallpaperManager CreateWallpaperManager(int selectedModeIndex)
    {
        return NormalizeModeIndex(selectedModeIndex) switch
        {
            NativeModeIndex => new NativeWallpaperManager(),
            CustomModeIndex => new CustomWallpaperManager(),
            _ => new NativeWallpaperManager()
        };
    }

    private static int NormalizeModeIndex(int selectedModeIndex)
    {
        return selectedModeIndex is NativeModeIndex or CustomModeIndex
            ? selectedModeIndex
            : NativeModeIndex;
    }

    private void PopulateComponentsFromInitialSettings()
    {
        ResetFolderComboBoxes();
        foreach (var folderPath in _appSettings.WallpaperFolders.Where(Directory.Exists))
        {
            AddFolderToComboBoxes(folderPath);
        }

        modeComboBox.SelectedIndexChanged -= modeComboBox_SelectedIndexChanged;
        modeComboBox.SelectedIndex = NormalizeModeIndex(_appSettings.SelectedModeIndex);
        modeComboBox.SelectedIndexChanged += modeComboBox_SelectedIndexChanged;

        SelectFolderIfConfigured(_appSettings.LastSelectedFolder);
    }

    private void SaveSettings()
    {
        _appSettings.WallpaperFolders = GetConfiguredFolders()
            .Where(Directory.Exists)
            .ToList();

        _appSettings.LastSelectedFolder = currentFolderComboBox.SelectedItem?.ToString() ?? string.Empty;
        _appSettings.SelectedModeIndex = NormalizeModeIndex(modeComboBox.SelectedIndex);

        _appSettingsStorage.Save(_appSettings);
    }

    private IEnumerable<string> GetConfiguredFolders()
    {
        return currentFolderComboBox.Items.Cast<string>();
    }

    private void ResetFolderComboBoxes()
    {
        currentFolderComboBox.Items.Clear();
        removeFolderComboBox.Items.Clear();
    }

    private void AddFolderToComboBoxes(string folderPath)
    {
        currentFolderComboBox.Items.Add(folderPath);
        removeFolderComboBox.Items.Add(folderPath);
    }

    private void RemoveFolderFromComboBoxes(string folderPath)
    {
        currentFolderComboBox.Items.Remove(folderPath);
        removeFolderComboBox.Items.Remove(folderPath);
    }

    private void SelectFolderIfConfigured(string folderPath)
    {
        if (!string.IsNullOrEmpty(folderPath) && currentFolderComboBox.Items.Contains(folderPath))
        {
            currentFolderComboBox.SelectedItem = folderPath;
        }
    }

    private async void MainForm_Load(object sender, EventArgs e)
    {
        try
        {
            await LoadInitialSettingsAsync();
        }
        catch (Exception exception)
        {
            FormHelper.ShowErrorMessageWithLink(
                $"An error occurred while loading settings: {exception.Message}\n\n" +
                "The application will now exit.");
            IsExiting = true;
            _trayIcon.Visible = false;
            Application.Exit();
        }
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing && !IsExiting)
        {
            e.Cancel = true;
            MinimizeToTray();
            return;
        }

        SaveSettings();
    }

    private void modeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        FormHelper.ShowSuccessMessage(
            "You have changed the wallpaper mode.\n\n" +
            "The change will take effect after restarting the application.",
            "Restart Required"
        );
        SaveSettings();
    }

    private void settingsButton_Click(object? sender, EventArgs e)
    {
        using var settingsForm = new SettingsForm(
            _hotkeyService,
            GetConfiguredFolders().ToList(),
            _appSettingsStorage,
            _appSettings
        );
        var result = settingsForm.ShowDialog(this);
        switch (result)
        {
            case DialogResult.OK:
                FormHelper.ShowSuccessMessage("Settings saved successfully.");
                break;
        }
    }
}
