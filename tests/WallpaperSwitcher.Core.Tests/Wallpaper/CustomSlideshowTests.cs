using WallpaperSwitcher.Core.Wallpaper;

namespace WallpaperSwitcher.Core.Tests.Wallpaper;

public class CustomSlideshowTests
{
    private string _testDirectory = string.Empty;
    private string _currentWallpaper = string.Empty;
    private readonly List<string> _attemptedWallpapers = [];
    private readonly HashSet<string> _failedWallpapers = new(StringComparer.OrdinalIgnoreCase);

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "CustomSlideshowTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _currentWallpaper = string.Empty;
        _attemptedWallpapers.Clear();
        _failedWallpapers.Clear();
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [TestCase("B.jpg", new[] { "E.jpg", "A.jpg", "B.jpg", "C.jpg" })]
    [TestCase("D.jpg", new[] { "D.jpg", "E.jpg", "A.jpg", "C.jpg" })]
    public void AdvanceForwardSlideshow_WhenImageIsAdded_PreservesPositionAndReachesNewImage(
        string addedName, string[] expectedNames)
    {
        CreateFile("A.jpg");
        _currentWallpaper = CreateFile("C.jpg");
        CreateFile("E.jpg");
        var slideshow = CreateSlideshow();
        slideshow.SetSlideShow(_testDirectory);
        CreateFile(addedName);

        foreach (var expectedName in expectedNames)
        {
            slideshow.AdvanceForwardSlideshow();
            Assert.That(_currentWallpaper, Is.EqualTo(Path.Combine(_testDirectory, expectedName)));
        }

        Assert.That(_attemptedWallpapers, Has.Count.EqualTo(expectedNames.Length));
    }

    [Test]
    public void AdvanceForwardSlideshow_WhenSingleImageGainsAnotherImage_SwitchesToNewImage()
    {
        _currentWallpaper = CreateFile("A.jpg");
        var slideshow = CreateSlideshow();
        slideshow.SetSlideShow(_testDirectory);
        var addedWallpaper = CreateFile("B.jpg");

        slideshow.AdvanceForwardSlideshow();

        Assert.That(_attemptedWallpapers, Is.EqualTo(new[] { addedWallpaper }));
    }

    [Test]
    public void AdvanceForwardSlideshow_WhenCurrentImageIsLast_WrapsToFirstImage()
    {
        var firstWallpaper = CreateFile("A.jpg");
        _currentWallpaper = CreateFile("B.jpg");
        var slideshow = CreateSlideshow();
        slideshow.SetSlideShow(_testDirectory);

        slideshow.AdvanceForwardSlideshow();

        Assert.That(_attemptedWallpapers, Is.EqualTo(new[] { firstWallpaper }));
    }

    [Test]
    public void AdvanceForwardSlideshow_WhenOnlyCurrentImageExists_DoesNotSetWallpaperAgain()
    {
        _currentWallpaper = CreateFile("A.jpg");
        var slideshow = CreateSlideshow();
        slideshow.SetSlideShow(_testDirectory);

        slideshow.AdvanceForwardSlideshow();

        Assert.That(_attemptedWallpapers, Is.Empty);
    }

    [Test]
    public void AdvanceForwardSlideshow_WhenCurrentImageIsDeleted_StartsFromFirstImage()
    {
        var firstWallpaper = CreateFile("A.jpg");
        _currentWallpaper = CreateFile("B.jpg");
        var slideshow = CreateSlideshow();
        slideshow.SetSlideShow(_testDirectory);
        File.Delete(_currentWallpaper);

        slideshow.AdvanceForwardSlideshow();

        Assert.That(_attemptedWallpapers, Is.EqualTo(new[] { firstWallpaper }));
    }

    [TestCase("A.jpg", "E.jpg")]
    [TestCase("E.jpg", "A.jpg")]
    public void AdvanceForwardSlideshow_WhenAnotherImageIsDeleted_UsesRefreshedOrder(
        string deletedName, string expectedName)
    {
        CreateFile("A.jpg");
        _currentWallpaper = CreateFile("C.jpg");
        CreateFile("E.jpg");
        var slideshow = CreateSlideshow();
        slideshow.SetSlideShow(_testDirectory);
        File.Delete(Path.Combine(_testDirectory, deletedName));

        slideshow.AdvanceForwardSlideshow();

        Assert.That(_attemptedWallpapers, Is.EqualTo(new[] { Path.Combine(_testDirectory, expectedName) }));
    }

