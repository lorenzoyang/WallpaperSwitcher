namespace WallpaperSwitcher.Core.GlobalHotKey;

/// <summary>
/// Defines virtual key codes for keyboard keys used in global hotkey registration.
/// These values correspond to Windows virtual key codes as defined in the Windows API.
/// </summary>
/// <remarks>
/// Virtual key codes are hardware-independent values that identify keyboard keys.
/// This enum includes commonly used keys for hotkey combinations including letters,
/// function keys, and special keys like Space, Enter, Escape, and Tab.
/// </remarks>
public enum VirtualKeys : uint
{
    A = 0x41,
    B = 0x42,
    C = 0x43,
    D = 0x44,
    E = 0x45,
    F = 0x46,
    G = 0x47,
    H = 0x48,
    I = 0x49,
    J = 0x4A,
    K = 0x4B,
    L = 0x4C,
    M = 0x4D,
    N = 0x4E,
    O = 0x4F,
    P = 0x50,
    Q = 0x51,
    R = 0x52,
    S = 0x53,
    T = 0x54,
    U = 0x55,
    V = 0x56,
    W = 0x57,
    X = 0x58,
    Y = 0x59,
    Z = 0x5A,
    F1 = 0x70,
    F2 = 0x71,
    F3 = 0x72,
    F4 = 0x73,
    F5 = 0x74,
    F6 = 0x75,
    F7 = 0x76,
    F8 = 0x77,
    F9 = 0x78,
    F10 = 0x79,
    F11 = 0x7A,
    F12 = 0x7B,
    Space = 0x20,
    Enter = 0x0D,
    Escape = 0x1B,
    Tab = 0x09
}