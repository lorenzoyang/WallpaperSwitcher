using WallpaperSwitcher.Core.GlobalHotKey;
using static WallpaperSwitcher.Core.GlobalHotKey.GlobalHotkeyManager;

namespace WallpaperSwitcher.Desktop;

public partial class SettingsForm : Form
{
    private readonly GlobalHotkeyManager _globalHotkeyManager;

    // Dictionary to hold folder hotkeys, where the key is the folder path and the value is the HotkeyInfo
    private Dictionary<string, HotkeyInfo?> _folderHotkeys;

    public SettingsForm(GlobalHotkeyManager globalHotkeyManager, List<string> folders)
    {
        InitializeComponent();

        // GlobalHotkeyManager passed from the main form that is already initialized
        _globalHotkeyManager = globalHotkeyManager;
        // Initialize the folder hotkeys dictionary with the provided folders
        _folderHotkeys = folders.ToDictionary(folder => folder, HotkeyInfo? (folder) => null);

        // Display Next Wallpaper Hotkey
        nextWallpaperHkTextBox.Text =
            _globalHotkeyManager.GetHotKeyInfo(DefaultNextWallpaperHotkeyName)?.ToString() ??
            string.Empty;

        var hotkeyInfosWithoutNextWallpaper = _globalHotkeyManager
            .GetRegisteredHotkeys()
            .Where(hotkeyInfo => hotkeyInfo.Name != DefaultNextWallpaperHotkeyName)
            .ToList();
        // Populate the folder hotkeys dictionary with existing hotkeys, excluding the next wallpaper hotkey
        foreach (var hotkeyInfo in hotkeyInfosWithoutNextWallpaper.Where(hotkeyInfo =>
                     _folderHotkeys.ContainsKey(hotkeyInfo.Name)))
        {
            _folderHotkeys[hotkeyInfo.Name] = hotkeyInfo;
        }

        // Display Folder Hotkeys in the ComboBox
        foreach (var folder in _folderHotkeys.Keys)
        {
            folderHkComboBox.Items.Add(folder);
        }

        folderHkComboBox.SelectedIndex = 0;
    }

    private void SetNextWallpaperHkEditMode(bool isEditing)
    {
        nextWallpaperHkTextBox.ReadOnly = !isEditing;
        // Assuming that the user has made modifications
        nextWallpaperHkSaveButton.Enabled = isEditing;
        // Prevent modifying the folder hotkey while editing the next wallpaper hotkey
        folderHkModifyButton.Enabled = !isEditing;
        // Until you press Save, the button is temporarily disabled.
        nextWallpaperHkModifyButton.Enabled = !isEditing;
    }

    // *********************************
    // Event handlers for Form events  *
    // *********************************
    private string OriginalValue { get; set; } = string.Empty;

    private void SettingsForm_Load(object sender, EventArgs e)
    {
        // To prevent any control from being focused when the form loads
        ActiveControl = nextWallpaperHkLabel;
    }

    //
    // Event handlers for Next Wallpaper Hotkey 
    //
    private void nextWallpaperHkModifyButton_Click(object sender, EventArgs e)
    {
        OriginalValue = nextWallpaperHkTextBox.Text;
        nextWallpaperHkTextBox.Focus();
        SetNextWallpaperHkEditMode(true);
    }

    private async void nextWallpaperHkSaveButton_Click(object sender, EventArgs e)
    {
        try
        {
            var newHotkeyText = nextWallpaperHkTextBox.Text.Trim();
            if (newHotkeyText == OriginalValue)
            {
                SetNextWallpaperHkEditMode(false);
                return;
            }

            if (string.IsNullOrEmpty(newHotkeyText))
            {
                // this is equivalent to unregistering the hotkey
                _globalHotkeyManager.UnregisterHotkey(DefaultNextWallpaperHotkeyName);
            }
            else
            {
                _globalHotkeyManager.ChangeHotkeyBinding(DefaultNextWallpaperHotkeyName, newHotkeyText);
            }

            await _globalHotkeyManager.SaveHotkeysAsync();

            SetNextWallpaperHkEditMode(false);
        }
        catch (Exception exception)
        {
            FormHelper.ShowErrorMessage(
                $"Failed to save the hotkey for '{DefaultNextWallpaperHotkeyName}': {exception.Message}, please try again.",
                "Error Saving Hotkey"
            );
        }
    }
    // End of Event handlers for Next Wallpaper Hotkey

    //
    // Event handlers for Folder Hotkeys
    //
    private void folderHkComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Update the TextBox to display the corresponding hotkey (if defined).
        var selectedFolder = folderHkComboBox.SelectedItem?.ToString();
        if (selectedFolder != null && _folderHotkeys.TryGetValue(selectedFolder, out var hotkeyInfo))
        {
            folderHkTextBox.Text = hotkeyInfo?.ToString() ?? string.Empty;
        }
        else
        {
            folderHkTextBox.Text = string.Empty;
            // TODO
        }
    }

    private void folderHkModifyButton_Click(object sender, EventArgs e)
    {
    }

    private void folderHkSaveButton_Click(object sender, EventArgs e)
    {
    }
    // End of Event handlers for Folder Hotkeys
}