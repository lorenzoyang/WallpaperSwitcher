using WallpaperSwitcher.Core.GlobalHotkey;

namespace WallpaperSwitcher.Desktop.Tests;

public class HotkeyCaptureInterpreterTests
{
    [TestCase(Keys.ControlKey)]
    [TestCase(Keys.LControlKey)]
    [TestCase(Keys.RControlKey)]
    [TestCase(Keys.ShiftKey)]
    [TestCase(Keys.LShiftKey)]
    [TestCase(Keys.RShiftKey)]
    [TestCase(Keys.Menu)]
    [TestCase(Keys.LMenu)]
    [TestCase(Keys.RMenu)]
    [TestCase(Keys.LWin)]
    [TestCase(Keys.RWin)]
    public void Interpret_ModifierOnly_WaitsForPrimaryKey(Keys keyCode)
    {
        var result = HotkeyCaptureInterpreter.Interpret(keyCode, Keys.None, false);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(HotkeyCaptureStatus.WaitingForPrimaryKey));
            Assert.That(result.Hotkey, Is.Null);
            Assert.That(result.Message, Is.Not.Empty);
        });
    }

    [TestCase(Keys.A, Keys.Control, false, ModifierKeys.Ctrl, VirtualKeys.A)]
    [TestCase(Keys.Z, Keys.Alt, false, ModifierKeys.Alt, VirtualKeys.Z)]
    [TestCase(Keys.N, Keys.Shift, false, ModifierKeys.Shift, VirtualKeys.N)]
    [TestCase(Keys.W, Keys.None, true, ModifierKeys.Win, VirtualKeys.W)]
    [TestCase(Keys.B, Keys.Control | Keys.Alt | Keys.Shift, true,
        ModifierKeys.Ctrl | ModifierKeys.Alt | ModifierKeys.Shift | ModifierKeys.Win,
        VirtualKeys.B)]
    public void Interpret_SupportedCombination_RecordsCanonicalHotkey(
        Keys keyCode,
        Keys modifiers,
        bool isWindowsKeyPressed,
        ModifierKeys expectedModifiers,
        VirtualKeys expectedKey)
    {
        var result = HotkeyCaptureInterpreter.Interpret(keyCode, modifiers, isWindowsKeyPressed);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(HotkeyCaptureStatus.Recorded));
            Assert.That(result.Hotkey, Is.EqualTo(new Hotkey(expectedModifiers, expectedKey)));
            Assert.That(result.Message, Is.Not.Empty);
        });
    }

    [Test]
    public void Interpret_AllModifiers_FormatsInCanonicalOrder()
    {
        var result = HotkeyCaptureInterpreter.Interpret(
            Keys.Z,
            Keys.Control | Keys.Alt | Keys.Shift,
            true);

        Assert.That(result.Hotkey?.ToString(), Is.EqualTo("Ctrl+Alt+Shift+Win+Z"));
    }

    [TestCase(Keys.A)]
    [TestCase(Keys.Z)]
    public void Interpret_LetterWithoutModifier_AllowsManualInput(Keys keyCode)
    {
        var result = HotkeyCaptureInterpreter.Interpret(keyCode, Keys.None, false);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(HotkeyCaptureStatus.ManualInput));
            Assert.That(result.Hotkey, Is.Null);
        });
    }

    [TestCase(Keys.Oemplus)]
    [TestCase(Keys.Add)]
    [TestCase(Keys.Back)]
    [TestCase(Keys.Delete)]
    [TestCase(Keys.Left)]
    [TestCase(Keys.Right)]
    [TestCase(Keys.Home)]
    [TestCase(Keys.End)]
    public void Interpret_TextEditingKey_AllowsManualInput(Keys keyCode)
    {
        var result = HotkeyCaptureInterpreter.Interpret(keyCode, Keys.Shift, false);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(HotkeyCaptureStatus.ManualInput));
            Assert.That(result.Hotkey, Is.Null);
        });
    }

    [TestCase(Keys.D0)]
    [TestCase(Keys.D9)]
    [TestCase(Keys.NumPad0)]
    [TestCase(Keys.NumPad9)]
    [TestCase(Keys.F1)]
    [TestCase(Keys.F12)]
    [TestCase(Keys.Space)]
    [TestCase(Keys.Up)]
    [TestCase(Keys.Down)]
    [TestCase(Keys.Tab)]
    [TestCase(Keys.Enter)]
    public void Interpret_UnsupportedPrimaryKey_ReturnsUnsupported(Keys keyCode)
    {
        var result = HotkeyCaptureInterpreter.Interpret(keyCode, Keys.Control, false);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(HotkeyCaptureStatus.Unsupported));
            Assert.That(result.Hotkey, Is.Null);
            Assert.That(result.Message, Does.Contain("A-Z"));
        });
    }

    [TestCase(Keys.D1)]
    [TestCase(Keys.F5)]
    public void Interpret_UnsupportedPrimaryKeyWithoutModifier_ReturnsUnsupported(Keys keyCode)
    {
        var result = HotkeyCaptureInterpreter.Interpret(keyCode, Keys.None, false);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(HotkeyCaptureStatus.Unsupported));
            Assert.That(result.Hotkey, Is.Null);
            Assert.That(result.Message, Does.Contain("A-Z"));
        });
    }
}
