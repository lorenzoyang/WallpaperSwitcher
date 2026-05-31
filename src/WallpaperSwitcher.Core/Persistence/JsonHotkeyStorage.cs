using System.Text.Json;
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
    private static readonly string DefaultLocation = AppDataPaths.HotkeysFile;

    /// <summary>
    /// Gets the full path of the JSON file used for storing hotkey configurations.
    /// </summary>
    public string Location { get; }

    /// <inheritdoc/>
    public bool Exists => File.Exists(Location);

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
        var fileStream = StorageFile.OpenReadIfExists(Location);
        return fileStream is null ? EmptyHotkeys() : await ReadHotkeysAsync(fileStream);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Returns an empty collection if the file does not exist or contains invalid JSON.
    /// </remarks>
    public IEnumerable<HotkeyInfo> Load()
    {
        var fileStream = StorageFile.OpenReadIfExists(Location);
        return fileStream is null ? EmptyHotkeys() : ReadHotkeys(fileStream);
    }

    /// <inheritdoc/>
    public async Task SaveAsync(IEnumerable<HotkeyInfo> hotkeys)
    {
        var hotkeyArray = ToHotkeyArray(hotkeys);
        StorageFile.EnsureParentDirectoryExists(Location);
        await using var fileStream = File.Create(Location);
        await JsonSerializer.SerializeAsync(fileStream, hotkeyArray, SourceGenerationContext.Default.HotkeyInfoArray);
    }

    /// <inheritdoc/>
    public void Save(IEnumerable<HotkeyInfo> hotkeys)
    {
        var hotkeyArray = ToHotkeyArray(hotkeys);
        StorageFile.EnsureParentDirectoryExists(Location);
        using var fileStream = File.Create(Location);
        JsonSerializer.Serialize(fileStream, hotkeyArray, SourceGenerationContext.Default.HotkeyInfoArray);
    }

    private static HotkeyInfo[] ToHotkeyArray(IEnumerable<HotkeyInfo> hotkeys)
    {
        return hotkeys as HotkeyInfo[] ?? hotkeys.ToArray();
    }

    private static HotkeyInfo[] EmptyHotkeys()
    {
        return [];
    }

    private static HotkeyInfo[] ReadHotkeys(Stream stream)
    {
        using (stream)
        {
            try
            {
                return JsonSerializer.Deserialize(
                    stream,
                    SourceGenerationContext.Default.HotkeyInfoArray
                ) ?? EmptyHotkeys();
            }
            catch (JsonException)
            {
                return EmptyHotkeys();
            }
        }
    }

    private static async Task<HotkeyInfo[]> ReadHotkeysAsync(Stream stream)
    {
        await using (stream)
        {
            try
            {
                return await JsonSerializer.DeserializeAsync(
                    stream,
                    SourceGenerationContext.Default.HotkeyInfoArray
                ) ?? EmptyHotkeys();
            }
            catch (JsonException)
            {
                return EmptyHotkeys();
            }
        }
    }
}
