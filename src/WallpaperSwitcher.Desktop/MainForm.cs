using System.Collections.Specialized;
using WallpaperSwitcher.Core;

namespace WallpaperSwitcher.Desktop
{
    public partial class MainForm : Form
    {
        private readonly WallpaperManager _wallpaperManager = new();

        public MainForm()
        {
            InitializeComponent();
        }

        private void LoadSettings()
        {
            currentFolderComboBox.Items.Clear();
            removeFolderComboBox.Items.Clear();

            var wallpaperFolders = Properties.Settings.Default.WallpaperFolders ?? [];
            foreach (var folderPath in wallpaperFolders)
            {
                // User might have deleted the folder, so we check if it still exists
                if (!Directory.Exists(folderPath)) continue;
                currentFolderComboBox.Items.Add(folderPath);
                removeFolderComboBox.Items.Add(folderPath);
            }

            var lastSelectedFolder = Properties.Settings.Default.LastSelectedFolder;

            if (string.IsNullOrEmpty(lastSelectedFolder)) return;
            // User might have deleted the last selected folder, so we check if it still exists
            if (!currentFolderComboBox.Items.Contains(lastSelectedFolder)) return;

            currentFolderComboBox.SelectedItem = lastSelectedFolder;
            _wallpaperManager.ChangeWallpaperFolder(lastSelectedFolder);
        }

        private void SaveSettings()
        {
            var wallpaperFolders = new StringCollection();
            foreach (string? item in currentFolderComboBox.Items)
            {
                if (!string.IsNullOrEmpty(item) && Directory.Exists(item))
                {
                    wallpaperFolders.Add(item);
                }
            }

            Properties.Settings.Default.WallpaperFolders = wallpaperFolders;

            if (currentFolderComboBox.SelectedItem != null)
            {
                Properties.Settings.Default.LastSelectedFolder = currentFolderComboBox.SelectedItem.ToString();
            }

            Properties.Settings.Default.Save();
        }

        private static bool ValidateFolder(string folderPath, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                errorMessage = "Please select a folder.";
                return false;
            }

            if (!Directory.Exists(folderPath))
            {
                errorMessage = "The selected folder does not exist.";
                return false;
            }

            // Check if folder contains any image files
            try
            {
                var imageFiles = Directory
                    .GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(file =>
                        WallpaperManager.SupportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                    .ToArray();

                if (imageFiles.Length == 0)
                {
                    errorMessage =
                        "The selected folder does not contain any supported image files (JPG, PNG, BMP, etc.).";
                    return false;
                }
            }
            catch (UnauthorizedAccessException)
            {
                errorMessage = "Access denied to the selected folder.";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error accessing folder: {ex.Message}";
                return false;
            }

            return true;
        }

        private static void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message,
                @"Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private static void ShowErrorMessage(string message)
        {
            MessageBox.Show(message,
                @"Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private static void ShowWarningMessage(string message)
        {
            MessageBox.Show(message,
                @"Warning",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void MainForm_Load(object sender, EventArgs e) => LoadSettings();

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) => SaveSettings();

        private void browseFolderButton_Click(object sender, EventArgs e)
        {
            using var folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = @"Select a folder containing wallpapers";
            folderBrowserDialog.ShowNewFolderButton = false; // User should select existing folders
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                addFolderTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void addFolderButton_Click(object sender, EventArgs e)
        {
            var newFolderPath = addFolderTextBox.Text.Trim();

            // Validate the folder
            if (!ValidateFolder(newFolderPath, out var errorMessage))
            {
                ShowErrorMessage(errorMessage);
                addFolderTextBox.Clear();
                return;
            }

            // Check for duplicates
            if (currentFolderComboBox.Items.Contains(newFolderPath))
            {
                ShowWarningMessage("This folder is already added.");
                addFolderTextBox.Clear();
                return;
            }

            // Add the folder
            currentFolderComboBox.Items.Add(newFolderPath);
            removeFolderComboBox.Items.Add(newFolderPath);

            // Clear the text box and show success message
            addFolderTextBox.Clear();
            // Get image count for user feedback
            var imageCount = Directory
                .GetFiles(newFolderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Count(file =>
                    WallpaperManager.SupportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()));
            ShowSuccessMessage($"Folder added successfully!\n\nPath: {newFolderPath}\nImages found: {imageCount}");
        }

        private void removeFolderButton_Click(object sender, EventArgs e)
        {
            if (removeFolderComboBox.SelectedItem is not string folderToRemove) return;

            // Confirm removal
            var result = MessageBox.Show(
                $"""
                 Are you sure you want to remove this folder from the list?

                 {folderToRemove}
                 """,
                @"Confirm Removal",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            var isCurrentSelected = (currentFolderComboBox.SelectedItem?.ToString() == folderToRemove);

            currentFolderComboBox.Items.Remove(folderToRemove);
            removeFolderComboBox.Items.Remove(folderToRemove);

            if (isCurrentSelected)
            {
                currentFolderComboBox_SelectedIndexChanged(currentFolderComboBox, EventArgs.Empty);
            }

            removeFolderComboBox_SelectedIndexChanged(removeFolderComboBox, EventArgs.Empty);
        }

        private void nextWallpaperButton_Click(object sender, EventArgs e)
        {
            _wallpaperManager.NextWallpaper();
        }

        private void prevWallpaperButton_Click(object sender, EventArgs e)
        {
            _wallpaperManager.PreviousWallpaper();
        }

        private void removeFolderComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            removeFolderButton.Enabled = removeFolderComboBox.SelectedItem != null;
        }

        private void currentFolderComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            prevWallpaperButton.Enabled = currentFolderComboBox.SelectedItem != null;
            nextWallpaperButton.Enabled = currentFolderComboBox.SelectedItem != null;
            _wallpaperManager.ChangeWallpaperFolder(currentFolderComboBox.SelectedItem?.ToString() ?? string.Empty);
        }

        private void addFolderTextBox_TextChanged(object sender, EventArgs e)
        {
            addFolderButton.Enabled = !string.IsNullOrWhiteSpace(addFolderTextBox.Text);
        }
    }
}