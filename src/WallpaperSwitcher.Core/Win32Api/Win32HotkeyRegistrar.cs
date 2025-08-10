using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using WallpaperSwitcher.Core.GlobalHotkey;

namespace WallpaperSwitcher.Core.Win32Api;

/// <summary>
/// Implements global hotkey registration using the Win32 API.
/// </summary>
public class Win32HotkeyRegistrar(IntPtr hWnd) : HotkeyRegistrar(hWnd)
{
    /// <inheritdoc/>
    public override bool RegisterHotKey(int id, ModifierKeys modifiers, VirtualKeys key)
    {
        return PInvoke.RegisterHotKey(HWnd, id, (HOT_KEY_MODIFIERS)modifiers, (uint)key);
    }

    /// <inheritdoc/>
    public override bool UnregisterHotKey(int id)
    {
        return PInvoke.UnregisterHotKey(HWnd, id);
    }
}