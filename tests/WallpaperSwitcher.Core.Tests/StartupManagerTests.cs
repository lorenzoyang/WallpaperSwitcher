using WallpaperSwitcher.Core;

namespace WallpaperSwitcher.Core.Tests;

public class StartupManagerTests
{
    [TestCase(@"C:\Program Files\WallpaperSwitcher\WallpaperSwitcher.exe", true,
        ExpectedResult = @"""C:\Program Files\WallpaperSwitcher\WallpaperSwitcher.exe"" --minimized")]
    [TestCase(@"C:\Program Files\WallpaperSwitcher\WallpaperSwitcher.exe", false,
        ExpectedResult = @"""C:\Program Files\WallpaperSwitcher\WallpaperSwitcher.exe""")]
    public string BuildStartupCommand_QuotesExecutablePathAndAddsMinimizedWhenRequested(
        string executablePath,
        bool startMinimized)
    {
        return StartupManager.BuildStartupCommand(executablePath, startMinimized);
    }
}
