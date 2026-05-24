using WallpaperSwitcher.Core.GlobalHotkey;

namespace WallpaperSwitcher.Desktop;

public partial class MainForm
{
    private void HandleHotkeyPressed(object? sender, HotkeyPressedEventArgs e)
    {
        if (TryHandleNextWallpaperHotkey(e.HotkeyInfo))
        {
            return;
        }

        SelectFolderForHotkey(e.HotkeyInfo);
    }

    private bool TryHandleNextWallpaperHotkey(HotkeyInfo hotkeyInfo)
    {
        if (hotkeyInfo.Name != Default.NextWallpaperHotkeyName)
        {
            return false;
        }

        nextWallpaperButton_Click(this, EventArgs.Empty);
        return true;
    }

    private void SelectFolderForHotkey(HotkeyInfo hotkeyInfo)
    {
        var selectedFolder = GetConfiguredFolders().FirstOrDefault(folder => folder == hotkeyInfo.Name);
        if (selectedFolder is not null)
        {
            currentFolderComboBox.SelectedItem = selectedFolder;
        }
    }
}
