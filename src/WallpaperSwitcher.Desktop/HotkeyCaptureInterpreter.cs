using WallpaperSwitcher.Core.GlobalHotkey;
using Windows.Win32;

namespace WallpaperSwitcher.Desktop;

internal enum HotkeyCaptureStatus
{
    ManualInput,
    WaitingForPrimaryKey,
    Recorded,
    Unsupported
}

internal readonly record struct HotkeyCaptureResult(
    HotkeyCaptureStatus Status,
    Hotkey? Hotkey,
    string Message);

internal static class HotkeyCaptureInterpreter
{
    private const string WaitingMessage =
        "Keep holding the modifier key and press a letter (A-Z).";

    private const string RecordedMessage =
        "Hotkey recorded. Click Save to apply it.";

    private const string UnsupportedMessage =
        "Use Ctrl, Alt, Shift, or Win with a letter (A-Z).";

    public static HotkeyCaptureResult Interpret(
        Keys keyCode,
        Keys modifiers,
        bool isWindowsKeyPressed)
    {
        // Strip modifier flags and keep only the primary key code.
        keyCode &= Keys.KeyCode;

        if (IsModifierKey(keyCode))
        {
            return new HotkeyCaptureResult(
                HotkeyCaptureStatus.WaitingForPrimaryKey,
                null,
                WaitingMessage);
        }

        if (IsManualEditingKey(keyCode))
        {
            return new HotkeyCaptureResult(
                HotkeyCaptureStatus.ManualInput,
                null,
                string.Empty);
        }

        if (keyCode is >= Keys.A and <= Keys.Z)
        {
            var modifierKeys = GetModifierKeys(modifiers, isWindowsKeyPressed);
            if (modifierKeys == ModifierKeys.None)
            {
                // Let unmodified letters reach the text box so bindings can still be typed manually.
                return new HotkeyCaptureResult(
                    HotkeyCaptureStatus.ManualInput,
                    null,
                    string.Empty);
            }

            // WinForms and Win32 use the same virtual-key values for A-Z.
            var hotkey = new Hotkey(modifierKeys, (VirtualKeys)(uint)keyCode);
            return new HotkeyCaptureResult(
                HotkeyCaptureStatus.Recorded,
                hotkey,
                RecordedMessage);
        }

        return new HotkeyCaptureResult(
            HotkeyCaptureStatus.Unsupported,
            null,
            UnsupportedMessage);
    }

    public static bool IsWindowsKeyPressed()
    {
        // KeyEventArgs.Modifiers excludes Win, so query the left and right keys directly.
        const int keyPressedMask = 0x8000;

        return (PInvoke.GetKeyState((int)Keys.LWin) & keyPressedMask) != 0 ||
               (PInvoke.GetKeyState((int)Keys.RWin) & keyPressedMask) != 0;
    }

    private static ModifierKeys GetModifierKeys(Keys modifiers, bool isWindowsKeyPressed)
    {
        var modifierKeys = ModifierKeys.None;

        if ((modifiers & Keys.Control) == Keys.Control)
        {
            modifierKeys |= ModifierKeys.Ctrl;
        }

        if ((modifiers & Keys.Alt) == Keys.Alt)
        {
            modifierKeys |= ModifierKeys.Alt;
        }

        if ((modifiers & Keys.Shift) == Keys.Shift)
        {
            modifierKeys |= ModifierKeys.Shift;
        }

        if (isWindowsKeyPressed)
        {
            modifierKeys |= ModifierKeys.Win;
        }

        return modifierKeys;
    }

    private static bool IsModifierKey(Keys keyCode)
    {
        return keyCode is Keys.ControlKey or Keys.LControlKey or Keys.RControlKey
            or Keys.ShiftKey or Keys.LShiftKey or Keys.RShiftKey
            or Keys.Menu or Keys.LMenu or Keys.RMenu
            or Keys.LWin or Keys.RWin;
    }

    private static bool IsManualEditingKey(Keys keyCode)
    {
        // Keep textual editing and the '+' separator available while recording is active.
        return keyCode is Keys.Oemplus or Keys.Add
            or Keys.Back or Keys.Delete
            or Keys.Left or Keys.Right
            or Keys.Home or Keys.End;
    }
}
