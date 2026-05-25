using System.Net.Http.Headers;
using System.Text.Json;

namespace WallpaperSwitcher.Core.Updates;

/// <summary>
/// Checks GitHub Releases for the latest published Wallpaper Switcher version.
/// </summary>
public sealed class GitHubUpdateChecker : IUpdateChecker
{
    private static readonly Uri DefaultLatestReleaseApiUri =
        new("https://api.github.com/repos/lorenzoyang/WallpaperSwitcher/releases/latest");

    private static readonly HttpClient SharedHttpClient = new();

    private readonly HttpClient _httpClient;
    private readonly Uri _latestReleaseApiUri;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubUpdateChecker"/> class.
    /// </summary>
    public GitHubUpdateChecker() : this(SharedHttpClient)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubUpdateChecker"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to call the GitHub API.</param>
    public GitHubUpdateChecker(HttpClient httpClient) : this(httpClient, DefaultLatestReleaseApiUri)
    {
    }

    internal GitHubUpdateChecker(HttpClient httpClient, Uri latestReleaseApiUri)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _latestReleaseApiUri = latestReleaseApiUri ?? throw new ArgumentNullException(nameof(latestReleaseApiUri));
    }

    /// <inheritdoc/>
    public async Task<UpdateCheckResult> CheckForUpdatesAsync(
        Version currentVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(currentVersion);

        try
        {
            var normalizedCurrentVersion = NormalizeVersion(currentVersion);
            var release = await GetLatestReleaseAsync(cancellationToken);
            var latestTagName = GetReleaseTagName(release.TagName);
            var latestVersion = ParseReleaseVersion(latestTagName);
            var releaseUri = ParseReleaseUri(release.HtmlUrl);

            return new UpdateCheckResult(
                normalizedCurrentVersion,
                latestVersion,
                latestTagName,
                releaseUri);
        }
        catch (UpdateCheckException)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            throw new UpdateCheckException("GitHub could not be reached.", exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new UpdateCheckException("The update check timed out.", exception);
        }
        catch (JsonException exception)
        {
            throw new UpdateCheckException("GitHub returned an invalid release response.", exception);
        }
        catch (NotSupportedException exception)
        {
            throw new UpdateCheckException("GitHub returned an unsupported release response.", exception);
        }
    }

    internal static Version NormalizeVersion(Version version)
    {
        return new Version(
            version.Major,
            Math.Max(version.Minor, 0),
            Math.Max(version.Build, 0));
    }

    private async Task<GitHubReleaseResponse> GetLatestReleaseAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, _latestReleaseApiUri);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("WallpaperSwitcher", "1.0"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new UpdateCheckException(
                $"GitHub returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).");
        }

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var release = await JsonSerializer.DeserializeAsync(
            responseStream,
            UpdateCheckJsonContext.Default.GitHubReleaseResponse,
            cancellationToken);

        if (release is null)
        {
            throw new UpdateCheckException("GitHub returned an empty release response.");
        }

        return release;
    }

    private static string GetReleaseTagName(string? tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            throw new UpdateCheckException("GitHub did not return a release tag.");
        }

        return tagName.Trim();
    }

    private static Version ParseReleaseVersion(string tagName)
    {
        var versionText = tagName.Trim().TrimStart('v', 'V');
        if (!Version.TryParse(versionText, out var version))
        {
            throw new UpdateCheckException($"GitHub returned an unsupported release tag: {tagName}.");
        }

        return NormalizeVersion(version);
    }

    private static Uri ParseReleaseUri(string? htmlUrl)
    {
        if (!Uri.TryCreate(htmlUrl, UriKind.Absolute, out var releaseUri))
        {
            throw new UpdateCheckException("GitHub did not return a valid release page URL.");
        }

        return releaseUri;
    }
}
