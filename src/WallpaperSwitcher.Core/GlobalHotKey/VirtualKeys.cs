namespace WallpaperSwitcher.Core.GlobalHotKey;

/// <summary>
/// Defines virtual key codes for keyboard keys used in global hotkey registration.
/// These values correspond to Windows virtual key codes as defined in the Windows API.
/// Only simple letter keys (A–Z) are supported.
/// </summary>
/// <remarks>
/// Virtual key codes are hardware-independent values that identify keyboard keys.
/// This enum includes only simple alphabet keys because the program does not support
/// special keys such as Space, Enter, Escape, or Tab for hotkey usage.
/// </remarks>
public enum VirtualKeys : uint
{
    None = 0x00,
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
}