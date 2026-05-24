using WallpaperSwitcher.Core.GlobalHotkey;
using WallpaperSwitcher.Core.Wallpaper;

namespace WallpaperSwitcher.Desktop;

public partial class MainForm
{
    private const int MaxFolderNumber = 5;

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
        if (currentFolderComboBox.Items.Count >= MaxFolderNumber)
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

        if (currentFolderComboBox.Items.Contains(newFolderPath))
        {
            FormHelper.ShowWarningMessage("This folder is already added.");
            addFolderTextBox.Clear();
            return;
        }

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

            var wasCurrentSelection = currentFolderComboBox.SelectedItem?.ToString() == folderToRemove;

            RemoveFolderFromComboBoxes(folderToRemove);

            if (wasCurrentSelection)
            {
                _wallpaperManager.SetWallpaper(WallpaperManager.DefaultWallpaper);
                currentFolderComboBox_SelectedIndexChanged(currentFolderComboBox, EventArgs.Empty);
            }

            removeFolderComboBox_SelectedIndexChanged(removeFolderComboBox, EventArgs.Empty);

            _ = _hotkeyService.UnregisterHotkey(folderToRemove);
            await _hotkeyService.SaveHotkeysAsync();

            SaveSettings();
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

    private void nextWallpaperButton_Click(object? sender, EventArgs e)
    {
        _wallpaperManager.AdvanceForwardSlideshow();
    }

    private void removeFolderComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        removeFolderButton.Enabled = removeFolderComboBox.SelectedItem != null;
    }

    private void currentFolderComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        nextWallpaperButton.Enabled = currentFolderComboBox.SelectedItem != null;
        _wallpaperManager.SetSlideShow(currentFolderComboBox.SelectedItem?.ToString() ?? string.Empty);
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
