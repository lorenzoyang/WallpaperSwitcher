using System.Globalization;
using System.Net;
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

    private static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(15);
    private static readonly HttpClient SharedHttpClient = new();

    private readonly HttpClient _httpClient;
    private readonly Uri _latestReleaseApiUri;
    private readonly TimeSpan _requestTimeout;

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
        : this(httpClient, latestReleaseApiUri, DefaultRequestTimeout)
    {
    }

    internal GitHubUpdateChecker(HttpClient httpClient, Uri latestReleaseApiUri, TimeSpan requestTimeout)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _latestReleaseApiUri = latestReleaseApiUri ?? throw new ArgumentNullException(nameof(latestReleaseApiUri));
        if (requestTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(requestTimeout), "The update check timeout must be positive.");
        }

        _requestTimeout = requestTimeout;
    }

    /// <inheritdoc/>
    public async Task<UpdateCheckResult> CheckForUpdatesAsync(
        Version currentVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(currentVersion);
        using var timeoutCancellationTokenSource = new CancellationTokenSource(_requestTimeout);
        using var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCancellationTokenSource.Token);

        try
        {
            var normalizedCurrentVersion = NormalizeVersion(currentVersion);
            var release = await GetLatestReleaseAsync(linkedCancellationTokenSource.Token);
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
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
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
            throw new UpdateCheckException(CreateHttpErrorMessage(response));
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
        var versionText = tagName.Trim();
        if (versionText.Length > 0 && (versionText[0] == 'v' || versionText[0] == 'V'))
        {
            versionText = versionText[1..];
        }

        if (!Version.TryParse(versionText, out var version))
        {
            throw new UpdateCheckException($"GitHub returned an unsupported release tag: {tagName}.");
        }

        return NormalizeVersion(version);
    }

    private static Uri ParseReleaseUri(string? htmlUrl)
    {
        if (!Uri.TryCreate(htmlUrl, UriKind.Absolute, out var releaseUri) ||
            !IsTrustedGitHubReleaseUri(releaseUri))
        {
            throw new UpdateCheckException("GitHub did not return a valid release page URL.");
        }

        return releaseUri;
    }

    private static string CreateHttpErrorMessage(HttpResponseMessage response)
    {
        if (IsRateLimitResponse(response))
        {
            var retryAfter = GetRateLimitRetryAfter(response);
            return retryAfter is null
                ? "GitHub API rate limit was exceeded. Please try again later."
                : $"GitHub API rate limit was exceeded. Please try again after {retryAfter}.";
        }

        return $"GitHub returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).";
    }

    private static bool IsRateLimitResponse(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            return true;
        }

        return response.StatusCode == HttpStatusCode.Forbidden &&
               response.Headers.TryGetValues("X-RateLimit-Remaining", out var values) &&
               values.Any(value => string.Equals(value, "0", StringComparison.Ordinal));
    }

    private static string? GetRateLimitRetryAfter(HttpResponseMessage response)
    {
        if (response.Headers.RetryAfter?.Date is { } retryAfterDate)
        {
            return FormatUtc(retryAfterDate);
        }

        if (response.Headers.RetryAfter?.Delta is { } retryAfterDelta)
        {
            return FormatUtc(DateTimeOffset.UtcNow.Add(retryAfterDelta));
        }

        if (response.Headers.TryGetValues("X-RateLimit-Reset", out var values) &&
            values.FirstOrDefault() is { } resetValue &&
            long.TryParse(resetValue, CultureInfo.InvariantCulture, out var unixResetTime))
        {
            return FormatUtc(DateTimeOffset.FromUnixTimeSeconds(unixResetTime));
        }

        return null;
    }

    private static string FormatUtc(DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset
            .ToUniversalTime()
            .ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);
    }

    private static bool IsTrustedGitHubReleaseUri(Uri releaseUri)
    {
        return releaseUri.Scheme == Uri.UriSchemeHttps &&
               string.Equals(releaseUri.Host, "github.com", StringComparison.OrdinalIgnoreCase) &&
               releaseUri.AbsolutePath.StartsWith(
                   "/lorenzoyang/WallpaperSwitcher/releases/",
                   StringComparison.OrdinalIgnoreCase);
    }
}
