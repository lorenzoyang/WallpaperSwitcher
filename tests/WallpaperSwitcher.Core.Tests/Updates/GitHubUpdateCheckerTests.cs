using System.Net;
using WallpaperSwitcher.Core.Updates;

namespace WallpaperSwitcher.Core.Tests.Updates;

public class GitHubUpdateCheckerTests
{
    [Test]
    public async Task CheckForUpdatesAsync_WhenLatestVersionIsNewer_ReturnsUpdateAvailable()
    {
        var checker = CreateChecker("""
                                    {
                                      "tag_name": "v3.0.2",
                                      "html_url": "https://github.com/lorenzoyang/WallpaperSwitcher/releases/tag/v3.0.2"
                                    }
                                    """);

        var result = await checker.CheckForUpdatesAsync(new Version(3, 0, 1));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsUpdateAvailable, Is.True);
            Assert.That(result.CurrentVersion, Is.EqualTo(new Version(3, 0, 1)));
            Assert.That(result.LatestVersion, Is.EqualTo(new Version(3, 0, 2)));
            Assert.That(result.LatestTagName, Is.EqualTo("v3.0.2"));
        }
    }

    [Test]
    public async Task CheckForUpdatesAsync_WhenVersionsAreEqual_IgnoresCurrentVersionRevision()
    {
        var checker = CreateChecker("""
                                    {
                                      "tag_name": "v3.0.1",
                                      "html_url": "https://github.com/lorenzoyang/WallpaperSwitcher/releases/tag/v3.0.1"
                                    }
                                    """);

        var result = await checker.CheckForUpdatesAsync(new Version(3, 0, 1, 0));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsUpdateAvailable, Is.False);
            Assert.That(result.CurrentVersion, Is.EqualTo(new Version(3, 0, 1)));
            Assert.That(result.LatestVersion, Is.EqualTo(new Version(3, 0, 1)));
        }
    }

    [Test]
    public async Task CheckForUpdatesAsync_WhenLatestVersionIsOlder_ReturnsNoUpdate()
    {
        var checker = CreateChecker("""
                                    {
                                      "tag_name": "v3.0.0",
                                      "html_url": "https://github.com/lorenzoyang/WallpaperSwitcher/releases/tag/v3.0.0"
                                    }
                                    """);

        var result = await checker.CheckForUpdatesAsync(new Version(3, 0, 1));

        Assert.That(result.IsUpdateAvailable, Is.False);
    }

    [Test]
    public void CheckForUpdatesAsync_WhenGitHubReturnsHttpError_ThrowsUpdateCheckException()
    {
        var checker = CreateChecker("Not found", HttpStatusCode.NotFound);

        var exception = Assert.ThrowsAsync<UpdateCheckException>(
            async () => await checker.CheckForUpdatesAsync(new Version(3, 0, 1)));

        Assert.That(exception?.Message, Does.Contain("HTTP 404"));
    }

    [Test]
    public void CheckForUpdatesAsync_WhenGitHubReturnsInvalidJson_ThrowsUpdateCheckException()
    {
        var checker = CreateChecker("{ invalid json");

        var exception = Assert.ThrowsAsync<UpdateCheckException>(
            async () => await checker.CheckForUpdatesAsync(new Version(3, 0, 1)));

        Assert.That(exception?.Message, Does.Contain("invalid release response"));
    }

    [Test]
    public void CheckForUpdatesAsync_WhenGitHubCannotBeReached_ThrowsUpdateCheckException()
    {
        var checker = CreateThrowingChecker(new HttpRequestException("Network unavailable"));

        var exception = Assert.ThrowsAsync<UpdateCheckException>(
            async () => await checker.CheckForUpdatesAsync(new Version(3, 0, 1)));

        Assert.That(exception?.Message, Does.Contain("could not be reached"));
    }

    [Test]
    public void CheckForUpdatesAsync_WhenGitHubReturnsInvalidReleaseTag_ThrowsUpdateCheckException()
    {
        var checker = CreateChecker("""
                                    {
                                      "tag_name": "latest",
                                      "html_url": "https://github.com/lorenzoyang/WallpaperSwitcher/releases/tag/latest"
                                    }
                                    """);

        var exception = Assert.ThrowsAsync<UpdateCheckException>(
            async () => await checker.CheckForUpdatesAsync(new Version(3, 0, 1)));

        Assert.That(exception?.Message, Does.Contain("unsupported release tag"));
    }

    [Test]
    public async Task CheckForUpdatesAsync_SendsGitHubHeaders()
    {
        HttpRequestMessage? capturedRequest = null;
        var checker = CreateChecker("""
                                    {
                                      "tag_name": "v3.0.1",
                                      "html_url": "https://github.com/lorenzoyang/WallpaperSwitcher/releases/tag/v3.0.1"
                                    }
                                    """, onRequest: request => capturedRequest = request);

        _ = await checker.CheckForUpdatesAsync(new Version(3, 0, 1));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(capturedRequest?.Headers.UserAgent.ToString(), Does.Contain("WallpaperSwitcher"));
            Assert.That(
                capturedRequest?.Headers.Accept.Select(header => header.MediaType),
                Does.Contain("application/vnd.github+json"));
        }
    }

    private static GitHubUpdateChecker CreateChecker(
        string content,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        Action<HttpRequestMessage>? onRequest = null)
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            onRequest?.Invoke(request);
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content)
            };
        });

        return new GitHubUpdateChecker(
            new HttpClient(handler),
            new Uri("https://api.github.com/repos/lorenzoyang/WallpaperSwitcher/releases/latest"));
    }

    private static GitHubUpdateChecker CreateThrowingChecker(Exception exception)
    {
        var handler = new StubHttpMessageHandler(_ => throw exception);

        return new GitHubUpdateChecker(
            new HttpClient(handler),
            new Uri("https://api.github.com/repos/lorenzoyang/WallpaperSwitcher/releases/latest"));
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(responseFactory(request));
        }
    }
}
