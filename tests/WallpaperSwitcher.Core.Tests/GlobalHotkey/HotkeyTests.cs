using WallpaperSwitcher.Core.GlobalHotkey;

namespace WallpaperSwitcher.Core.Tests.GlobalHotkey;

public class HotkeyTests
{
    [TestCase(ModifierKeys.None, VirtualKeys.A, ExpectedResult = "A")]
    [TestCase(ModifierKeys.Shift | ModifierKeys.Alt, VirtualKeys.N, ExpectedResult = "Shift+Alt+N")]
    [TestCase(ModifierKeys.None, VirtualKeys.None, ExpectedResult = "None")]
    public string ToString_ReturnsCorrectFormattedString(ModifierKeys modifiers, VirtualKeys key)
    {
        var hotkey = new Hotkey(modifiers, key);

        var result = hotkey.ToString();

        return result;
    }

    [TestCase("Ctrl+A", ModifierKeys.Ctrl, VirtualKeys.A)]
    [TestCase("Ctrl+Shift+N", ModifierKeys.Ctrl | ModifierKeys.Shift, VirtualKeys.N)]
    [TestCase("Shift+j", ModifierKeys.Shift, VirtualKeys.J)]
    public void TryParseFrom_ValidInput_ReturnsTrueAndCorrectHotkey(
        string hotkeyString, ModifierKeys modifiers, VirtualKeys key)
    {
        var expectedHotkey = new Hotkey(modifiers, key);
        var result = Hotkey.TryParseFrom(hotkeyString, out var hotkey, out var errorMessage);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(hotkey, Is.EqualTo(expectedHotkey));
            Assert.That(errorMessage, Is.Empty);
        }
    }
}