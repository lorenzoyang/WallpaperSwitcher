using Windows.Win32.Foundation;
using WallpaperSwitcher.Core.GlobalHotkey;

namespace WallpaperSwitcher.Core.Win32Api;

/// <summary>
/// Provides a base abstraction for registering and unregistering global hotkeys
/// with a specific native window handle.
/// </summary>
public abstract class HotkeyRegistrar
{
    /// <summary>
    /// Gets the native window handle associated with this hotkey registrar.
    /// </summary>
    private protected HWND HWnd { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HotkeyRegistrar"/> class
    /// using the specified window handle.
    /// </summary>
    /// <param name="hWnd">The handle to the window that will receive hotkey messages.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="hWnd"/> is <see cref="IntPtr.Zero"/>.
    /// </exception>
    protected HotkeyRegistrar(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero) throw new ArgumentException("Window handle cannot be null.", nameof(hWnd));
        HWnd = (HWND)hWnd;
    }

    /// <summary>
    /// Registers a global hotkey for the associated window.
    /// </summary>
    /// <param name="id">The unique identifier for the hotkey.</param>
    /// <param name="modifiers">The modifier keys (e.g., Ctrl, Alt) to use with the hotkey.</param>
    /// <param name="key">The primary key for the hotkey.</param>
    /// <returns>
    /// <see langword="true"/> if the hotkey was successfully registered; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public abstract bool RegisterHotKey(int id, ModifierKeys modifiers, VirtualKeys key);

    /// <summary>
    /// Unregisters a previously registered global hotkey.
    /// </summary>
    /// <param name="id">The unique identifier of the hotkey to unregister.</param>
    /// <returns>
    /// <see langword="true"/> if the hotkey was successfully unregistered; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public abstract bool UnregisterHotKey(int id);
}