using System.Text.Json;
using WallpaperSwitcher.Core.GlobalHotkey;

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
    /// Asynchronously saves the specified array of hotkeys to the JSON file.
    /// </summary>
    /// <param name="hotkeys">An array of <see cref="HotkeyInfo"/> objects to be saved.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    public async Task SaveHotkeysAsync(HotkeyInfo[] hotkeys)
    {
        var directory = Path.GetDirectoryName(Location);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fileStream = File.Create(Location);
        await JsonSerializer.SerializeAsync(fileStream, hotkeys, SourceGenerationContext.Default.HotkeyInfoArray);
    }

    /// <summary>
    /// Synchronously saves the specified array of hotkeys to the JSON file.
    /// </summary>
    /// <param name="hotkeys">An array of <see cref="HotkeyInfo"/> objects to be saved.</param>
    public void SaveHotkeys(HotkeyInfo[] hotkeys)
    {
        var directory = Path.GetDirectoryName(Location);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var fileStream = File.Create(Location);
        JsonSerializer.Serialize(fileStream, hotkeys, SourceGenerationContext.Default.HotkeyInfoArray);
    }

    /// <summary>
    /// Asynchronously loads hotkeys from the JSON file.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous load operation. The task result contains
    /// an array of <see cref="HotkeyInfo"/> objects loaded from the file, or an empty
    /// array if the file does not exist or is invalid.
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

    /// <summary>
    /// Synchronously loads hotkeys from the JSON file.
    /// </summary>
    /// <returns>
    /// An array of <see cref="HotkeyInfo"/> objects loaded from the file, or an empty
    /// array if the file does not exist or is invalid.
    /// </returns>
    public HotkeyInfo[] LoadHotkeys()
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