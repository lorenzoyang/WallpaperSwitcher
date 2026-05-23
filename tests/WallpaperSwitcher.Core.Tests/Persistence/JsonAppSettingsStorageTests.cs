using WallpaperSwitcher.Core.Persistence;

namespace WallpaperSwitcher.Core.Tests.Persistence;

public class JsonAppSettingsStorageTests
{
    private string _testDirectory = string.Empty;
    private string _testFilePath = string.Empty;
    private JsonAppSettingsStorage _storage = null!;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "AppSettingsStorageTests", Guid.NewGuid().ToString());
        _testFilePath = Path.Combine(_testDirectory, "settings.json");
        _storage = new JsonAppSettingsStorage(_testFilePath);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Test]
    public void Constructor_WithDefaultLocation_UsesAppDataSettingsFile()
    {
        var storage = new JsonAppSettingsStorage();

        Assert.That(storage.Location, Is.EqualTo(AppDataPaths.SettingsFile));
    }

    [Test]
    public void Load_WhenFileDoesNotExist_ReturnsDefaultSettings()
    {
        var settings = _storage.Load();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(settings.WallpaperFolders, Is.Empty);
            Assert.That(settings.LastSelectedFolder, Is.Empty);
            Assert.That(settings.SelectedModeIndex, Is.Zero);
            Assert.That(settings.HasShownTrayTip, Is.False);
            Assert.That(settings.LaunchAtStartup, Is.False);
        }
    }

    [Test]
    public void Save_AndLoad_RoundTripsSettings()
    {
        var expected = new AppSettings
        {
            WallpaperFolders = ["C:\\Wallpapers", "D:\\Images"],
            LastSelectedFolder = "D:\\Images",
            SelectedModeIndex = 1,
            HasShownTrayTip = true,
            LaunchAtStartup = true
        };

        _storage.Save(expected);

        var actual = _storage.Load();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(actual.WallpaperFolders, Is.EqualTo(expected.WallpaperFolders));
            Assert.That(actual.LastSelectedFolder, Is.EqualTo(expected.LastSelectedFolder));
            Assert.That(actual.SelectedModeIndex, Is.EqualTo(expected.SelectedModeIndex));
            Assert.That(actual.HasShownTrayTip, Is.EqualTo(expected.HasShownTrayTip));
            Assert.That(actual.LaunchAtStartup, Is.EqualTo(expected.LaunchAtStartup));
        }
    }

    [Test]
    public async Task SaveAsync_AndLoadAsync_RoundTripsSettings()
    {
        var expected = new AppSettings
        {
            WallpaperFolders = ["C:\\Wallpapers"],
            LastSelectedFolder = "C:\\Wallpapers",
            SelectedModeIndex = 1
        };

        await _storage.SaveAsync(expected);

        var actual = await _storage.LoadAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(actual.WallpaperFolders, Is.EqualTo(expected.WallpaperFolders));
            Assert.That(actual.LastSelectedFolder, Is.EqualTo(expected.LastSelectedFolder));
            Assert.That(actual.SelectedModeIndex, Is.EqualTo(expected.SelectedModeIndex));
        }
    }

    [Test]
    public void Load_WithInvalidJson_ReturnsDefaultSettings()
    {
        Directory.CreateDirectory(_testDirectory);
        File.WriteAllText(_testFilePath, "invalid json content");

        var settings = _storage.Load();

        Assert.That(settings.WallpaperFolders, Is.Empty);
    }

    [Test]
    public void Load_WithNullJsonProperties_NormalizesDefaultValues()
    {
        Directory.CreateDirectory(_testDirectory);
        File.WriteAllText(_testFilePath,
            """
            {
              "WallpaperFolders": null,
              "LastSelectedFolder": null,
              "SelectedModeIndex": 1
            }
            """);

        var settings = _storage.Load();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(settings.WallpaperFolders, Is.Empty);
            Assert.That(settings.LastSelectedFolder, Is.Empty);
            Assert.That(settings.SelectedModeIndex, Is.EqualTo(1));
        }
    }

    [Test]
    public async Task LoadAsync_WithPartialJson_NormalizesDefaultValues()
    {
        Directory.CreateDirectory(_testDirectory);
        await File.WriteAllTextAsync(_testFilePath,
            """
            {
              "SelectedModeIndex": 1
            }
            """);

        var settings = await _storage.LoadAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(settings.WallpaperFolders, Is.Empty);
            Assert.That(settings.LastSelectedFolder, Is.Empty);
            Assert.That(settings.SelectedModeIndex, Is.EqualTo(1));
        }
    }
}
