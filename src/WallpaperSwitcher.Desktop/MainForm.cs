using System.Collections.Specialized;
using WallpaperSwitcher.Core;
using WallpaperSwitcher.Core.GlobalHotkey;
using WallpaperSwitcher.Core.GlobalHotKey;

namespace WallpaperSwitcher.Desktop;

public partial class MainForm : Form
{
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

    // Allow the form to be visible when SetVisibleCore is called
    // Used to implement minimizing to tray functionality at application startup
    private bool AllowVisible { get; set; } = true; // Important: initially allow visibility

    // Flag to indicate if initial settings have been loaded
    private bool HasLoadedInitialSettings { get; set; }

    // Override SetVisibleCore to control initial visibility without affecting other properties
    protected override void SetVisibleCore(bool value)
    {
        base.SetVisibleCore(AllowVisible && value);
    }

    protected override void WndProc(ref Message m)
    {
        // Handle custom message to show the first instance of the application
        if (m.Msg == FormHelper.WmShowFirstInstanceMessage)
        {
            ShowMainForm();
        }

        base.WndProc(ref m);

        // Handle global hotkey messages
        if (m.Msg == GlobalHotkeyManager.WmHotkeyMessage)
        {
            var id = m.WParam.ToInt32();
            _globalHotkeyManager.ProcessWindowMessage(id);
        }
    }

    public MainForm(bool startMinimized = false)
    {
        InitializeComponent();

        // ********************************
        // System Tray Icon Initialization
        _trayIcon = new NotifyIcon()
        {
            Icon = Icon,
            Visible = true, // Always visible while app is running
            Text = @"Wallpaper Switcher"
        };
        InitializeSystemTray();

        // *********************************************************
        // GlobalHotkeyManager initialization and event subscription
        _globalHotkeyManager = new GlobalHotkeyManager(this.Handle);
        _globalHotkeyManager.HotkeyPressed += (_, e) =>
        {
            if (e.HotkeyInfo.Name == GlobalHotkeyManager.DefaultNextWallpaperHotkeyName)
            {
                nextWallpaperButton_Click(this, EventArgs.Empty);
                return;
            }

            if (currentFolderComboBox.Items
                    .Cast<string>()
                    .FirstOrDefault(folder => folder == e.HotkeyInfo.Name) is { } selectedFolder)
            {
                currentFolderComboBox.SelectedItem = selectedFolder;
            }
            // add return to the last if statement to avoid unnecessary processing
            // add more hotkeys here if needed
        };

        // If starting minimized, prevent initial visibility
        // And load initial settings synchronously
        if (startMinimized)
        {
            AllowVisible = false;
            LoadInitialSettings();
        }
    }

    private async Task LoadInitialSettingsAsync()
    {
        if (HasLoadedInitialSettings) return;
        HasLoadedInitialSettings = true;
        PopulateComponentsFromInitialSettings();
        // ********************************
        // Load hotkeys from user settings
        await _globalHotkeyManager.LoadHotkeysAsync();
    }

    private void LoadInitialSettings()
    {
        if (HasLoadedInitialSettings) return;
        HasLoadedInitialSettings = true;
        PopulateComponentsFromInitialSettings();
        // ********************************
        // Load hotkeys from user settings
        _globalHotkeyManager.LoadHotkeys();
    }

    private void PopulateComponentsFromInitialSettings()
    {
        // ****************************************
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

        // ******************************************
        // Load the selected mode index from settings
        // This loading must be done before loading the last selected folder
        // Selected mode: 0 = Native, 1 = Custom, 0 is the default
        // disable temporarily the event handler to prevent unnecessary message box reminder
        modeComboBox.SelectedIndexChanged -= modeComboBox_SelectedIndexChanged;
        modeComboBox.SelectedIndex = Properties.Settings.Default.SelectedModeIndex;
        modeComboBox.SelectedIndexChanged += modeComboBox_SelectedIndexChanged;

        // *******************************************
        // Load the last selected folder from settings
        var lastSelectedFolder = Properties.Settings.Default.LastSelectedFolder;
        if (string.IsNullOrEmpty(lastSelectedFolder)) return;
        // User might have deleted the last selected folder, so we check if it still exists
        if (!currentFolderComboBox.Items.Contains(lastSelectedFolder)) return;
        currentFolderComboBox.SelectedItem = lastSelectedFolder;
    }

    private void SaveSettings()
    {
        // **************************************
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

        // *****************************************
        // Save the last selected folder to settings
        if (currentFolderComboBox.SelectedItem != null)
        {
            Properties.Settings.Default.LastSelectedFolder = currentFolderComboBox.SelectedItem.ToString();
        }

        // *****************************
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
        "Settings",
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
        // Add settings option
        trayMenu.Items.Add(new ToolStripMenuItem(TrayMenuItemNames[2], null, settingsButton_Click));
        trayMenu.Items.Add(new ToolStripSeparator());
        // Add "Exit" option
        trayMenu.Items.Add(new ToolStripMenuItem(TrayMenuItemNames[3], null, ExitApplication));
        // Left-click to restore the main form
        _trayIcon.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left) ShowMainForm();
        };
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
                   throw new InvalidOperationException($"System tray menu item '{name}' not found.");
        }
    }

    private void ExitApplication(object? sender, EventArgs e)
    {
        IsExiting = true; // Set to true to exit application completely
        _trayIcon.Visible = false;
        Application.Exit();
    }

    private void ShowMainForm()
    {
        AllowVisible = true;
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    private void MinimizeToTray()
    {
        Hide();

        // Show a balloon tip only once
        if (Properties.Settings.Default.HasShownTrayTip) return;
        _trayIcon.BalloonTipTitle = @"Wallpaper Switcher";
        _trayIcon.BalloonTipText =
            @"Application minimized to system tray. Double-click the tray icon to restore.";
        _trayIcon.BalloonTipIcon = ToolTipIcon.Info;
        _trayIcon.ShowBalloonTip(10000);

        Properties.Settings.Default.HasShownTrayTip = true;
        SaveSettings();
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
            await LoadInitialSettingsAsync();
        }
        catch (Exception exception)
        {
            FormHelper.ShowErrorMessageWithLink(
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

        SaveSettings();
    }

    private async void removeFolderButton_Click(object sender, EventArgs e)
    {
        try
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
            await _globalHotkeyManager.SaveHotkeysAsync();

            SaveSettings();
        }
        catch (Exception exception)
        {
            FormHelper.ShowErrorMessageWithLink(
                $"An error occurred while removing the folder: {exception.Message}\n\n" +
                "Please try again.");
        }
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

    private void settingsButton_Click(object? sender, EventArgs e)
    {
        using var settingsForm = new SettingsForm(
            _globalHotkeyManager,
            currentFolderComboBox.Items.Cast<string>().ToList()
        );
        var result = settingsForm.ShowDialog(this);
        switch (result)
        {
            case DialogResult.OK:
                FormHelper.ShowSuccessMessage("Settings saved successfully.");
                break;
            // case DialogResult.Cancel:
            //     FormHelper.ShowWarningMessage(
            //         "Settings window was closed without confirming. Any unsaved changes may have been discarded.");
            //     break;
        }
    }
}