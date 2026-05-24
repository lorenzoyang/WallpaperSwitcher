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
public sealed class HotkeyService : IDisposable
{
    private const int FirstGeneratedHotkeyId = 1000;

    private readonly HotkeyRegistrar _hotkeyRegistrar;
    private readonly IHotkeyStorage _hotkeyStorage;

    // Tracks only hotkeys successfully registered with the operating system.
    private readonly Dictionary<int, HotkeyInfo> _registeredHotkeys = new();

    private bool _disposed;

    private int NextHotkeyId { get; set; } = FirstGeneratedHotkeyId;

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
    /// A snapshot of <see cref="HotkeyInfo"/> values representing the currently registered hotkeys.
    /// </returns>
    /// <exception cref="ObjectDisposedException">Thrown if the service has been disposed.</exception>
    public IEnumerable<HotkeyInfo> GetRegisteredHotkeys()
    {
        ThrowIfDisposed();
        return GetHotkeySnapshot();
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
        ThrowIfDisposed();
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
        ThrowIfDisposed();

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
        ThrowIfDisposed();
        return RegisterHotkey(ParseHotkeyOrThrow(hotkeyString), name, id);
    }

    private int RegisterHotkey(Hotkey hotkey, string name, int? id = null)
    {
        ThrowIfDisposed();

        ValidateHotkeyName(name);
        EnsureHotkeyIdIsAvailable(id);
        EnsureHotkeyCombinationIsAvailable(hotkey);

        var hotkeyId = ReserveHotkeyId(id);
        if (TryRegisterWithOperatingSystem(hotkeyId, hotkey))
        {
            _registeredHotkeys[hotkeyId] = CreateHotkeyInfo(hotkeyId, hotkey, name);
            EnsureNextHotkeyIdIsAfter(hotkeyId);
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
        ThrowIfDisposed();

        var existingHkInfo = GetHotKeyInfoBy(h => h.Name, name);
        return existingHkInfo is not null && UnregisterHotkey(existingHkInfo.Id);
    }

    private bool UnregisterHotkey(int id)
    {
        ThrowIfDisposed();

        if (!_registeredHotkeys.ContainsKey(id))
        {
            return false;
        }

        if (!_hotkeyRegistrar.UnregisterHotKey(id))
        {
            return false;
        }

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
        ThrowIfDisposed();

        var existingHkInfo = GetHotKeyInfoBy(h => h.Name, name);
        if (existingHkInfo is null)
        {
            throw new HotkeyBindingException($"No hotkey registered with the name '{name}'.");
        }

        // A blank binding means the user intentionally disabled this hotkey.
        if (string.IsNullOrWhiteSpace(newHotkeyString))
        {
            if (!UnregisterHotkey(existingHkInfo.Id))
            {
                throw new HotkeyBindingException($"Failed to unregister hotkey '{name}' during re-binding.");
            }

            return;
        }

        var newHotkey = ParseHotkeyOrThrow(newHotkeyString);
        EnsureHotkeyCombinationIsAvailable(newHotkey, allowedHotkeyId: existingHkInfo.Id);

        if (!UnregisterHotkey(existingHkInfo.Id))
        {
            throw new HotkeyBindingException($"Failed to unregister hotkey '{name}' during re-binding.");
        }

        try
        {
            _ = RegisterHotkey(newHotkey, name, existingHkInfo.Id);
        }
        catch
        {
            RestoreHotkey(existingHkInfo);
            throw;
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(HotkeyService));
    }

    private HotkeyInfo[] GetHotkeySnapshot()
    {
        return _registeredHotkeys.Values.ToArray();
    }

    private static void ValidateHotkeyName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new HotkeyBindingException("Hotkey name cannot be null or empty.");
        }
    }

    private void EnsureHotkeyIdIsAvailable(int? id)
    {
        if (id is not null && _registeredHotkeys.ContainsKey(id.Value))
        {
            throw new HotkeyBindingException($"A hotkey with the id '{id}' is already registered.");
        }
    }

    private void EnsureHotkeyCombinationIsAvailable(Hotkey hotkey, int? allowedHotkeyId = null)
    {
        var duplicateHotkeyInfo = IsHotkeyDuplicate(hotkey);
        if (duplicateHotkeyInfo is null || duplicateHotkeyInfo.Id == allowedHotkeyId)
        {
            return;
        }

        throw new HotkeyDuplicateBindingException(
            "A hotkey with the same combination is already registered.",
            duplicateHotkeyInfo
        );
    }

    private static Hotkey ParseHotkeyOrThrow(string hotkeyString)
    {
        if (Hotkey.TryParseFrom(hotkeyString, out var hotkey, out var errorMessage))
        {
            return hotkey;
        }

        throw new HotkeyParsingException(
            $"Failed to parse hotkey string '{hotkeyString}': {errorMessage}",
            hotkeyString
        );
    }

    private int ReserveHotkeyId(int? id)
    {
        return id ?? NextHotkeyId++;
    }

    private bool TryRegisterWithOperatingSystem(int hotkeyId, Hotkey hotkey)
    {
        return _hotkeyRegistrar.RegisterHotKey(hotkeyId, hotkey.ModifierKeys, hotkey.VirtualKeys);
    }

    private static HotkeyInfo CreateHotkeyInfo(int hotkeyId, Hotkey hotkey, string name)
    {
        return new HotkeyInfo
        {
            Id = hotkeyId,
            Hotkey = hotkey,
            Name = name,
        };
    }

    private void RestoreHotkey(HotkeyInfo hotkeyInfo)
    {
        if (TryRegisterWithOperatingSystem(hotkeyInfo.Id, hotkeyInfo.Hotkey))
        {
            _registeredHotkeys[hotkeyInfo.Id] = hotkeyInfo;
        }
    }

    private void EnsureNextHotkeyIdIsAfter(int hotkeyId)
    {
        if (hotkeyId >= NextHotkeyId)
        {
            NextHotkeyId = hotkeyId + 1;
        }
    }

    /// <summary>
    /// Loads hotkeys asynchronously from persistent storage and registers them.
    /// If no hotkeys are found, a default hotkey is registered and saved.
    /// </summary>
    public async Task LoadHotkeysAsync()
    {
        ThrowIfDisposed();

        if (RegisterLoadedHotkeys((await _hotkeyStorage.LoadAsync()).ToArray()))
        {
            await _hotkeyStorage.SaveAsync(GetHotkeySnapshot());
        }
    }

    /// <summary>
    /// Loads hotkeys from persistent storage and registers them.
    /// If no hotkeys are found, a default hotkey is registered and saved.
    /// </summary>
    public void LoadHotkeys()
    {
        ThrowIfDisposed();

        if (RegisterLoadedHotkeys(_hotkeyStorage.Load().ToArray()))
        {
            _hotkeyStorage.Save(GetHotkeySnapshot());
        }
    }

    private bool RegisterLoadedHotkeys(IReadOnlyCollection<HotkeyInfo> hotkeyInfos)
    {
        // First launch: create the default "Next Wallpaper" binding and let the caller persist it.
        if (hotkeyInfos.Count == 0)
        {
            _ = RegisterHotkey(Default.NextWallpaperHotkeyString, Default.NextWallpaperHotkeyName);
            return true;
        }

        foreach (var (id, hotkey, name) in hotkeyInfos)
        {
            _ = RegisterHotkey(hotkey, name, id);
        }

        return false;
    }

    /// <summary>
    /// Saves the currently registered hotkeys asynchronously to persistent storage.
    /// </summary>
    public async Task SaveHotkeysAsync()
    {
        ThrowIfDisposed();
        await _hotkeyStorage.SaveAsync(GetHotkeySnapshot());
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
        if (_disposed)
        {
            return;
        }

        // Release all OS registrations even when Dispose is reached from the finalizer.
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
