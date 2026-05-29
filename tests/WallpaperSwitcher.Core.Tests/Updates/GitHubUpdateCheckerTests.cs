using System.Net;
using WallpaperSwitcher.Core.Updates;

namespace WallpaperSwitcher.Core.Tests.Updates;

public class GitHubUpdateCheckerTests
{
    private static readonly Uri LatestReleaseApiUri =
        new("https://api.github.com/repos/lorenzoyang/WallpaperSwitcher/releases/latest");

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
    public void CheckForUpdatesAsync_WhenGitHubRateLimitIsExceeded_ThrowsFriendlyUpdateCheckException()
    {
        var checker = CreateChecker(
            "Rate limit exceeded",
            HttpStatusCode.Forbidden,
            configureResponse: response =>
            {
                response.Headers.TryAddWithoutValidation("X-RateLimit-Remaining", "0");
                response.Headers.TryAddWithoutValidation("X-RateLimit-Reset", "0");
            });

        var exception = Assert.ThrowsAsync<UpdateCheckException>(
            async () => await checker.CheckForUpdatesAsync(new Version(3, 0, 1)));

        Assert.That(
            exception?.Message,
            Does.Contain("rate limit").And.Contain("1970-01-01 00:00:00 UTC"));
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
    public void CheckForUpdatesAsync_WhenReleaseTagHasMoreThanOneVPrefix_ThrowsUpdateCheckException()
    {
        var checker = CreateChecker("""
                                    {
                                      "tag_name": "vv3.0.2",
                                      "html_url": "https://github.com/lorenzoyang/WallpaperSwitcher/releases/tag/vv3.0.2"
                                    }
                                    """);

        var exception = Assert.ThrowsAsync<UpdateCheckException>(
            async () => await checker.CheckForUpdatesAsync(new Version(3, 0, 1)));

        Assert.That(exception?.Message, Does.Contain("unsupported release tag"));
    }

    [Test]
    public void CheckForUpdatesAsync_WhenReleaseUrlIsNotTrustedGitHubUrl_ThrowsUpdateCheckException()
    {
        var checker = CreateChecker("""
                                    {
                                      "tag_name": "v3.0.2",
                                      "html_url": "https://example.com/lorenzoyang/WallpaperSwitcher/releases/tag/v3.0.2"
                                    }
                                    """);

        var exception = Assert.ThrowsAsync<UpdateCheckException>(
            async () => await checker.CheckForUpdatesAsync(new Version(3, 0, 1)));

        Assert.That(exception?.Message, Does.Contain("valid release page URL"));
    }

    [Test]
    public void CheckForUpdatesAsync_WhenRequestTimesOut_ThrowsUpdateCheckException()
    {
        var handler = new StubHttpMessageHandler(async (_, cancellationToken) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            };
        });

        var checker = new GitHubUpdateChecker(
            new HttpClient(handler),
            LatestReleaseApiUri,
            TimeSpan.FromMilliseconds(10));

        var exception = Assert.ThrowsAsync<UpdateCheckException>(
            async () => await checker.CheckForUpdatesAsync(new Version(3, 0, 1)));

        Assert.That(exception?.Message, Does.Contain("timed out"));
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
        Action<HttpRequestMessage>? onRequest = null,
        Action<HttpResponseMessage>? configureResponse = null)
    {
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            onRequest?.Invoke(request);
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content)
            };
            configureResponse?.Invoke(response);

            return Task.FromResult(response);
        });

        return new GitHubUpdateChecker(
            new HttpClient(handler),
            LatestReleaseApiUri);
    }

    private static GitHubUpdateChecker CreateThrowingChecker(Exception exception)
    {
        var handler = new StubHttpMessageHandler((_, _) => throw exception);

        return new GitHubUpdateChecker(
            new HttpClient(handler),
            LatestReleaseApiUri);
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return responseFactory(request, cancellationToken);
        }
    }
}
