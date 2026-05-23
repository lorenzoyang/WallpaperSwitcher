using System.Text.Json;

namespace WallpaperSwitcher.Core.Persistence;

public sealed class JsonAppSettingsStorage : IAppSettingsStorage
{
    private static readonly string DefaultLocation = AppDataPaths.SettingsFile;

    public string Location { get; }

    public JsonAppSettingsStorage() : this(DefaultLocation)
    {
    }

    public JsonAppSettingsStorage(string location)
    {
        Location = location;
    }

    public AppSettings Load()
    {
        if (!File.Exists(Location))
        {
            return new AppSettings();
        }

        try
        {
            using var fileStream = File.OpenRead(Location);
            return JsonSerializer.Deserialize(
                fileStream,
                SourceGenerationContext.Default.AppSettings
            ) ?? new AppSettings();
        }
        catch (JsonException)
        {
            return new AppSettings();
        }
    }

    public async Task<AppSettings> LoadAsync()
    {
        if (!File.Exists(Location))
        {
            return new AppSettings();
        }

        try
        {
            await using var fileStream = File.OpenRead(Location);
            return await JsonSerializer.DeserializeAsync(
                fileStream,
                SourceGenerationContext.Default.AppSettings
            ) ?? new AppSettings();
        }
        catch (JsonException)
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        EnsureDirectoryExists();
        using var fileStream = File.Create(Location);
        JsonSerializer.Serialize(
            fileStream,
            settings,
            SourceGenerationContext.Default.AppSettings
        );
    }

    public async Task SaveAsync(AppSettings settings)
    {
        EnsureDirectoryExists();
        await using var fileStream = File.Create(Location);
        await JsonSerializer.SerializeAsync(
            fileStream,
            settings,
            SourceGenerationContext.Default.AppSettings
        );
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