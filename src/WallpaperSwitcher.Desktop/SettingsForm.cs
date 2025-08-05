using WallpaperSwitcher.Core.GlobalHotKey;
using static WallpaperSwitcher.Core.GlobalHotKey.GlobalHotkeyManager;

namespace WallpaperSwitcher.Desktop;

public partial class SettingsForm : Form
{
    private readonly GlobalHotkeyManager _globalHotkeyManager;

    // Dictionary to hold folder hotkeys, where the key is the folder path and the value is the HotkeyInfo
    private readonly Dictionary<string, HotkeyInfo?> _folderHotkeys;

    public SettingsForm(GlobalHotkeyManager globalHotkeyManager, List<string> folders)
    {
        InitializeComponent();

        // GlobalHotkeyManager passed from the main form that is already initialized
        _globalHotkeyManager = globalHotkeyManager;
        // Initialize the folder hotkeys dictionary with the provided folders
        _folderHotkeys = folders.ToDictionary(folder => folder, HotkeyInfo? (_) => null);

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
        // Clean the original value if we are exiting edit mode
        OriginalValue = isEditing ? OriginalValue : string.Empty;
    }

    private void SetFolderHkEditMode(bool isEditing)
    {
        folderHkTextBox.ReadOnly = !isEditing;
        // Assuming that the user has made modifications
        folderHkSaveButton.Enabled = isEditing;
        // Prevent modifying the next wallpaper hotkey while editing the folder hotkey
        nextWallpaperHkModifyButton.Enabled = !isEditing;
        // Until you press Save, the button is temporarily disabled.
        folderHkModifyButton.Enabled = !isEditing;
        // Clean the original value if we are exiting edit mode
        OriginalValue = isEditing ? OriginalValue : string.Empty;
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
            else if (string.IsNullOrEmpty(OriginalValue))
            {
                // this is equivalent to registering a new hotkey
                _globalHotkeyManager.RegisterHotkey(newHotkeyText, DefaultNextWallpaperHotkeyName);
            }
            else
            {
                // this is equivalent to changing the hotkey binding
                _globalHotkeyManager.ChangeHotkeyBinding(DefaultNextWallpaperHotkeyName, newHotkeyText);
            }

            await _globalHotkeyManager.SaveHotkeysAsync();

            SetNextWallpaperHkEditMode(false);
        }
        catch (Exception exception)
        {
            SetNextWallpaperHkEditMode(false);
            nextWallpaperHkTextBox.Text = string.Empty;
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
        // Di default no item is selected, so when the form loads, the button is disabled.
        // If the user selects an item, the button is enabled.
        if (!folderHkModifyButton.Enabled) folderHkModifyButton.Enabled = true;

        if (folderHkComboBox is null || folderHkComboBox.Items.Count <= 0) return;
        // Update the TextBox to display the corresponding hotkey (if defined).
        var selectedFolder = folderHkComboBox.SelectedItem!.ToString()!;
        if (_folderHotkeys.TryGetValue(selectedFolder, out var hotkeyInfo))
        {
            folderHkTextBox.Text = hotkeyInfo?.ToString() ?? string.Empty;
        }
    }

    private void folderHkModifyButton_Click(object sender, EventArgs e)
    {
        OriginalValue = folderHkTextBox.Text;
        folderHkTextBox.Focus();
        SetFolderHkEditMode(true);
    }

    private async void folderHkSaveButton_Click(object sender, EventArgs e)
    {
        try
        {
            var newHotkeyText = folderHkTextBox.Text.Trim();
            if (newHotkeyText == OriginalValue)
            {
                SetFolderHkEditMode(false);
                return;
            }

            var selectedFolder = folderHkComboBox.SelectedItem!.ToString()!;
            if (string.IsNullOrEmpty(newHotkeyText))
            {
                // this is equivalent to unregistering the hotkey
                _globalHotkeyManager.UnregisterHotkey(selectedFolder);
            }
            else if (string.IsNullOrEmpty(OriginalValue))
            {
                // this is equivalent to registering a new hotkey
                _globalHotkeyManager.RegisterHotkey(newHotkeyText, selectedFolder);
            }
            else
            {
                // this is equivalent to changing the hotkey binding
                _globalHotkeyManager.ChangeHotkeyBinding(selectedFolder, newHotkeyText);
            }

            await _globalHotkeyManager.SaveHotkeysAsync();

            SetFolderHkEditMode(false);
        }
        catch (Exception exception)
        {
            SetFolderHkEditMode(false);
            folderHkTextBox.Text = string.Empty;
            FormHelper.ShowErrorMessage(
                $"Failed to save the hotkey for the selected folder: {exception.Message}, please try again.",
                "Error Saving Hotkey"
            );
        }
    }
    // End of Event handlers for Folder Hotkeys
}