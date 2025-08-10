using System.Text.Json;
using System.Text.Json.Serialization;
using WallpaperSwitcher.Core.GlobalHotkey;

namespace WallpaperSwitcher.Core.Persistence;

/// <summary>
/// Provides a JSON-based implementation of <see cref="IHotkeyStorage"/> for
/// persisting and retrieving global hotkey configurations.
/// </summary>
/// <remarks>
/// Hotkeys are stored in a JSON file at a specified location on the local file system.
/// By default, the storage file is located in the user's local application data folder.
/// </remarks>
public sealed class JsonHotkeyStorage : IHotkeyStorage
{
    private static readonly string DefaultLocation = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WallpaperSwitcher",
        "hotkeys.json"
    );

    /// <summary>
    /// Gets the full path of the JSON file used for storing hotkey configurations.
    /// </summary>
    public string Location { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonHotkeyStorage"/> class
    /// using the default file storage location.
    /// </summary>
    public JsonHotkeyStorage() : this(DefaultLocation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonHotkeyStorage"/> class
    /// with the specified file storage location.
    /// </summary>
    /// <param name="location">
    /// The full file path where hotkey configurations will be stored.
    /// </param>
    public JsonHotkeyStorage(string location)
    {
        Location = location;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Returns an empty collection if the file does not exist or contains invalid JSON.
    /// </remarks>
    public async Task<IEnumerable<HotkeyInfo>> LoadAsync()
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

    /// <inheritdoc/>
    /// <remarks>
    /// Returns an empty collection if the file does not exist or contains invalid JSON.
    /// </remarks>
    public IEnumerable<HotkeyInfo> Load()
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

    /// <inheritdoc/>
    public async Task SaveAsync(IEnumerable<HotkeyInfo> hotkeys)
    {
        EnsureDirectoryExists();
        await using var fileStream = File.Create(Location);
        await JsonSerializer.SerializeAsync(fileStream, hotkeys, SourceGenerationContext.Default.HotkeyInfoArray);
    }

    /// <inheritdoc/>
    public void Save(IEnumerable<HotkeyInfo> hotkeys)
    {
        EnsureDirectoryExists();
        using var fileStream = File.Create(Location);
        JsonSerializer.Serialize(fileStream, hotkeys, SourceGenerationContext.Default.HotkeyInfoArray);
    }
    
    private void EnsureDirectoryExists()
    {
        var directory = Path.GetDirectoryName(Location);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}

/// <summary>
/// Provides source generation context for System.Text.Json to enable
/// high-performance serialization and deserialization of <see cref="HotkeyInfo"/> objects.
/// </summary>
/// <remarks>
/// This class is used to configure JSON source generation, reducing runtime reflection overhead.
/// </remarks>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(HotkeyInfo))]
[JsonSerializable(typeof(HotkeyInfo[]))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}