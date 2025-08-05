namespace WallpaperSwitcher.Core.GlobalHotKey.Exceptions;

/// <summary>
/// 
/// </summary>
public class HotkeyParsingException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    public string InvalidHotkeyText { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="invalidHotkeyText"></param>
    public HotkeyParsingException(string message, string invalidHotkeyText)
        : base(message)
    {
        InvalidHotkeyText = invalidHotkeyText;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    /// <param name="invalidHotkeyText"></param>
    public HotkeyParsingException(string message, Exception inner, string invalidHotkeyText)
        : base(message, inner)
    {
        InvalidHotkeyText = invalidHotkeyText;
    }
}