using System.Text.Json;

namespace WallpaperSwitcher.Core.GlobalHotKey;

public class HotkeyPersistenceManager
{
    private static readonly string DefaultLocation = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WallpaperSwitcher",
        "hotkeys.json"
    );

    private string Location { get; }

    public HotkeyPersistenceManager()
        : this(DefaultLocation)
    {
    }

    private HotkeyPersistenceManager(string location)
    {
        Location = location;
    }

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
                await JsonSerializer.DeserializeAsync(fileStream, SourceGenerationContext.Default.HotkeyInfoArray);
            return hotkeys ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}