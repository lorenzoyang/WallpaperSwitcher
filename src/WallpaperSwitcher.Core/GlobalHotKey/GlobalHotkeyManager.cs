using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using WallpaperSwitcher.Core.GlobalHotKey.Exceptions;
using WallpaperSwitcher.Core.GlobalHotKey.Persistence;

namespace WallpaperSwitcher.Core.GlobalHotKey;

/// <summary>
/// Manages the registration and handling of global hotkeys for a Windows application.
/// Allows the application to respond to hotkey presses even when not in focus.
/// </summary>
public class GlobalHotkeyManager
{
    /// <summary>
    /// Handle to the window that receives hotkey messages.
    /// </summary>
    private readonly HWND _windowHandle;

    /// <summary>
    /// Gets or sets the next available unique ID for hotkey registration.
    /// </summary>
    private int NextHotkeyId { get; set; } = 1000;

    /// <summary>
    /// Stores the mapping between registered hotkey IDs and their corresponding hotkey information.
    /// </summary>
    private readonly Dictionary<int, HotkeyInfo> _registeredHotkeys = new();

    private bool _disposed;

    /// <summary>
    /// Handles the persistence of registered hotkeys (e.g., saving and loading from disk).
    /// </summary>
    private readonly HotkeyPersistenceManager _hotkeyPersistenceManager = new();

    /// <summary>
    /// The Windows message identifier for a registered hotkey being pressed.
    /// Used in message processing to identify hotkey notifications.
    /// </summary>
    public const int WmHotkeyMessage = 0x0312;

