namespace WallpaperSwitcher.Desktop;

public partial class MainForm
{
    private const string TraySwitchFolderText = "Switch Folder";
    private const string TrayNextWallpaperText = "Next Wallpaper";
    private const string TraySettingsText = "Settings";
    private const string TrayExitText = "Exit";

    private void InitializeSystemTray()
    {
        components ??= new System.ComponentModel.Container();
        var trayMenu = new ModernContextMenuStrip(components);
        var folderMenuItem = new ModernMenuItem(TraySwitchFolderText, ModernMenuIcon.Folder)
        {
            DropDown = new ModernContextMenuStrip(components)
        };
        trayMenu.Items.Add(folderMenuItem);
        trayMenu.Items.Add(new ModernMenuItem(TrayNextWallpaperText, ModernMenuIcon.NextWallpaper, nextWallpaperButton_Click)
        {
            ForeColor = ModernTheme.PrimaryAccent
        });
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add(new ModernMenuItem(TraySettingsText, ModernMenuIcon.Settings, settingsButton_Click));
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add(new ModernMenuItem(TrayExitText, ModernMenuIcon.Exit, ExitApplication)
        {
            ForeColor = ModernTheme.DangerAccent
        });

        _trayIcon.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowMainForm();
            }
        };
        trayMenu.Opening += (_, _) => UpdateTrayMenu();

        _trayIcon.ContextMenuStrip = trayMenu;
    }

    private void UpdateTrayMenu()
    {
        var folderMenuItem = GetTrayMenuItem<ToolStripMenuItem>(TraySwitchFolderText);
        var folderMenu = folderMenuItem.DropDown;
        folderMenu.SuspendLayout();
        try
        {
            while (folderMenu.Items.Count > 0)
            {
                folderMenu.Items[0].Dispose();
            }

            foreach (var folderPath in GetConfiguredFolders())
            {
                folderMenu.Items.Add(CreateFolderTrayMenuItem(folderPath));
            }

            if (folderMenu.Items.Count == 0)
            {
                folderMenu.Items.Add(new ModernMenuItem("No folders configured") { Enabled = false });
            }
        }
        finally
        {
            folderMenu.ResumeLayout(true);
        }

        GetTrayMenuItem(TrayNextWallpaperText).Enabled = currentFolderComboBox.SelectedItem != null;
    }

    private ToolStripMenuItem CreateFolderTrayMenuItem(string folderPath)
    {
        var trimmedPath = Path.TrimEndingDirectorySeparator(folderPath);
        var folderName = Path.GetFileName(trimmedPath);
        if (string.IsNullOrEmpty(folderName))
        {
            folderName = trimmedPath;
        }

        // Escape menu mnemonics without changing the path used by the click handler.
        var menuItem = new ModernMenuItem(folderName.Replace("&", "&&"))
        {
            AccessibleName = folderName,
            Tag = folderPath,
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

        return menuItem;
    }

    private ToolStripItem GetTrayMenuItem(string text)
    {
        return _trayIcon.ContextMenuStrip?.Items
                   .Cast<ToolStripItem>()
                   .FirstOrDefault(item => item.Text == text) ??
               throw new InvalidOperationException($"System tray menu item '{text}' not found.");
    }

    private T GetTrayMenuItem<T>(string text) where T : ToolStripItem
    {
        return GetTrayMenuItem(text) as T ??
               throw new InvalidOperationException($"System tray menu item '{text}' has an unexpected type.");
    }

    private void ExitApplication(object? sender, EventArgs e)
    {
        IsExiting = true;
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

        if (_appSettings.HasShownTrayTip)
        {
            return;
        }

        _trayIcon.BalloonTipTitle = ApplicationTitle;
        _trayIcon.BalloonTipText = @"Application minimized to system tray. Click the tray icon to restore.";
        _trayIcon.BalloonTipIcon = ToolTipIcon.Info;
        _trayIcon.ShowBalloonTip(10000);

        _appSettings.HasShownTrayTip = true;
        SaveSettings();
    }
}
