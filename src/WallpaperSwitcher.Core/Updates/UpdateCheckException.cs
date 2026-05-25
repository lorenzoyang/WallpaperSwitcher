namespace WallpaperSwitcher.Core.Updates;

/// <summary>
/// Represents a controlled failure while checking for application updates.
/// </summary>
public sealed class UpdateCheckException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCheckException"/> class.
    /// </summary>
    /// <param name="message">The failure message.</param>
    public UpdateCheckException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCheckException"/> class.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <param name="innerException">The original exception that caused the failure.</param>
    public UpdateCheckException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