    /// <summary>
    /// Occurs when a registered global hotkey is pressed.
    /// </summary>
    public event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalHotkeyManager"/> class
    /// with the specified window handle to receive hotkey messages.
    /// </summary>
    /// <param name="windowHandle">The handle to the window that will receive hotkey messages.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the provided window handle is <see cref="IntPtr.Zero"/>.
    /// </exception>
    public GlobalHotkeyManager(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
            throw new ArgumentException("Window handle cannot be null.", nameof(windowHandle));
        _windowHandle = (HWND)windowHandle;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ObjectDisposedException"></exception>
    public HotkeyInfo[] GetRegisteredHotkeys()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GlobalHotkeyManager));

        return _registeredHotkeys.Values.ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="ObjectDisposedException"></exception>
    public HotkeyInfo? GetHotKeyInfo(string name)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GlobalHotkeyManager));
        return _registeredHotkeys.Values
            .FirstOrDefault(hotkeyInfo => hotkeyInfo.Name == name);
    }

    /// <summary>
    /// Processes a window message to check if it's a hotkey press event.
    /// If the message is a hotkey press, it raises the <see cref="HotkeyPressed"/> event.
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
    /// 
    /// </summary>
    /// <param name="modifierKeys"></param>
    /// <param name="key"></param>
    /// <param name="name"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="ObjectDisposedException"></exception>
    /// <exception cref="HotkeyDuplicateBindingException"></exception>
    /// <exception cref="HotkeyBindingException"></exception>
    public int RegisterHotkey(ModifierKeys modifierKeys, VirtualKeys key, string name, int? id = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GlobalHotkeyManager));

        var existingHotkeyInfo = IsHotkeyDuplicate(modifierKeys, key);
        if (existingHotkeyInfo is not null)
        {
            throw new HotkeyDuplicateBindingException(
                "A hotkey with the same combination is already registered.",
                existingHotkeyInfo
            );
        }

        var hotkeyId = id ?? NextHotkeyId++;
        if (PInvoke.RegisterHotKey(_windowHandle, hotkeyId, (HOT_KEY_MODIFIERS)modifierKeys, (uint)key))
        {
            _registeredHotkeys[hotkeyId] = new HotkeyInfo
            {
                Id = hotkeyId,
                ModifierKeys = modifierKeys,
                Key = key,
                Name = name,
            };
            return hotkeyId;
        }

        throw new HotkeyBindingException(
            $"Failed to register hotkey: {modifierKeys.ToFormattedString()} + {key} for {name}."
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hotkeyText"></param>
    /// <param name="name"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="ObjectDisposedException"></exception>
    /// <exception cref="HotkeyParsingException"></exception>
    public int RegisterHotkey(string hotkeyText, string name, int? id = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GlobalHotkeyManager));
        if (HotkeyInfo.TryParseFromString(hotkeyText, out var modifierKeys, out var virtualKey, out var errorMessage))
        {
            return RegisterHotkey(modifierKeys, virtualKey, name);
        }

        throw new HotkeyParsingException($"Failed to parse hotkey text '{hotkeyText}': {errorMessage}", hotkeyText);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modifierKeys"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    private HotkeyInfo? IsHotkeyDuplicate(ModifierKeys modifierKeys, VirtualKeys key)
    {
        return _registeredHotkeys.Values
            .FirstOrDefault(hotkeyInfo => hotkeyInfo.ModifierKeys == modifierKeys && hotkeyInfo.Key == key);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="ObjectDisposedException"></exception>
    public bool UnregisterHotkey(int id)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GlobalHotkeyManager));

        if (!_registeredHotkeys.ContainsKey(id))
            return false;

        if (PInvoke.UnregisterHotKey(_windowHandle, id))
        {
            _registeredHotkeys.Remove(id);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="newHotkeyText"></param>
    /// <exception cref="ObjectDisposedException"></exception>
    /// <exception cref="HotkeyBindingException"></exception>
    public void ChangeHotkeyBinding(string name, string newHotkeyText)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GlobalHotkeyManager));

        var existingHkInfo = GetHotKeyInfo(name);
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
        if (string.IsNullOrWhiteSpace(newHotkeyText)) return;

        _ = RegisterHotkey(newHotkeyText, name, existingHkInfo.Id);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="ObjectDisposedException"></exception>
    public bool UnregisterHotkey(string name)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GlobalHotkeyManager));
        var hotkeyInfo = _registeredHotkeys.Values
            .FirstOrDefault(hotkeyInfo => hotkeyInfo.Name == name);
        return hotkeyInfo is not null && UnregisterHotkey(hotkeyInfo.Id);
    }


    /// <summary>
    /// The default name used for the "Next Wallpaper" hotkey.
    /// </summary>
    public const string DefaultNextWallpaperHotkeyName = "Next Wallpaper";

    /// <summary>
    /// The default key combination string for the "Next Wallpaper" hotkey.
    /// </summary>
    private const string DefaultNextWallpaperHotkey = "CTRL+SHIFT+N";

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    public async Task LoadingHotkeysAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GlobalHotkeyManager));

        var isFirstLoad = !File.Exists(_hotkeyPersistenceManager.Location);
        var hotkeyInfos = await _hotkeyPersistenceManager.LoadHotkeysAsync();
        if (isFirstLoad)
        {
            // initialize with default hotkey: Next Wallpaper CTRL+SHIFT+N
            _ = RegisterHotkey(DefaultNextWallpaperHotkey, DefaultNextWallpaperHotkeyName);
            await _hotkeyPersistenceManager.SaveHotkeysAsync(_registeredHotkeys.Values.ToArray());
        }
        else
        {
            // otherwise, register all loaded hotkeys
            foreach (var (id, modifierKeys, key, name) in hotkeyInfos)
            {
                _ = RegisterHotkey(modifierKeys, key, name, id);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task SaveHotkeysAsync()
    {
        await _hotkeyPersistenceManager.SaveHotkeysAsync(_registeredHotkeys.Values.ToArray());
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
    /// Finalizes an instance of the <see cref="GlobalHotkeyManager"/> class.
    /// This is not strictly necessary as Windows automatically cleans up hotkeys,
    /// but it ensures resources are released in case of unexpected termination.
    /// </summary>
    ~GlobalHotkeyManager()
    {
        Dispose(false);
    }

    /// <summary>
    /// Disposes of the managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing">
    /// A value indicating whether to dispose of managed resources.
    /// </param>
    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        // Always unregister hotkeys (both managed and unmanaged cleanup)
        var ids = new List<int>(_registeredHotkeys.Keys);
        foreach (var id in ids)
        {
            PInvoke.UnregisterHotKey(_windowHandle, id);
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
/// Provides data for the <see cref="GlobalHotkeyManager.HotkeyPressed"/> event.
/// </summary>
/// <param name="hotkeyInfo">The information about the hotkey that was pressed.</param>
public class HotkeyPressedEventArgs(HotkeyInfo hotkeyInfo) : EventArgs
{
    public HotkeyInfo HotkeyInfo { get; } = hotkeyInfo;
}