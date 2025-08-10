using WallpaperSwitcher.Core.Exceptions;
using WallpaperSwitcher.Core.Persistence;

namespace WallpaperSwitcher.Core.GlobalHotkey;

/// <summary>
/// Provides services for registering, managing, and handling global hotkeys.
/// </summary>
/// <remarks>
/// This service encapsulates registration, persistence, and event handling for
/// global hotkeys. It works with a <see cref="HotkeyRegistrar"/> to communicate
/// with the Windows API and an <see cref="IHotkeyStorage"/> implementation for
/// saving and loading hotkey configurations.
/// </remarks>
public sealed class HotkeyService
{
    private readonly HotkeyRegistrar _hotkeyRegistrar;
    private readonly IHotkeyStorage _hotkeyStorage;

    // Registered hotkeys should be released when the service is disposed.
    // Any usage of this field should check for disposal.
    private readonly Dictionary<int, HotkeyInfo> _registeredHotkeys = new();

    private bool _disposed;

    private int NextHotkeyId { get; set; } = 1000;

    /// <summary>
    /// The Windows message identifier for a registered hotkey being pressed.
    /// Used in message processing to identify hotkey notifications.
    /// </summary>
    public const int WmHotkey = 0x0312;

    /// <summary>
    /// Initializes a new instance of the <see cref="HotkeyService"/> class.
    /// </summary>
    /// <param name="hotkeyRegistrar">
    /// The component responsible for registering and unregistering hotkeys with the operating system.
    /// </param>
    /// <param name="hotkeyStorage">
    /// The storage provider used for loading and saving hotkey configurations.
    /// </param>
    public HotkeyService(HotkeyRegistrar hotkeyRegistrar, IHotkeyStorage hotkeyStorage)
    {
        _hotkeyRegistrar = hotkeyRegistrar;
        _hotkeyStorage = hotkeyStorage;
    }

    /// <summary>
    /// Occurs when a registered hotkey is pressed.
    /// </summary>
    public event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

