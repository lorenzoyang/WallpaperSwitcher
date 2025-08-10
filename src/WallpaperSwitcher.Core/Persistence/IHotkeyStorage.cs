using WallpaperSwitcher.Core.GlobalHotkey;

namespace WallpaperSwitcher.Core.Persistence;

/// <summary>
/// Defines methods for loading and saving global hotkey configurations.
/// </summary>
public interface IHotkeyStorage
{
    /// <summary>
    /// Asynchronously loads all stored global hotkey configurations.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains 
    /// an <see cref="IEnumerable{T}"/> of <see cref="HotkeyInfo"/> objects representing the stored hotkeys.
    /// </returns>
    Task<IEnumerable<HotkeyInfo>> LoadAsync();

    /// <summary>
    /// Loads all stored global hotkey configurations.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="HotkeyInfo"/> objects representing the stored hotkeys.
    /// </returns>
    IEnumerable<HotkeyInfo> Load();

    /// <summary>
    /// Asynchronously saves the specified global hotkey configurations.
    /// </summary>
    /// <param name="hotkeys">
    /// The collection of <see cref="HotkeyInfo"/> objects to store.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous save operation.
    /// </returns>
    Task SaveAsync(IEnumerable<HotkeyInfo> hotkeys);

    /// <summary>
    /// Saves the specified global hotkey configurations.
    /// </summary>
    /// <param name="hotkeys">
    /// The collection of <see cref="HotkeyInfo"/> objects to store.
    /// </param>
    void Save(IEnumerable<HotkeyInfo> hotkeys);
}