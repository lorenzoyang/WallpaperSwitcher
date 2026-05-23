namespace WallpaperSwitcher.Core.Persistence;

/// <summary>
/// Defines persistence operations for loading and saving application settings.
/// </summary>
/// <remarks>
/// This abstraction keeps the desktop UI independent from the concrete storage format used for
/// user settings.
/// </remarks>
public interface IAppSettingsStorage
{
    /// <summary>
    /// Loads application settings from persistent storage.
    /// </summary>
    /// <returns>The stored settings, or default settings when no valid settings are available.</returns>
    AppSettings Load();

    /// <summary>
    /// Loads application settings from persistent storage asynchronously.
    /// </summary>
    /// <returns>A task that resolves to the stored settings, or default settings when no valid settings are available.</returns>
    Task<AppSettings> LoadAsync();

    /// <summary>
    /// Saves the specified application settings to persistent storage.
    /// </summary>
    /// <param name="settings">The settings to save.</param>
    void Save(AppSettings settings);

    /// <summary>
    /// Saves the specified application settings to persistent storage asynchronously.
    /// </summary>
    /// <param name="settings">The settings to save.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    Task SaveAsync(AppSettings settings);
}
