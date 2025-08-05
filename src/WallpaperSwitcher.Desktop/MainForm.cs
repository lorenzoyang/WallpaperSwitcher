using System.Collections.Specialized;
using WallpaperSwitcher.Core;
using WallpaperSwitcher.Core.GlobalHotKey;

namespace WallpaperSwitcher.Desktop;

public partial class MainForm : Form
{
    protected override void WndProc(ref Message m)
    {
        // Handle custom message to show the first instance of the application
        if (m.Msg == FormHelper.WmShowFirstInstanceMessage)
        {
            ShowMainForm(null, EventArgs.Empty);
        }

        base.WndProc(ref m);

        // Handle global hotkey messages
        if (m.Msg == GlobalHotkeyManager.WmHotkeyMessage)
        {
            var id = m.WParam.ToInt32();
            _globalHotkeyManager.ProcessWindowMessage(id);
        }
    }

    private readonly GlobalHotkeyManager _globalHotkeyManager;

    // default wallpaper manager implementation
    private readonly WallpaperManager _wallpaperManager = Properties.Settings.Default.SelectedModeIndex switch
    {
        0 => new NativeWallpaperManager(), // Native Windows implementation
        1 => new CustomWallpaperManager(), // Custom implementation (if any)
        _ => throw new NotSupportedException("Selected mode is not supported.")
    };

    private readonly ToolTip _toolTip = new()
    {
        AutoPopDelay = 10000, // Show for 10 seconds
        InitialDelay = 500, // Wait 0.5 seconds before showing
        ReshowDelay = 100, // Quick reshow
        ShowAlways = true
    };

    private readonly NotifyIcon _trayIcon;

    // When the user closes the form, if this is true the program will exit completely.
    // If false, it will minimize to the system tray.
    private bool IsExiting { get; set; }

    public MainForm()
    {
        InitializeComponent();

        // System Tray Icon Initialization
        _trayIcon = new NotifyIcon()
        {
            Icon = Icon,
            Visible = true, // Always visible while app is running
            Text = @"Wallpaper Switcher"
        };
        InitializeSystemTray();

        // GlobalHotkeyManager initialization and event subscription
        _globalHotkeyManager = new GlobalHotkeyManager(this.Handle);
        _globalHotkeyManager.HotkeyPressed += (_, e) =>
        {
            if (e.HotkeyInfo.Name == GlobalHotkeyManager.DefaultNextWallpaperHotkeyName)
            {
                nextWallpaperButton_Click(this, EventArgs.Empty);
            }

            // add more hotkeys here if needed
        };
    }

    private async Task LoadSettings()
    {
        // Load the wallpaper folders from settings
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

        // Load the selected mode index from settings
        // Selected mode: 0 = Native, 1 = Custom, 0 is the default
        // disable temporarily the event handler to prevent unnecessary message box reminder
        modeComboBox.SelectedIndexChanged -= modeComboBox_SelectedIndexChanged;
        modeComboBox.SelectedIndex = Properties.Settings.Default.SelectedModeIndex;
        modeComboBox.SelectedIndexChanged += modeComboBox_SelectedIndexChanged;

        // Load the last selected folder from settings
        var lastSelectedFolder = Properties.Settings.Default.LastSelectedFolder;
        if (string.IsNullOrEmpty(lastSelectedFolder)) return;
        // User might have deleted the last selected folder, so we check if it still exists
        if (!currentFolderComboBox.Items.Contains(lastSelectedFolder)) return;
        currentFolderComboBox.SelectedItem = lastSelectedFolder;

        // Load hotkeys from user settings
        await _globalHotkeyManager.LoadingHotkeysAsync();
    }

    private void SaveSettings()
    {
        // Save the wallpaper folders to settings
        var wallpaperFolders = new StringCollection();
        foreach (string? item in currentFolderComboBox.Items)
        {
            if (!string.IsNullOrEmpty(item) && Directory.Exists(item))
            {
                wallpaperFolders.Add(item);
            }
        }

        Properties.Settings.Default.WallpaperFolders = wallpaperFolders;

        // Save the last selected folder to settings
        if (currentFolderComboBox.SelectedItem != null)
        {
            Properties.Settings.Default.LastSelectedFolder = currentFolderComboBox.SelectedItem.ToString();
        }

        // Save the selected mode index
        // If an unsupported mode is selected, default to Native (0)
        Properties.Settings.Default.SelectedModeIndex =
            modeComboBox.SelectedIndex is 0 or 1 ? modeComboBox.SelectedIndex : 0;

        Properties.Settings.Default.Save();
    }

    // ****************************
    // System Tray Implementation
    // ****************************
    private static readonly string[] TrayMenuItemNames =
    [
        "Switch Folder",
        "Next Wallpaper",
        "Exit"
    ];

    private void InitializeSystemTray()
    {
        var trayMenu = new ContextMenuStrip();
        // Add folder selection submenu
        trayMenu.Items.Add(new ToolStripMenuItem(TrayMenuItemNames[0]));
        trayMenu.Items.Add(new ToolStripSeparator());
        // Add wallpaper controls
        var nextWallpaperItem = new ToolStripMenuItem(TrayMenuItemNames[1], null, nextWallpaperButton_Click);
        trayMenu.Items.Add(nextWallpaperItem);
        trayMenu.Items.Add(new ToolStripSeparator());
        // Add "Exit" option
        trayMenu.Items.Add(new ToolStripMenuItem(TrayMenuItemNames[2], null, ExitApplication));
        // Double-click to restore window
        _trayIcon.DoubleClick += ShowMainForm;
        trayMenu.Opening += (_, _) => UpdateTrayMenu();

        _trayIcon.ContextMenuStrip = trayMenu;
    }

