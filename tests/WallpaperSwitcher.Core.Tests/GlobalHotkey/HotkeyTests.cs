using WallpaperSwitcher.Core.GlobalHotkey;

namespace WallpaperSwitcher.Core.Tests.GlobalHotkey;

public class HotkeyTests
{
    [TestCase(ModifierKeys.None, VirtualKeys.A, ExpectedResult = "A")]
    [TestCase(ModifierKeys.Shift | ModifierKeys.Alt, VirtualKeys.N, ExpectedResult = "Alt+Shift+N")]
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
    [TestCase("Control+Windows+n", ModifierKeys.Ctrl | ModifierKeys.Win, VirtualKeys.N)]
    [TestCase(" ctrl + alt + n ", ModifierKeys.Ctrl | ModifierKeys.Alt, VirtualKeys.N)]
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

    [TestCase("A")]
    [TestCase("Ctrl+None")]
    [TestCase("None+A")]
    [TestCase("Ctrl+Ctrl+A")]
    [TestCase("Ctrl+Control+A")]
    [TestCase("Ctrl+65")]
    [TestCase("Ctrl+F1")]
    [TestCase("Ctrl+")]
    [TestCase("")]
    [TestCase("   ")]
    public void TryParseFrom_InvalidInput_ReturnsFalseAndError(string hotkeyString)
    {
        var result = Hotkey.TryParseFrom(hotkeyString, out var hotkey, out var errorMessage);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(hotkey, Is.EqualTo(default(Hotkey)));
            Assert.That(errorMessage, Is.Not.Empty);
        }
    }
}