    /// <summary>
    /// Gets all currently registered hotkeys.
    /// </summary>
    /// <returns>
    /// An immutable collection of <see cref="HotkeyInfo"/> representing the currently registered hotkeys.
    /// </returns>
    /// <exception cref="ObjectDisposedException">Thrown if the service has been disposed.</exception>
    public IEnumerable<HotkeyInfo> GetRegisteredHotkeys()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(HotkeyService));
        return _registeredHotkeys.Values;
    }

    /// <summary>
    /// Finds a registered hotkey by a property value.
    /// </summary>
    /// <typeparam name="T">The type of the property being searched.</typeparam>
    /// <param name="propertySelector">A function that selects the property to compare.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>
    /// The matching <see cref="HotkeyInfo"/> if found; otherwise, <c>null</c>.
    /// </returns>
    /// <exception cref="ObjectDisposedException">Thrown if the service has been disposed.</exception>
    public HotkeyInfo? GetHotKeyInfoBy<T>(Func<HotkeyInfo, T> propertySelector, T value)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(HotkeyService));
        return _registeredHotkeys.Values
            .FirstOrDefault(h => EqualityComparer<T>.Default.Equals(propertySelector(h), value));
    }

    /// <summary>
    /// Processes a Windows message to detect hotkey presses.
    /// If the message corresponds to a registered hotkey, the <see cref="HotkeyPressed"/> event is raised.
    /// </summary>
    /// <param name="id">The identifier of the hotkey that was pressed.</param>
    public void ProcessWindowMessage(int id)
    {
        if (_registeredHotkeys.TryGetValue(id, out var hotkeyInfo))
        {
            HotkeyPressed?.Invoke(this, new HotkeyPressedEventArgs(hotkeyInfo));
        }
    }

    /// <summary>
    /// Registers a hotkey from its string representation.
    /// </summary>
    /// <param name="hotkeyString">The string representation of the hotkey (e.g., "Ctrl+Shift+N").</param>
    /// <param name="name">The descriptive name of the hotkey.</param>
    /// <param name="id">An optional identifier to assign to the hotkey.</param>
    /// <returns>The identifier assigned to the registered hotkey.</returns>
    /// <exception cref="HotkeyParsingException">
    /// Thrown if the hotkey string could not be parsed.
    /// </exception>
    /// <exception cref="HotkeyDuplicateBindingException">
    /// Thrown if a hotkey with the same combination is already registered.
    /// </exception>
    /// <exception cref="HotkeyBindingException">
    /// Thrown if registration with the OS fails.
    /// </exception>
    public int RegisterHotkey(string hotkeyString, string name, int? id = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(HotkeyService));
        if (Hotkey.TryParseFrom(hotkeyString, out var hotkey, out var errorMessage))
        {
            return RegisterHotkey(hotkey, name);
        }

        throw new HotkeyParsingException($"Failed to parse hotkey string '{hotkeyString}': {errorMessage}",
            hotkeyString);
    }

    private int RegisterHotkey(Hotkey hotkey, string name, int? id = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(HotkeyService));

        var existingHotkeyInfo = IsHotkeyDuplicate(hotkey);
        if (existingHotkeyInfo is not null)
        {
            throw new HotkeyDuplicateBindingException(
                "A hotkey with the same combination is already registered.",
                existingHotkeyInfo
            );
        }

        if (string.IsNullOrEmpty(name))
        {
            throw new HotkeyBindingException("Hotkey name cannot be null or empty.");
        }

        var hotkeyId = id ?? NextHotkeyId++;
        if (_hotkeyRegistrar.RegisterHotKey(hotkeyId, hotkey.ModifierKeys, hotkey.VirtualKeys))
        {
            _registeredHotkeys[hotkeyId] = new HotkeyInfo
            {
                Id = hotkeyId,
                Hotkey = hotkey,
                Name = name,
            };
            return hotkeyId;
        }

        throw new HotkeyBindingException(
            $"Failed to register hotkey: {hotkey} for {name}."
        );
    }

    private HotkeyInfo? IsHotkeyDuplicate(Hotkey hotkey)
    {
        return _registeredHotkeys.Values.FirstOrDefault(hotkeyInfo => hotkeyInfo.Hotkey == hotkey);
    }

    /// <summary>
    /// Unregisters a hotkey by name.
    /// </summary>
    /// <param name="name">The name of the hotkey to unregister.</param>
    /// <returns><c>true</c> if the hotkey was successfully unregistered; otherwise, <c>false</c>.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the service has been disposed.</exception>
    public bool UnregisterHotkey(string name)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(HotkeyService));

        var existingHkInfo = GetHotKeyInfoBy(h => h.Name, name);
        return existingHkInfo is not null && UnregisterHotkey(existingHkInfo.Id);
    }

    private bool UnregisterHotkey(int id)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(HotkeyService));

        if (!_registeredHotkeys.ContainsKey(id))
            return false;

        if (_hotkeyRegistrar.UnregisterHotKey(id)) return false;

        _registeredHotkeys.Remove(id);
        return true;
    }

    /// <summary>
    /// Changes the hotkey binding for the specified name.
    /// </summary>
    /// <param name="name">The name of the existing hotkey binding.</param>
    /// <param name="newHotkeyString">The new hotkey string. If null or whitespace, the hotkey is removed.</param>
    /// <exception cref="HotkeyBindingException">
    /// Thrown if no hotkey with the specified name exists or if re-binding fails.
    /// </exception>
    public void ChangeHotkeyBinding(string name, string newHotkeyString)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(HotkeyService));

        var existingHkInfo = GetHotKeyInfoBy(h => h.Name, name);
        if (existingHkInfo is null)
        {
            throw new HotkeyBindingException($"No hotkey registered with the name '{name}'.");
        }

        if (!UnregisterHotkey(existingHkInfo.Id))
        {
            throw new HotkeyBindingException($"Failed to unregister hotkey '{name}' during re-binding.");
        }

        // If the new hotkey text is empty or whitespace, we do not register a new hotkey
        // and simply remove the existing hotkey.
        if (string.IsNullOrWhiteSpace(newHotkeyString)) return;

        _ = RegisterHotkey(newHotkeyString, name, existingHkInfo.Id);
    }

    /// <summary>
    /// Loads hotkeys asynchronously from persistent storage and registers them.
    /// If no hotkeys are found, a default hotkey is registered and saved.
    /// </summary>
    public async Task LoadHotkeysAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(HotkeyService));

        var hotkeyInfos = (await _hotkeyStorage.LoadAsync()).ToArray();
        // If is the first load
        if (hotkeyInfos.Length == 0)
        {
            // initialize with default hotkey: Next Wallpaper CTRL+SHIFT+N
            _ = RegisterHotkey(Default.NextWallpaperHotkeyString, Default.NextWallpaperHotkeyName);
            await _hotkeyStorage.SaveAsync(_registeredHotkeys.Values.ToArray());
        }
        else
        {
            // otherwise, register all loaded hotkeys
            foreach (var (id, hotkey, name) in hotkeyInfos)
            {
                _ = RegisterHotkey(hotkey, name, id);
            }
        }
    }

    /// <summary>
    /// Loads hotkeys from persistent storage and registers them.
    /// If no hotkeys are found, a default hotkey is registered and saved.
    /// </summary>
    public void LoadHotkeys()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(HotkeyService));

        var hotkeyInfos = _hotkeyStorage.Load().ToArray();
        // If is the first load
        if (hotkeyInfos.Length == 0)
        {
            // initialize with default hotkey: Next Wallpaper CTRL+SHIFT+N
            _ = RegisterHotkey(Default.NextWallpaperHotkeyString, Default.NextWallpaperHotkeyName);
            _hotkeyStorage.Save(_registeredHotkeys.Values.ToArray());
        }
        else
        {
            // otherwise, register all loaded hotkeys
            foreach (var (id, hotkey, name) in hotkeyInfos)
            {
                _ = RegisterHotkey(hotkey, name, id);
            }
        }
    }

    /// <summary>
    /// Saves the currently registered hotkeys asynchronously to persistent storage.
    /// </summary>
    public async Task SaveHotkeysAsync()
    {
        await _hotkeyStorage.SaveAsync(_registeredHotkeys.Values.ToArray());
    }

    /// <summary>
    /// Unregisters all hotkeys and releases the resources used by the manager.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="HotkeyService"/> class.
    /// This is not strictly necessary as Windows automatically cleans up hotkeys,
    /// but it ensures resources are released in case of unexpected termination.
    /// </summary>
    ~HotkeyService()
    {
        Dispose(false);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        // Always unregister hotkeys (both managed and unmanaged cleanup)
        var ids = new List<int>(_registeredHotkeys.Keys);
        foreach (var id in ids)
        {
            _hotkeyRegistrar.UnregisterHotKey(id);
            _registeredHotkeys.Remove(id);
        }

        if (disposing)
        {
            _registeredHotkeys.Clear();
        }

        _disposed = true;
    }
}

/// <summary>
/// Provides data for the <see cref="HotkeyService.HotkeyPressed"/> event.
/// </summary>
/// <param name="hotkeyInfo">The information about the hotkey that was pressed.</param>
public class HotkeyPressedEventArgs(HotkeyInfo hotkeyInfo) : EventArgs
{
    /// <summary>
    /// Gets the information about the hotkey that was pressed.
    /// </summary>
    public HotkeyInfo HotkeyInfo { get; } = hotkeyInfo;
}