    [Test]
    public void AdvanceForwardSlideshow_WhenCurrentImageIsRenamed_UsesNewPath()
    {
        _currentWallpaper = CreateFile("A.jpg");
        var slideshow = CreateSlideshow();
        slideshow.SetSlideShow(_testDirectory);
        var renamedWallpaper = Path.Combine(_testDirectory, "B.jpg");
        File.Move(_currentWallpaper, renamedWallpaper);

        slideshow.AdvanceForwardSlideshow();

        Assert.That(_attemptedWallpapers, Is.EqualTo(new[] { renamedWallpaper }));
    }

    [Test]
    public void AdvanceForwardSlideshow_WhenFolderIsEmptiedAndRefilled_ResumesWithoutReselection()
    {
        var originalWallpaper = CreateFile("A.jpg");
        _currentWallpaper = originalWallpaper;
        var slideshow = CreateSlideshow();
        slideshow.SetSlideShow(_testDirectory);
        File.Delete(originalWallpaper);

        slideshow.AdvanceForwardSlideshow();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_currentWallpaper, Is.EqualTo(originalWallpaper));
            Assert.That(_attemptedWallpapers, Is.Empty);
        }

        var addedWallpaper = CreateFile("B.jpg");
        slideshow.AdvanceForwardSlideshow();

        Assert.That(_attemptedWallpapers, Is.EqualTo(new[] { addedWallpaper }));
    }

    [Test]
    public void AdvanceForwardSlideshow_WhenDirectoryDisappearsAndReturns_RetriesSelectedFolder()
    {
        var originalWallpaper = CreateFile("A.jpg");
        _currentWallpaper = originalWallpaper;
        var slideshow = CreateSlideshow();
        slideshow.SetSlideShow(_testDirectory);
        File.Delete(originalWallpaper);
        Directory.Delete(_testDirectory);

        slideshow.AdvanceForwardSlideshow();
        Assert.That(_attemptedWallpapers, Is.Empty);

        Directory.CreateDirectory(_testDirectory);
        var addedWallpaper = CreateFile("B.jpg");
        slideshow.AdvanceForwardSlideshow();

        Assert.That(_attemptedWallpapers, Is.EqualTo(new[] { addedWallpaper }));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void AdvanceForwardSlideshow_WhenEnumerationFailsAfterYielding_DoesNotSwitchAndRecovers(
        bool accessDenied)
    {
        _currentWallpaper = CreateFile("A.jpg");
        var nextWallpaper = CreateFile("C.jpg");
        var failEnumeration = false;
        var partialWallpaper = Path.Combine(_testDirectory, "B.jpg");
        var slideshow = CreateSlideshow(folder => failEnumeration
            ? EnumerateThenFail(partialWallpaper, accessDenied)
            : WallpaperHelper.EnumerateWallpaperFiles(folder));
        slideshow.SetSlideShow(_testDirectory);
        CreateFile("B.jpg");
        failEnumeration = true;

        slideshow.AdvanceForwardSlideshow();
        Assert.That(_attemptedWallpapers, Is.Empty);

        File.Delete(partialWallpaper);
        failEnumeration = false;
        slideshow.AdvanceForwardSlideshow();

        Assert.That(_attemptedWallpapers, Is.EqualTo(new[] { nextWallpaper }));
    }

    [Test]
    public void AdvanceForwardSlideshow_WhenImageFails_SkipsItAndRetriesOnLaterCycle()
    {
        var firstWallpaper = CreateFile("A.jpg");
        _currentWallpaper = firstWallpaper;
        var failedWallpaper = CreateFile("B.jpg");
        var nextWallpaper = CreateFile("C.jpg");
        _failedWallpapers.Add(failedWallpaper);
        var slideshow = CreateSlideshow();
        slideshow.SetSlideShow(_testDirectory);

        slideshow.AdvanceForwardSlideshow();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_attemptedWallpapers, Is.EqualTo(new[] { failedWallpaper, nextWallpaper }));
            Assert.That(_currentWallpaper, Is.EqualTo(nextWallpaper));
        }

        _failedWallpapers.Clear();
        slideshow.AdvanceForwardSlideshow();
        slideshow.AdvanceForwardSlideshow();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_attemptedWallpapers,
                Is.EqualTo(new[] { failedWallpaper, nextWallpaper, firstWallpaper, failedWallpaper }));
            Assert.That(_currentWallpaper, Is.EqualTo(failedWallpaper));
        }
    }

    [Test]
    public void AdvanceForwardSlideshow_WhenAllOtherImagesFail_TriesOneCycleAndPreservesPosition()
    {
        var firstWallpaper = CreateFile("A.jpg");
        var originalWallpaper = CreateFile("B.jpg");
        _currentWallpaper = originalWallpaper;
        var nextWallpaper = CreateFile("C.jpg");
        _failedWallpapers.UnionWith([firstWallpaper, nextWallpaper]);
        var slideshow = CreateSlideshow();
        slideshow.SetSlideShow(_testDirectory);

        slideshow.AdvanceForwardSlideshow();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_attemptedWallpapers, Is.EqualTo(new[] { nextWallpaper, firstWallpaper }));
            Assert.That(_currentWallpaper, Is.EqualTo(originalWallpaper));
        }

        _failedWallpapers.Clear();
        _attemptedWallpapers.Clear();
        slideshow.AdvanceForwardSlideshow();

        Assert.That(_attemptedWallpapers, Is.EqualTo(new[] { nextWallpaper }));
    }

    [Test]
    public void AdvanceForwardSlideshow_WhenImageDisappearsAfterScanning_SkipsMissingImage()
    {
        _currentWallpaper = CreateFile("A.jpg");
        var removedWallpaper = CreateFile("B.jpg");
        var nextWallpaper = CreateFile("C.jpg");
        var deleteAfterScanning = false;
        var slideshow = CreateSlideshow(folder =>
        {
            var wallpapers = WallpaperHelper.EnumerateWallpaperFiles(folder).ToList();
            if (deleteAfterScanning)
            {
                File.Delete(removedWallpaper);
            }

            return wallpapers;
        });
        slideshow.SetSlideShow(_testDirectory);
        deleteAfterScanning = true;

        slideshow.AdvanceForwardSlideshow();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_attemptedWallpapers, Is.EqualTo(new[] { removedWallpaper, nextWallpaper }));
            Assert.That(_currentWallpaper, Is.EqualTo(nextWallpaper));
        }
    }

    [Test]
    public void AdvanceForwardSlideshow_WhenNewFilesIncludeUnsupportedOrNestedImages_IgnoresThem()
    {
        _currentWallpaper = CreateFile("A.jpg");
        var slideshow = CreateSlideshow();
        slideshow.SetSlideShow(_testDirectory);
        CreateFile("B.txt");
        CreateFile(Path.Combine("nested", "C.jpg"));
        var addedWallpaper = CreateFile("D.PNG");

        slideshow.AdvanceForwardSlideshow();

        Assert.That(_attemptedWallpapers, Is.EqualTo(new[] { addedWallpaper }));
    }

    [Test]
    public void SetSlideShow_WhenCurrentPathDiffersInCase_PreservesWallpaperAndPosition()
    {
        CreateFile("A.jpg");
        _currentWallpaper = CreateFile("B.jpg").ToUpperInvariant();
        var nextWallpaper = CreateFile("C.jpg");
        var slideshow = CreateSlideshow();

        slideshow.SetSlideShow(_testDirectory);
        Assert.That(_attemptedWallpapers, Is.Empty);

        slideshow.AdvanceForwardSlideshow();

        Assert.That(_attemptedWallpapers, Is.EqualTo(new[] { nextWallpaper }));
    }

    [Test]
    public void SetSlideShow_WhenCurrentWallpaperIsOutsideFolder_StartsWithFirstUsableImage()
    {
        var firstWallpaper = CreateFile("A.jpg");
        var nextWallpaper = CreateFile("B.jpg");
        _failedWallpapers.Add(firstWallpaper);
        var slideshow = CreateSlideshow();

        slideshow.SetSlideShow(_testDirectory);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_attemptedWallpapers, Is.EqualTo(new[] { firstWallpaper, nextWallpaper }));
            Assert.That(_currentWallpaper, Is.EqualTo(nextWallpaper));
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void SetSlideShow_WhenFolderStartsEmptyOrMissing_NextFindsFirstAddedImage(bool missing)
    {
        if (missing)
        {
            Directory.Delete(_testDirectory);
        }

        var slideshow = CreateSlideshow();
        slideshow.SetSlideShow(_testDirectory);
        Assert.That(_attemptedWallpapers, Is.Empty);
        Directory.CreateDirectory(_testDirectory);
        var addedWallpaper = CreateFile("A.jpg");

        slideshow.AdvanceForwardSlideshow();

        Assert.That(_attemptedWallpapers, Is.EqualTo(new[] { addedWallpaper }));
    }

    [Test]
    public void SetSlideShow_WhenSameFolderIsSelectedAgain_PreservesCurrentWallpaper()
    {
        CreateFile("A.jpg");
        var nextWallpaper = CreateFile("B.jpg");
        var slideshow = CreateSlideshow();
        slideshow.SetSlideShow(_testDirectory);
        slideshow.AdvanceForwardSlideshow();
        _attemptedWallpapers.Clear();

        slideshow.SetSlideShow(_testDirectory);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_attemptedWallpapers, Is.Empty);
            Assert.That(_currentWallpaper, Is.EqualTo(nextWallpaper));
        }
    }

    [Test]
    public void SetSlideShow_WhenSwitchingToMissingFolder_DoesNotAdvancePreviousFolder()
    {
        _currentWallpaper = CreateFile("A.jpg");
        CreateFile("B.jpg");
        var slideshow = CreateSlideshow();
        slideshow.SetSlideShow(_testDirectory);
        var newFolder = Path.Combine(_testDirectory, "new");

        slideshow.SetSlideShow(newFolder);
        slideshow.AdvanceForwardSlideshow();
        Assert.That(_attemptedWallpapers, Is.Empty);

        var addedWallpaper = CreateFile(Path.Combine("new", "C.jpg"));
        slideshow.AdvanceForwardSlideshow();

        Assert.That(_attemptedWallpapers, Is.EqualTo(new[] { addedWallpaper }));
    }

    [TestCase("")]
    [TestCase(" ")]
    public void SetSlideShow_WhenSelectionIsCleared_NextDoesNotScanOrSwitch(string folder)
    {
        _currentWallpaper = CreateFile("A.jpg");
        var scanCount = 0;
        var slideshow = CreateSlideshow(path =>
        {
            scanCount++;
            return WallpaperHelper.EnumerateWallpaperFiles(path);
        });
        slideshow.SetSlideShow(_testDirectory);
        var scansBeforeClear = scanCount;

        slideshow.SetSlideShow(folder);
        CreateFile("B.jpg");
        slideshow.AdvanceForwardSlideshow();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(scanCount, Is.EqualTo(scansBeforeClear));
            Assert.That(_attemptedWallpapers, Is.Empty);
        }
    }

    [Test]
    public void AdvanceForwardSlideshow_WhenNoFolderIsSelected_DoesNothing()
    {
        var slideshow = CreateSlideshow(_ => throw new InvalidOperationException("No folder was selected."));

        slideshow.AdvanceForwardSlideshow();

        Assert.That(_attemptedWallpapers, Is.Empty);
    }

    [Test]
    public void AdvanceForwardSlideshow_WhenEnumerationThrowsUnexpectedException_PropagatesIt()
    {
        _currentWallpaper = CreateFile("A.jpg");
        var failEnumeration = false;
        var slideshow = CreateSlideshow(folder => failEnumeration
            ? throw new InvalidOperationException("Unexpected failure.")
            : WallpaperHelper.EnumerateWallpaperFiles(folder));
        slideshow.SetSlideShow(_testDirectory);
        failEnumeration = true;

        Assert.Throws<InvalidOperationException>(() => slideshow.AdvanceForwardSlideshow());
    }

    private CustomSlideshow CreateSlideshow(Func<string, IEnumerable<string>>? enumerateWallpapers = null)
    {
        return new CustomSlideshow(
            enumerateWallpapers ?? WallpaperHelper.EnumerateWallpaperFiles,
            () => _currentWallpaper,
            wallpaper =>
            {
                _attemptedWallpapers.Add(wallpaper);
                if (_failedWallpapers.Contains(wallpaper) || !WallpaperHelper.IsValidWallpaper(wallpaper))
                {
                    return false;
                }

                _currentWallpaper = wallpaper;
                return true;
            });
    }

    private string CreateFile(string name)
    {
        var path = Path.Combine(_testDirectory, name);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, string.Empty);
        return path;
    }

    private static IEnumerable<string> EnumerateThenFail(string wallpaper, bool accessDenied)
    {
        yield return wallpaper;
        if (accessDenied)
        {
            throw new UnauthorizedAccessException("Folder became inaccessible during enumeration.");
        }

        throw new IOException("Folder became unavailable during enumeration.");
    }
}
