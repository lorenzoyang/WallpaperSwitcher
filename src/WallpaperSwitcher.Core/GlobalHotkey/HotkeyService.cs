using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using WallpaperSwitcher.Core.Exceptions;
using WallpaperSwitcher.Core.Persistence;

namespace WallpaperSwitcher.Core.GlobalHotkey;

/// <summary>
/// 
/// </summary>
public sealed class HotkeyService
{
    /// <summary>
    /// 
    /// </summary>
    private readonly HWND _hWnd;

    /// <summary>
    /// 
    /// </summary>
    private readonly Dictionary<int, HotkeyInfo> _registeredHotkeys = new();

    /// <summary>
    /// 
    /// </summary>
    private readonly HotkeyStorage _hotkeyStorage = new();

    private bool _disposed;

    /// <summary>
    /// 
    /// </summary>
    private int NextHotkeyId { get; set; } = 1000;

    // The Windows message identifier for a registered hotkey being pressed.
    // Used in message processing to identify hotkey notifications.
    public const int WmHotkey = 0x0312;

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
    public event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

    public HotkeyService(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero)
            throw new ArgumentException("Window handle cannot be null.", nameof(hWnd));
        _hWnd = (HWND)hWnd;
    }
    
    public HotkeyInfo[] GetRegisteredHotkeys()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HotkeyService));

        return _registeredHotkeys.Values.ToArray();
    }
    
    public HotkeyInfo? GetHotKeyInfo(string name)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HotkeyService));
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
    /// <param name="hotkey"></param>
    /// <param name="name"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="HotkeyDuplicateBindingException"></exception>
    /// <exception cref="HotkeyBindingException"></exception>
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
        if (PInvoke.RegisterHotKey(_hWnd, hotkeyId, (HOT_KEY_MODIFIERS)hotkey.ModifierKeys, (uint)hotkey.VirtualKeys))
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hotkeyString"></param>
    /// <param name="name"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="HotkeyParsingException"></exception>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hotkey"></param>
    /// <returns></returns>
    private HotkeyInfo? IsHotkeyDuplicate(Hotkey hotkey)
    {
        return _registeredHotkeys.Values.FirstOrDefault(hotkeyInfo => hotkeyInfo.Hotkey == hotkey);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private bool UnregisterHotkey(int id)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(HotkeyService));

        if (!_registeredHotkeys.ContainsKey(id))
            return false;

        if (!PInvoke.UnregisterHotKey(_hWnd, id)) return false;

        _registeredHotkeys.Remove(id);
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="newHotkeyString"></param>
    /// <exception cref="HotkeyBindingException"></exception>
    public void ChangeHotkeyBinding(string name, string newHotkeyString)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(HotkeyService));

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
        if (string.IsNullOrWhiteSpace(newHotkeyString)) return;

        _ = RegisterHotkey(newHotkeyString, name, existingHkInfo.Id);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool UnregisterHotkey(string name)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(HotkeyService));

        var existingHkInfo = GetHotKeyInfo(name);
        return existingHkInfo is not null && UnregisterHotkey(existingHkInfo.Id);
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task LoadHotkeysAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(HotkeyService));

        var hotkeyInfos = await _hotkeyStorage.LoadAsync();
        // If is the first load
        if (hotkeyInfos.Length == 0)
        {
            // initialize with default hotkey: Next Wallpaper CTRL+SHIFT+N
            _ = RegisterHotkey(DefaultNextWallpaperHotkey, DefaultNextWallpaperHotkeyName);
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
    /// 
    /// </summary>
    public void LoadHotkeys()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(HotkeyService));

        var hotkeyInfos = _hotkeyStorage.Load();
        // If is the first load
        if (hotkeyInfos.Length == 0)
        {
            // initialize with default hotkey: Next Wallpaper CTRL+SHIFT+N
            _ = RegisterHotkey(DefaultNextWallpaperHotkey, DefaultNextWallpaperHotkeyName);
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
    /// 
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
            PInvoke.UnregisterHotKey(_hWnd, id);
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
    public HotkeyInfo HotkeyInfo { get; } = hotkeyInfo;
}