using WallpaperSwitcher.Core.GlobalHotkey;
using WallpaperSwitcher.Core.Wallpaper;

namespace WallpaperSwitcher.Desktop;

public partial class MainForm
{
    private const int MaxFolderNumber = 5;
    private string _activeFolder = string.Empty;

    private void browseFolderButton_Click(object sender, EventArgs e)
    {
        using var folderBrowserDialog = new FolderBrowserDialog
        {
            Description = @"Select a folder containing wallpapers",
            ShowNewFolderButton = false
        };

        if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
        {
            addFolderTextBox.Text = folderBrowserDialog.SelectedPath;
        }
    }

    private void addFolderButton_Click(object sender, EventArgs e)
    {
        if (_appSettings.WallpaperFolders.Count >= MaxFolderNumber)
        {
            FormHelper.ShowWarningMessage($"Max folders reached ({MaxFolderNumber}). Cannot add more.");
            addFolderTextBox.Clear();
            return;
        }

        var newFolderPath = addFolderTextBox.Text.Trim();

        if (!WallpaperHelper.IsValidWallpaperFolder(newFolderPath, out var errorMessage))
        {
            FormHelper.ShowErrorMessage(errorMessage);
            addFolderTextBox.Clear();
            return;
        }

        if (_appSettings.WallpaperFolders.Contains(newFolderPath))
        {
            FormHelper.ShowWarningMessage("This folder is already added.");
            addFolderTextBox.Clear();
            return;
        }

        _appSettings.WallpaperFolders.Add(newFolderPath);
        AddFolderToComboBoxes(newFolderPath);
        addFolderTextBox.Clear();

        FormHelper.ShowSuccessMessage(
            $"Folder added successfully!\n\nPath: {newFolderPath}\nImages found: {WallpaperHelper.GetImageCount(newFolderPath)}");

        SaveSettings();
    }

    private async void removeFolderButton_Click(object sender, EventArgs e)
    {
        try
        {
            if (removeFolderComboBox.SelectedItem is not string folderToRemove)
            {
                return;
            }

            if (!ConfirmFolderRemoval(folderToRemove))
            {
                return;
            }

            await RemoveFolderAsync(folderToRemove);
        }
        catch (Exception exception)
        {
            FormHelper.ShowErrorMessageWithLink(
                $"An error occurred while removing the folder: {exception.Message}\n\n" +
                "Please try again.");
        }
    }

    private static bool ConfirmFolderRemoval(string folderToRemove)
    {
        var result = MessageBox.Show(
            $"""
             Are you sure you want to remove this folder from the list?

             {folderToRemove}
             """,
            @"Confirm Removal",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        return result == DialogResult.Yes;
    }

    internal async Task RemoveFolderAsync(string folder)
    {
        if (_activeFolder == folder)
        {
            currentFolderComboBox.SelectedIndex = -1;
            _wallpaperManager.SetWallpaper(WallpaperManager.DefaultWallpaper);
        }

        if (_appSettings.LastSelectedFolder == folder)
        {
            _appSettings.LastSelectedFolder = string.Empty;
        }

        _appSettings.WallpaperFolders.Remove(folder);
        RemoveFolderFromComboBoxes(folder);
        _ = _hotkeyService.UnregisterHotkey(folder);
        await _hotkeyService.SaveHotkeysAsync();
        SaveSettings();
    }

    private void nextWallpaperButton_Click(object? sender, EventArgs e)
    {
        if (_activeFolder.Length > 0)
        {
            _wallpaperManager.AdvanceForwardSlideshow();
        }
    }

    private void removeFolderComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        removeFolderButton.Enabled = removeFolderComboBox.SelectedItem != null;
    }

    private void currentFolderComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        var requested = currentFolderComboBox.SelectedItem as string ?? string.Empty;
        if (requested == _activeFolder) return;

        if (_wallpaperManager.TrySetSlideShow(requested, out var errorMessage))
        {
            _activeFolder = requested;
            _appSettings.LastSelectedFolder = requested;
            nextWallpaperButton.Enabled = requested.Length > 0;
            return;
        }

        // Restoring the selection raises this event again with the already active folder.
        currentFolderComboBox.SelectedIndex = currentFolderComboBox.Items.IndexOf(_activeFolder);
        var message = $"{requested}\n\n{errorMessage}";
        if (Visible)
        {
            FormHelper.ShowWarningMessage(message, "Unable to switch folders");
        }
        else
        {
            _trayIcon.ShowBalloonTip(5000, "Unable to switch folders", message, ToolTipIcon.Warning);
        }
    }

    private void addFolderTextBox_TextChanged(object sender, EventArgs e)
    {
        addFolderButton.Enabled = !string.IsNullOrWhiteSpace(addFolderTextBox.Text);
        addFolderTextBox.SelectionStart = addFolderTextBox.Text.Length;
    }

    private void currentFolderComboBox_MouseEnter(object sender, EventArgs e)
    {
        FormHelper.ShowFolderToolTipForComboBox(_toolTip, sender as ComboBox);
    }

    private void removeFolderComboBox_MouseEnter(object sender, EventArgs e)
    {
        FormHelper.ShowFolderToolTipForComboBox(_toolTip, sender as ComboBox);
    }
}
