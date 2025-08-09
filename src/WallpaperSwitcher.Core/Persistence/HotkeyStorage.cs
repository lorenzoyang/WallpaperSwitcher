using System.Text.Json;
using WallpaperSwitcher.Core.GlobalHotkey;

namespace WallpaperSwitcher.Core.Persistence;

/// <summary>
/// Handles the persistent storage and retrieval of <see cref="HotkeyInfo"/> objects
/// to and from a JSON file on disk.
/// </summary>
internal sealed class HotkeyStorage
{
    /// <summary>
    /// The default file path for storing hotkey data, located in the user's
    /// local application data folder.
    /// </summary>
    private static readonly string DefaultLocation = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WallpaperSwitcher",
        "hotkeys.json"
    );

    /// <summary>
    /// Gets the file path where hotkey data is stored.
    /// </summary>
    public string Location { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HotkeyStorage"/> class
    /// using the default storage location.
    /// </summary>
    public HotkeyStorage() : this(DefaultLocation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HotkeyStorage"/> class
    /// using the specified storage file path.
    /// </summary>
    /// <param name="location">The file path where hotkey data will be stored.</param>
    public HotkeyStorage(string location)
    {
        Location = location;
    }

    /// <summary>
    /// Asynchronously saves the specified hotkey collection to disk in JSON format.
    /// </summary>
    /// <param name="hotkeys">The array of hotkeys to save.</param>
    public async Task SaveAsync(HotkeyInfo[] hotkeys)
    {
        EnsureDirectoryExists();
        await using var fileStream = File.Create(Location);
        await JsonSerializer.SerializeAsync(fileStream, hotkeys, SourceGenerationContext.Default.HotkeyInfoArray);
    }

    /// <summary>
    /// Saves the specified hotkey collection to disk in JSON format.
    /// </summary>
    /// <param name="hotkeys">The array of hotkeys to save.</param>
    public void Save(HotkeyInfo[] hotkeys)
    {
        EnsureDirectoryExists();
        using var fileStream = File.Create(Location);
        JsonSerializer.Serialize(fileStream, hotkeys, SourceGenerationContext.Default.HotkeyInfoArray);
    }

    /// <summary>
    /// Ensures that the directory containing the storage file exists.
    /// Creates it if it does not exist.
    /// </summary>
    private void EnsureDirectoryExists()
    {
        var directory = Path.GetDirectoryName(Location);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    /// <summary>
    /// Asynchronously loads hotkeys from the storage file.
    /// </summary>
    /// <returns>
    /// An array of loaded <see cref="HotkeyInfo"/> objects,
    /// or an empty array if the file does not exist or contains invalid JSON.
    /// </returns>
    public async Task<HotkeyInfo[]> LoadAsync()
    {
        if (!File.Exists(Location))
        {
            return [];
        }

        try
        {
            await using var fileStream = File.OpenRead(Location);
            var hotkeys =
                await JsonSerializer.DeserializeAsync(fileStream, SourceGenerationContext.Default.HotkeyInfoArray);
            return hotkeys ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    /// <summary>
    /// Loads hotkeys from the storage file.
    /// </summary>
    /// <returns>
    /// An array of loaded <see cref="HotkeyInfo"/> objects,
    /// or an empty array if the file does not exist or contains invalid JSON.
    /// </returns>
    public HotkeyInfo[] Load()
    {
        if (!File.Exists(Location))
        {
            return [];
        }

        try
        {
            using var fileStream = File.OpenRead(Location);
            var hotkeys =
                JsonSerializer.Deserialize(fileStream, SourceGenerationContext.Default.HotkeyInfoArray);
            return hotkeys ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}