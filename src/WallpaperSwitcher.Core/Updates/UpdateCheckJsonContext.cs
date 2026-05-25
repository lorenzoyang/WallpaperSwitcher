using System.Text.Json.Serialization;

namespace WallpaperSwitcher.Core.Updates;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(GitHubReleaseResponse))]
internal partial class UpdateCheckJsonContext : JsonSerializerContext
{
}
