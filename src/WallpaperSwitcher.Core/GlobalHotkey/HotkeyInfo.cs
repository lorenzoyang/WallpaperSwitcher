namespace WallpaperSwitcher.Core.GlobalHotkey;

/// <summary>
/// Represents information about a registered global hotkey, including its identifier, key combination, and display name.
/// </summary>
public sealed class HotkeyInfo : IEquatable<HotkeyInfo>
{
    /// <summary>
    /// Gets the unique identifier for the hotkey.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the hotkey combination associated with this instance.
    /// </summary>
    public required Hotkey Hotkey { get; init; }

    /// <summary>
    /// Gets the display name of the hotkey.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Returns a string representation of the hotkey.
    /// </summary>
    /// <returns>A string that represents the <see cref="Hotkey"/>.</returns>
    public override string ToString() => Hotkey.ToString();

    /// <summary>
    /// Deconstructs the current instance into its component values.
    /// </summary>
    /// <param name="id">The unique identifier of the hotkey.</param>
    /// <param name="hotkey">The hotkey combination.</param>
    /// <param name="name">The display name of the hotkey.</param>
    public void Deconstruct(out int id, out Hotkey hotkey, out string name)
    {
        id = Id;
        hotkey = Hotkey;
        name = Name;
    }

    #region Equality Members

    /// <summary>
    /// Determines whether the specified <see cref="HotkeyInfo"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="HotkeyInfo"/> to compare with the current instance.</param>
    /// <returns><c>true</c> if the hotkey combinations are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(HotkeyInfo? other)
    {
        if (ReferenceEquals(null, other)) return false;
        return ReferenceEquals(this, other) || Hotkey.Equals(other.Hotkey);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return Equals(obj as HotkeyInfo);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Hotkey.GetHashCode();
    }

    /// <summary>
    /// Determines whether two specified <see cref="HotkeyInfo"/> objects are equal.
    /// </summary>
    /// <param name="left">The first <see cref="HotkeyInfo"/> to compare.</param>
    /// <param name="right">The second <see cref="HotkeyInfo"/> to compare.</param>
    /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(HotkeyInfo? left, HotkeyInfo? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two specified <see cref="HotkeyInfo"/> objects are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="HotkeyInfo"/> to compare.</param>
    /// <param name="right">The second <see cref="HotkeyInfo"/> to compare.</param>
    /// <returns><c>true</c> if the objects are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(HotkeyInfo? left, HotkeyInfo? right)
    {
        return !Equals(left, right);
    }

    #endregion Equality Members
}