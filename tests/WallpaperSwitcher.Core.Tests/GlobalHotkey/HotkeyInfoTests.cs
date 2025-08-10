using WallpaperSwitcher.Core.GlobalHotkey;

namespace WallpaperSwitcher.Core.Tests.GlobalHotkey;

public class HotkeyInfoTests
{
    [Test]
    public void Equals_IdenticalHotkeys_ReturnsTrue()
    {
        var hotkey1 = new HotkeyInfo
        {
            Id = 1,
            Hotkey = new Hotkey(ModifierKeys.Ctrl, VirtualKeys.A),
            Name = "Wallpaper Folder 1"
        };

        var hotkey2 = new HotkeyInfo
        {
            Id = 2,
            Hotkey = new Hotkey(ModifierKeys.Ctrl, VirtualKeys.A),
            Name = "Wallpaper Folder 2"
        };

        Assert.That(hotkey1, Is.EqualTo(hotkey2));
    }

    [Test]
    public void Equals_DifferentHotkeys_ReturnsFalse()
    {
        var hotkey1 = new HotkeyInfo
        {
            Id = 1,
            Hotkey = new Hotkey(ModifierKeys.Ctrl, VirtualKeys.A),
            Name = "Wallpaper Folder 1"
        };

        var hotkey2 = new HotkeyInfo
        {
            Id = 1,
            Hotkey = new Hotkey(ModifierKeys.Alt, VirtualKeys.B),
            Name = "Wallpaper Folder 1"
        };

        Assert.That(hotkey1, Is.Not.EqualTo(hotkey2));
    }

    [Test]
    public void GetHashCode_IdenticalHotkeys_ReturnsSameHashCode()
    {
        var hotkey1 = new HotkeyInfo
        {
            Id = 1,
            Hotkey = new Hotkey(ModifierKeys.Ctrl, VirtualKeys.A),
            Name = "Wallpaper Folder 1"
        };

        var hotkey2 = new HotkeyInfo
        {
            Id = 2,
            Hotkey = new Hotkey(ModifierKeys.Ctrl, VirtualKeys.A),
            Name = "Wallpaper Folder 2"
        };

        Assert.That(hotkey1.GetHashCode(), Is.EqualTo(hotkey2.GetHashCode()));
    }

    [Test]
    public void GetHashCode_DifferentHotkeys_ReturnsDifferentHashCode()
    {
        var hotkey1 = new HotkeyInfo
        {
            Id = 1,
            Hotkey = new Hotkey(ModifierKeys.Ctrl, VirtualKeys.A),
            Name = "Wallpaper Folder 1"
        };

        var hotkey2 = new HotkeyInfo
        {
            Id = 1,
            Hotkey = new Hotkey(ModifierKeys.Alt, VirtualKeys.B),
            Name = "Wallpaper Folder 1"
        };

        Assert.That(hotkey1.GetHashCode(), Is.Not.EqualTo(hotkey2.GetHashCode()));
    }

    [Test]
    public void Operators_ConsistentWithEquals()
    {
        var hotkey1 = new HotkeyInfo
        {
            Id = 1,
            Hotkey = new Hotkey(ModifierKeys.Ctrl, VirtualKeys.A),
            Name = "Wallpaper Folder 1"
        };

        var hotkey2 = new HotkeyInfo
        {
            Id = 2,
            Hotkey = new Hotkey(ModifierKeys.Ctrl, VirtualKeys.A),
            Name = "Wallpaper Folder 2"
        };

        var hotkey3 = new HotkeyInfo
        {
            Id = 1,
            Hotkey = new Hotkey(ModifierKeys.Alt, VirtualKeys.B),
            Name = "Wallpaper Folder 1"
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(hotkey1 == hotkey2, Is.True);
            Assert.That(hotkey1 != hotkey2, Is.False);
            Assert.That(hotkey1 == hotkey3, Is.False);
        }
    }

    [Test]
    public void Deconstruct_ReturnsCorrectValues()
    {
        var hotkeyInfo = new HotkeyInfo
        {
            Id = 1,
            Hotkey = new Hotkey(ModifierKeys.Ctrl, VirtualKeys.A),
            Name = "Wallpaper Folder 1"
        };

        var (id, hotkey, name) = hotkeyInfo;

        Assert.Multiple(() =>
        {
            Assert.That(id, Is.EqualTo(1));
            Assert.That(hotkey, Is.EqualTo(new Hotkey(ModifierKeys.Ctrl, VirtualKeys.A)));
            Assert.That(name, Is.EqualTo("Wallpaper Folder 1"));
        });
    }

    [Test]
    public void ToString_ReturnsCorrectValues()
    {
        var hotkeyInfo = new HotkeyInfo
        {
            Id = 1,
            Hotkey = new Hotkey(ModifierKeys.Ctrl, VirtualKeys.A),
            Name = "Wallpaper Folder 1"
        };

        var result = hotkeyInfo.ToString();

        Assert.That(result, Is.EqualTo("Ctrl+A"));
    }
}