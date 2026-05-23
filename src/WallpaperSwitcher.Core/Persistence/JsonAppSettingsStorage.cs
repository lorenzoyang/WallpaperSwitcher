using System.Text.Json;

namespace WallpaperSwitcher.Core.Persistence;

/// <summary>
/// Provides a JSON-based implementation of <see cref="IAppSettingsStorage"/>.
/// </summary>
/// <remarks>
/// By default, settings are persisted to <see cref="AppDataPaths.SettingsFile"/> in the current
/// user's local application data folder.
/// </remarks>
public sealed class JsonAppSettingsStorage : IAppSettingsStorage
{
    private static readonly string DefaultLocation = AppDataPaths.SettingsFile;

    /// <summary>
    /// Gets the full path of the JSON file used for storing application settings.
    /// </summary>
    public string Location { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonAppSettingsStorage"/> class
    /// using the default settings file location.
    /// </summary>
    public JsonAppSettingsStorage() : this(DefaultLocation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonAppSettingsStorage"/> class
    /// with the specified settings file location.
    /// </summary>
    /// <param name="location">The full file path where application settings are stored.</param>
    public JsonAppSettingsStorage(string location)
    {
        Location = location;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Returns default settings if the file does not exist or contains invalid JSON.
    /// </remarks>
    public AppSettings Load()
    {
        return StorageFile.TryOpenRead(Location, out var fileStream)
            ? ReadSettings(fileStream)
            : new AppSettings();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Returns default settings if the file does not exist or contains invalid JSON.
    /// </remarks>
    public async Task<AppSettings> LoadAsync()
    {
        return StorageFile.TryOpenRead(Location, out var fileStream)
            ? await ReadSettingsAsync(fileStream)
            : new AppSettings();
    }

    /// <inheritdoc/>
    public void Save(AppSettings settings)
    {
        StorageFile.EnsureParentDirectoryExists(Location);
        using var fileStream = File.Create(Location);
        JsonSerializer.Serialize(
            fileStream,
            settings,
            SourceGenerationContext.Default.AppSettings
        );
    }

    /// <inheritdoc/>
    public async Task SaveAsync(AppSettings settings)
    {
        StorageFile.EnsureParentDirectoryExists(Location);
        await using var fileStream = File.Create(Location);
        await JsonSerializer.SerializeAsync(
            fileStream,
            settings,
            SourceGenerationContext.Default.AppSettings
        );
    }

    private static AppSettings ReadSettings(Stream stream)
    {
        using (stream)
        {
            try
            {
                return (JsonSerializer.Deserialize(
                    stream,
                    SourceGenerationContext.Default.AppSettings
                ) ?? new AppSettings()).Normalize();
            }
            catch (JsonException)
            {
                return new AppSettings();
            }
        }
    }

    private static async Task<AppSettings> ReadSettingsAsync(Stream stream)
    {
        await using (stream)
        {
            try
            {
                var settings = await JsonSerializer.DeserializeAsync(
                    stream,
                    SourceGenerationContext.Default.AppSettings
                );
                return (settings ?? new AppSettings()).Normalize();
            }
            catch (JsonException)
            {
                return new AppSettings();
            }
        }
    }
}
