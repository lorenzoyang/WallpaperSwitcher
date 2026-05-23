using WallpaperSwitcher.Core.Wallpaper;

namespace WallpaperSwitcher.Core.Tests.Wallpaper;

public class WallpaperHelperTests
{
    private string _testDirectory = string.Empty;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "WallpaperHelperTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
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
    public void GetImageCount_CountsSupportedExtensionsCaseInsensitively()
    {
        File.WriteAllText(Path.Combine(_testDirectory, "one.JPG"), string.Empty);
        File.WriteAllText(Path.Combine(_testDirectory, "two.dib"), string.Empty);
        File.WriteAllText(Path.Combine(_testDirectory, "three.jfif"), string.Empty);
        File.WriteAllText(Path.Combine(_testDirectory, "notes.txt"), string.Empty);

        var result = WallpaperHelper.GetImageCount(_testDirectory);

        Assert.That(result, Is.EqualTo(3));
    }

    [Test]
    public void IsValidWallpaper_WithSupportedUppercaseExtension_ReturnsTrue()
    {
        var wallpaper = Path.Combine(_testDirectory, "wallpaper.PNG");
        File.WriteAllText(wallpaper, string.Empty);

        var result = WallpaperHelper.IsValidWallpaper(wallpaper);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValidWallpaperFolder_WhenFolderDoesNotExist_ReturnsFalseWithMessage()
    {
        var missingFolder = Path.Combine(_testDirectory, "missing");

        var result = WallpaperHelper.IsValidWallpaperFolder(missingFolder, out var errorMessage);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(errorMessage, Is.EqualTo("The selected folder does not exist."));
        }
    }

    [Test]
    public void IsValidWallpaperFolder_WhenFolderHasNoImages_ReturnsFalseWithMessage()
    {
        File.WriteAllText(Path.Combine(_testDirectory, "notes.txt"), string.Empty);

        var result = WallpaperHelper.IsValidWallpaperFolder(_testDirectory, out var errorMessage);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(errorMessage, Is.EqualTo("The selected folder does not contain any supported image files."));
        }
    }
}
