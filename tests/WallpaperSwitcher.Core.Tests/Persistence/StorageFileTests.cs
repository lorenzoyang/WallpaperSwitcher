using WallpaperSwitcher.Core.Persistence;

namespace WallpaperSwitcher.Core.Tests.Persistence;

public class StorageFileTests
{
    [Test]
    public void Dispose_AfterPartialWrite_DiscardsOnlyTheUncommittedFile()
    {
        var directory = Directory.CreateTempSubdirectory("WallpaperSwitcher-storage-");
        try
        {
            var destination = Path.Combine(directory.FullName, "settings.json");
            File.WriteAllText(destination, "{}");
            using (var interrupted = new StorageFile(destination))
            {
                interrupted.Stream.Write("{\"partial"u8);
            }

            Assert.That(File.ReadAllText(destination), Is.EqualTo("{}"));
            Assert.That(Directory.GetFiles(directory.FullName), Is.EqualTo(new[] { destination }));
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}
