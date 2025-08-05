namespace WallpaperSwitcher.Core.GlobalHotKey.Exceptions;

/// <summary>
/// 
/// </summary>
public class HotkeyBindingException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    public HotkeyBindingException()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public HotkeyBindingException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public HotkeyBindingException(string message, Exception inner)
        : base(message, inner)
    {
    }
}