using System.Text.Json.Serialization;

namespace WallpaperSwitcher.Core.Updates;

internal sealed class GitHubReleaseResponse
{
    [JsonPropertyName("tag_name")]
    public string? TagName { get; set; }

    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }
}
