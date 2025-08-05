namespace WallpaperSwitcher.Core.GlobalHotKey.Exceptions;

/// <summary>
/// 
/// </summary>
public class HotkeyDuplicateBindingException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    public HotkeyInfo ExistingHotkey { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="existingHotkeyInfo"></param>
    public HotkeyDuplicateBindingException(string message, HotkeyInfo existingHotkeyInfo)
        : base(message)
    {
        ExistingHotkey = existingHotkeyInfo;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    /// <param name="existingHotkeyInfo"></param>
    public HotkeyDuplicateBindingException(string message, Exception inner, HotkeyInfo existingHotkeyInfo)
        : base(message, inner)
    {
        ExistingHotkey = existingHotkeyInfo;
    }
}