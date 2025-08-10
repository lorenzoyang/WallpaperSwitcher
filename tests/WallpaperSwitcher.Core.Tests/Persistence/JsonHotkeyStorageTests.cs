using WallpaperSwitcher.Core.GlobalHotkey;
using WallpaperSwitcher.Core.Persistence;

namespace WallpaperSwitcher.Core.Tests.Persistence;

public class JsonHotkeyStorageTests
{
    private string _testDirectory;
    private string _testFilePath;
    private JsonHotkeyStorage _jsonHotkeyStorage;

    [SetUp]
    public void SetUp()
    {
        // Create a unique test directory for each test
        _testDirectory = Path.Combine(Path.GetTempPath(), "HotkeyStorageTests", Guid.NewGuid().ToString());
        _testFilePath = Path.Combine(_testDirectory, "test-hotkeys.json");
        _jsonHotkeyStorage = new JsonHotkeyStorage(_testFilePath);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up test directory after each test
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Test]
    public void Constructor_WithDefaultLocation_SetsLocationCorrectly()
    {
        var storage = new JsonHotkeyStorage();
        var expectedPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WallpaperSwitcher",
            "hotkeys.json");

        Assert.That(storage.Location, Is.EqualTo(expectedPath));
    }

    [Test]
    public void Constructor_WithCustomLocation_SetsLocationCorrectly()
    {
        const string customPath = "/custom/path/hotkeys.json";

        var storage = new JsonHotkeyStorage(customPath);

        Assert.That(storage.Location, Is.EqualTo(customPath));
    }

    [Test]
    public async Task SaveAsync_WithValidHotkeys_CreatesFileWithCorrectContent()
    {
        var hotkeys = new[]
        {
            new HotkeyInfo
            {
                Id = 1,
                Hotkey = new Hotkey(ModifierKeys.Alt, VirtualKeys.A),
                Name = "Test Hotkey 1"
            },
            new HotkeyInfo
            {
                Id = 2,
                Hotkey = new Hotkey(ModifierKeys.Alt, VirtualKeys.B),
                Name = "Test Hotkey 2"
            }
        };

        // Act
        await _jsonHotkeyStorage.SaveAsync(hotkeys);

        Assert.That(File.Exists(_testFilePath), Is.True);

        // Verify content can be read back
        var loadedHotkeys = (await _jsonHotkeyStorage.LoadAsync()).ToArray();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(loadedHotkeys, Has.Length.EqualTo(hotkeys.Length));
            Assert.That(loadedHotkeys[0], Is.EqualTo(hotkeys[0]));
            Assert.That(loadedHotkeys[1], Is.EqualTo(hotkeys[1]));
        }
    }

    [Test]
    public void Save_WithValidHotkeys_CreatesFileWithCorrectContent()
    {
        var hotkeys = new[]
        {
            new HotkeyInfo
            {
                Id = 1,
                Hotkey = new Hotkey(ModifierKeys.Ctrl, VirtualKeys.A),
                Name = "Test Hotkey 1"
            },
            new HotkeyInfo
            {
                Id = 2,
                Hotkey = new Hotkey(ModifierKeys.Ctrl, VirtualKeys.B),
                Name = "Test Hotkey 2"
            }
        };

        _jsonHotkeyStorage.Save(hotkeys);

        Assert.That(File.Exists(_testFilePath), Is.True);

        // Verify content can be read back
        var loadedHotkeys = _jsonHotkeyStorage.Load().ToArray();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(loadedHotkeys, Has.Length.EqualTo(hotkeys.Length));
            Assert.That(loadedHotkeys[0], Is.EqualTo(hotkeys[0]));
            Assert.That(loadedHotkeys[1], Is.EqualTo(hotkeys[1]));
        }
    }

    [Test]
    public async Task SaveAsync_CreatesFileIfNotExists()
    {
        var hotkeys = new[]
        {
            new HotkeyInfo
            {
                Id = 1,
                Hotkey = new Hotkey(ModifierKeys.Alt, VirtualKeys.A),
                Name = "Test"
            }
        };

        await _jsonHotkeyStorage.SaveAsync(hotkeys);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Directory.Exists(_testDirectory), Is.True);
            Assert.That(File.Exists(_testFilePath), Is.True);
        }
    }

    [Test]
    public void Save_CreatesFileIfNotExists()
    {
        var hotkeys = new[]
        {
            new HotkeyInfo
            {
                Id = 1,
                Hotkey = new Hotkey(ModifierKeys.Alt, VirtualKeys.A),
                Name = "Test"
            }
        };

        _jsonHotkeyStorage.Save(hotkeys);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Directory.Exists(_testDirectory), Is.True);
            Assert.That(File.Exists(_testFilePath), Is.True);
        }
    }

    [Test]
    public async Task LoadAsync_WhenFileDoesNotExist_ReturnsEmptyArray()
    {
        var result = (await _jsonHotkeyStorage.LoadAsync()).ToArray();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Load_WhenFileDoesNotExist_ReturnsEmptyArray()
    {
        var result = _jsonHotkeyStorage.Load().ToArray();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task LoadAsync_WithInvalidJson_ReturnsEmptyArray()
    {
        Directory.CreateDirectory(_testDirectory);
        await File.WriteAllTextAsync(_testFilePath, "invalid json content");

        var result = (await _jsonHotkeyStorage.LoadAsync()).ToArray();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Load_WithInvalidJson_ReturnsEmptyArray()
    {
        Directory.CreateDirectory(_testDirectory);
        File.WriteAllText(_testFilePath, "invalid json content");

        var result = _jsonHotkeyStorage.Load().ToArray();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task LoadAsync_WithEmptyFile_ReturnsEmptyArray()
    {
        Directory.CreateDirectory(_testDirectory);
        await File.WriteAllTextAsync(_testFilePath, string.Empty);

        var result = await _jsonHotkeyStorage.LoadAsync();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Load_WithEmptyFile_ReturnsEmptyArray()
    {
        Directory.CreateDirectory(_testDirectory);
        File.WriteAllText(_testFilePath, string.Empty);

        var result = _jsonHotkeyStorage.Load();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task LoadAsync_WithNullJson_ReturnsEmptyArray()
    {
        Directory.CreateDirectory(_testDirectory);
        await File.WriteAllTextAsync(_testFilePath, "null");

        var result = await _jsonHotkeyStorage.LoadAsync();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Load_WithNullJson_ReturnsEmptyArray()
    {
        Directory.CreateDirectory(_testDirectory);
        File.WriteAllText(_testFilePath, "null");

        var result = _jsonHotkeyStorage.Load();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task SaveAsync_OverwritesExistingFile()
    {
        var firstSet = new[]
        {
            new HotkeyInfo { Id = 1, Hotkey = new Hotkey(ModifierKeys.Alt, VirtualKeys.A), Name = "First" }
        };
        var secondSet = new[]
        {
            new HotkeyInfo { Id = 2, Hotkey = new Hotkey(ModifierKeys.Ctrl, VirtualKeys.B), Name = "Second" }
        };

        await _jsonHotkeyStorage.SaveAsync(firstSet);
        await _jsonHotkeyStorage.SaveAsync(secondSet);

        var result = (await _jsonHotkeyStorage.LoadAsync()).ToArray();
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("Second"));
    }

    [Test]
    public void Save_OverwritesExistingFile()
    {
        var firstSet = new[]
        {
            new HotkeyInfo { Id = 1, Hotkey = new Hotkey(ModifierKeys.Alt, VirtualKeys.A), Name = "First" }
        };
        var secondSet = new[]
        {
            new HotkeyInfo { Id = 2, Hotkey = new Hotkey(ModifierKeys.Ctrl, VirtualKeys.B), Name = "Second" }
        };

        _jsonHotkeyStorage.Save(firstSet);
        _jsonHotkeyStorage.Save(secondSet);

        var result = _jsonHotkeyStorage.Load().ToArray();
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("Second"));
    }
}