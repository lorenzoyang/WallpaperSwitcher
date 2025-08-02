using System.Diagnostics.CodeAnalysis;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace WallpaperSwitcher.Core.GlobalHotKey;

/// <summary>
/// 
/// </summary>
public class GlobalHotkeyManager
{
    /// <summary>
    /// 
    /// </summary>
    private readonly HWND _windowHandle;

    /// <summary>
    /// 
    /// </summary>
    private int NextHotkeyId { get; set; } = 1000;

    /// <summary>
    /// 
    /// </summary>
    private readonly Dictionary<int, HotkeyInfo> _registeredHotkeys = new();

    private bool _disposed;

    /// <summary>
    /// A special Windows message code that means: "A registered hotkey was pressed",
    /// It's part of the Windows messaging system (how Windows communicates with applications).
    /// </summary>
    public const int WmHotkeyMessage = 0x0312;

    /// <summary>
    /// Event fired when a registered hotkey is pressed
    /// </summary>
    public event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

    public GlobalHotkeyManager(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
            throw new ArgumentException("Window handle cannot be null.", nameof(windowHandle));
        _windowHandle = (HWND)windowHandle;
    }

    public void ProcessWindowMessage(HotkeyInfo hotkeyInfo)
    {
        HotkeyPressed?.Invoke(this, new HotkeyPressedEventArgs(hotkeyInfo));
    }

    public bool TryGetHotkeyInfo(int id, [NotNullWhen(true)] out HotkeyInfo? hotkeyInfo)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GlobalHotkeyManager));

        return _registeredHotkeys.TryGetValue(id, out hotkeyInfo);
    }

    public int RegisterHotkey(ModifierKeys modifiers, VirtualKeys key, string? name = null, int? id = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GlobalHotkeyManager));

        var hotkeyId = id ?? NextHotkeyId++;
        if (PInvoke.RegisterHotKey(_windowHandle, hotkeyId, (HOT_KEY_MODIFIERS)modifiers, (uint)key))
        {
            _registeredHotkeys[hotkeyId] = new HotkeyInfo
            {
                Id = hotkeyId,
                Modifiers = modifiers,
                Key = key,
                Name = name ?? $"{modifiers} + {key}"
            };
            return hotkeyId;
        }

        NextHotkeyId--;
        return -1;
    }

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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            var ids = new List<int>(_registeredHotkeys.Keys);
            foreach (var id in ids)
            {
                PInvoke.UnregisterHotKey(_windowHandle, id);
                _registeredHotkeys.Remove(id);
            }
        }

        _disposed = true;
    }
}

/// <summary>
/// Event arguments for hotkey pressed events
/// </summary>
public class HotkeyPressedEventArgs(HotkeyInfo hotkeyInfo) : EventArgs
{
    public HotkeyInfo HotkeyInfo { get; } = hotkeyInfo;
}