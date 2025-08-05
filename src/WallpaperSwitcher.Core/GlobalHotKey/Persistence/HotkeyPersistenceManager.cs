using System.Text.Json;

namespace WallpaperSwitcher.Core.GlobalHotKey.Persistence;

/// <summary>
/// Manages the persistence of global hotkeys by saving and loading them from a JSON file.
/// </summary>
internal class HotkeyPersistenceManager
{
    /// <summary>
    /// The default file path where hotkey data is stored.
    /// </summary>
    private static readonly string DefaultLocation = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WallpaperSwitcher",
        "hotkeys.json"
    );

    /// <summary>
    /// Gets the file path where hotkeys are stored.
    /// </summary>
    public string Location { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HotkeyPersistenceManager"/> class
    /// using the default storage location.
    /// </summary>
    public HotkeyPersistenceManager()
        : this(DefaultLocation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HotkeyPersistenceManager"/> class
    /// using a specified storage location.
    /// </summary>
    /// <param name="location">The path to the JSON file used for storing hotkeys.</param>
    private HotkeyPersistenceManager(string location)
    {
        Location = location;
    }

    /// <summary>
    /// Saves an array of <see cref="HotkeyInfo"/> instances to the JSON file asynchronously.
    /// </summary>
    /// <param name="hotkeys">The array of hotkey information to save.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task SaveHotkeysAsync(HotkeyInfo[] hotkeys)
    {
        var directory = Path.GetDirectoryName(Location);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fileStream = File.Create(Location);
        await JsonSerializer.SerializeAsync(fileStream, hotkeys,
            SourceGenerationContext.Default.HotkeyInfoArray);
    }

    /// <summary>
    /// Loads an array of <see cref="HotkeyInfo"/> instances from the JSON file asynchronously.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous load operation. 
    /// The task result contains the array of loaded hotkey information. 
    /// Returns an empty array if the file does not exist or is invalid.
    /// </returns>
    public async Task<HotkeyInfo[]> LoadHotkeysAsync()
    {
        if (!File.Exists(Location))
        {
            return [];
        }

        try
        {
            await using var fileStream = File.OpenRead(Location);
            var hotkeys =
                await JsonSerializer.DeserializeAsync(fileStream,
                    SourceGenerationContext.Default.HotkeyInfoArray);
            return hotkeys ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}