    private void UpdateTrayMenu()
    {
        if (GetTrayMenuItemByName(TrayMenuItemNames[0]) is not ToolStripMenuItem folderMenuItem) return;
        folderMenuItem.DropDownItems.Clear();
        foreach (string folderPath in currentFolderComboBox.Items)
        {
            var menuItem = new ToolStripMenuItem(Path.GetFileName(folderPath)) // Use folder name as display text
            {
                Tag = folderPath, // Store the full path in the Tag property
                Checked = folderPath == currentFolderComboBox.SelectedItem?.ToString(),
                ToolTipText = folderPath
            };
            menuItem.Click += (s, _) =>
            {
                if (s is ToolStripMenuItem { Tag: string selectedFolderPath })
                {
                    currentFolderComboBox.SelectedItem = selectedFolderPath;
                }
            };

            folderMenuItem.DropDownItems.Add(menuItem);
        }

        if (folderMenuItem.DropDownItems.Count == 0)
        {
            folderMenuItem.DropDownItems.Add(new ToolStripMenuItem("No folders configured") { Enabled = false });
        }

        // Disable wallpaper controls if no folder is selected
        var nextWallpaperMenuItem = GetTrayMenuItemByName(TrayMenuItemNames[1]);
        var hasSelectedFolder = currentFolderComboBox.SelectedItem != null;
        nextWallpaperMenuItem.Enabled = hasSelectedFolder;

        return;

        ToolStripItem GetTrayMenuItemByName(string name)
        {
            return _trayIcon.ContextMenuStrip?.Items
                       .Cast<ToolStripItem>()
                       .FirstOrDefault(item => item.Text == name) ??
                   throw new InvalidOperationException($"Menu item '{name}' not found.");
        }
    }

    private void ExitApplication(object? sender, EventArgs e)
    {
        IsExiting = true; // Set to true to exit application completely
        _trayIcon.Visible = false;
        Application.Exit();
    }

    private void ShowMainForm(object? sender, EventArgs e)
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    private bool HasShownTrayTip { get; set; }

    private void MinimizeToTray()
    {
        Hide();

        // Show balloon tip on first minimize
        if (HasShownTrayTip) return;
        _trayIcon.BalloonTipTitle = @"Wallpaper Switcher";
        _trayIcon.BalloonTipText =
            @"Application minimized to system tray. Double-click the tray icon to restore.";
        _trayIcon.BalloonTipIcon = ToolTipIcon.Info;
        _trayIcon.ShowBalloonTip(1000);

        HasShownTrayTip = true;
    }

    // **********************************
    // End of System Tray Implementation
    // **********************************

    // *********************************
    // Event handlers for Form events  *
    // *********************************

    private async void MainForm_Load(object sender, EventArgs e)
    {
        try
        {
            await LoadSettings();
        }
        catch (Exception exception)
        {
            FormHelper.ShowErrorMessage(
                $"An error occurred while loading settings: {exception.Message}\n\n" +
                "The application will now exit.");
            IsExiting = true; // Set to true to exit application completely
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

    private const int MaxFolderNumber = 5;

    private void addFolderButton_Click(object sender, EventArgs e)
    {
        if (currentFolderComboBox.Items.Count >= MaxFolderNumber)
        {
            FormHelper.ShowWarningMessage("Max folders reached (10). Cannot add more.");
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

        // Check for duplicates
        if (currentFolderComboBox.Items.Contains(newFolderPath))
        {
            FormHelper.ShowWarningMessage("This folder is already added.");
            addFolderTextBox.Clear();
            return;
        }

        // Add the folder
        currentFolderComboBox.Items.Add(newFolderPath);
        removeFolderComboBox.Items.Add(newFolderPath);

        // Clear the text box and show success message
        addFolderTextBox.Clear();
        // Get image count for user feedback
        var imageCount = WallpaperHelper.GetImageCount(newFolderPath);
        FormHelper.ShowSuccessMessage(
            $"Folder added successfully!\n\nPath: {newFolderPath}\nImages found: {imageCount}");
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
            _wallpaperManager.SetWallpaper(WallpaperManager.DefaultWallpaper);
            currentFolderComboBox_SelectedIndexChanged(currentFolderComboBox, EventArgs.Empty);
        }

        removeFolderComboBox_SelectedIndexChanged(removeFolderComboBox, EventArgs.Empty);

        _ = _globalHotkeyManager.UnregisterHotkey(folderToRemove);
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
        var currentFolderPath = currentFolderComboBox.SelectedItem?.ToString() ?? string.Empty;
        _wallpaperManager.SetSlideShow(currentFolderPath);
        SaveSettings(); // Save user selection promptly
    }

    private void addFolderTextBox_TextChanged(object sender, EventArgs e)
    {
        addFolderButton.Enabled = !string.IsNullOrWhiteSpace(addFolderTextBox.Text);
        addFolderTextBox.SelectionStart = addFolderTextBox.Text.Length; // Move cursor to the end
    }

    private void currentFolderComboBox_MouseEnter(object sender, EventArgs e)
    {
        var comboBox = sender as ComboBox;
        FormHelper.ShowFolderToolTipForComboBox(_toolTip, comboBox);
    }

    private void removeFolderComboBox_MouseEnter(object sender, EventArgs e)
    {
        var comboBox = sender as ComboBox;
        FormHelper.ShowFolderToolTipForComboBox(_toolTip, comboBox);
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

    private void settingsButton_Click(object sender, EventArgs e)
    {
        using var settingsForm =
            new SettingsForm(_globalHotkeyManager, currentFolderComboBox.Items.Cast<string>().ToList());
        if (settingsForm.ShowDialog(this) == DialogResult.OK)
        {
            FormHelper.ShowSuccessMessage("Settings updated successfully!");
        }
    }
}