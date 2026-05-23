namespace WallpaperSwitcher.Core.Persistence;

public interface IAppSettingsStorage
{
    AppSettings Load();

    Task<AppSettings> LoadAsync();

    void Save(AppSettings settings);

    Task SaveAsync(AppSettings settings);
}