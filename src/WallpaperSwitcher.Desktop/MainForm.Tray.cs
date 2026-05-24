namespace WallpaperSwitcher.Desktop;

public partial class MainForm
{
    private const string TraySwitchFolderText = "Switch Folder";
    private const string TrayNextWallpaperText = "Next Wallpaper";
    private const string TraySettingsText = "Settings";
    private const string TrayExitText = "Exit";

    private void InitializeSystemTray()
    {
        var trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add(new ToolStripMenuItem(TraySwitchFolderText));
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add(new ToolStripMenuItem(TrayNextWallpaperText, null, nextWallpaperButton_Click));
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add(new ToolStripMenuItem(TraySettingsText, null, settingsButton_Click));
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add(new ToolStripMenuItem(TrayExitText, null, ExitApplication));

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
        folderMenuItem.DropDownItems.Clear();

        foreach (var folderPath in GetConfiguredFolders())
        {
            folderMenuItem.DropDownItems.Add(CreateFolderTrayMenuItem(folderPath));
        }

        if (folderMenuItem.DropDownItems.Count == 0)
        {
            folderMenuItem.DropDownItems.Add(new ToolStripMenuItem("No folders configured") { Enabled = false });
        }

        GetTrayMenuItem(TrayNextWallpaperText).Enabled = currentFolderComboBox.SelectedItem != null;
    }

    private ToolStripMenuItem CreateFolderTrayMenuItem(string folderPath)
    {
        var menuItem = new ToolStripMenuItem(Path.GetFileName(folderPath))
        {
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
