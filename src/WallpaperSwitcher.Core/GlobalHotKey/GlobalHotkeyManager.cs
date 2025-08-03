using System.Diagnostics.CodeAnalysis;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;

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
    /// Registers a new global hotkey with the system.
    /// </summary>
    /// <param name="modifiers">The modifier keys (e.g., Ctrl, Alt) for the hotkey.</param>
    /// <param name="key">The primary virtual key for the hotkey.</param>
    /// <param name="description">
    /// An optional human-readable description for the hotkey. Defaults to a string representation of the key combination.
    /// </param>
    /// <param name="id">An optional specific unique ID for the hotkey. If not provided, a new one is generated.</param>
    /// <returns>
    /// The unique ID of the registered hotkey if successful; otherwise, returns -1.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the <see cref="GlobalHotkeyManager"/> instance has been disposed.
    /// </exception>
    public int RegisterHotkey(ModifierKeys modifiers, VirtualKeys key, string? description = null, int? id = null)
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
                Description = description ?? $"{modifiers} + {key}"
            };
            return hotkeyId;
        }

        NextHotkeyId--;
        return -1;
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