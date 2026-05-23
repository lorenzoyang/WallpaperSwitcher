using System.Text.Json.Serialization;
using WallpaperSwitcher.Core.GlobalHotkey;

namespace WallpaperSwitcher.Core.Persistence;

/// <summary>
/// Provides source generation context for System.Text.Json to enable
/// high-performance serialization and deserialization of <see cref="HotkeyInfo"/> objects.
/// </summary>
/// <remarks>
/// This class is used to configure JSON source generation, reducing runtime reflection overhead.
/// </remarks>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(HotkeyInfo))]
[JsonSerializable(typeof(HotkeyInfo[]))]
[JsonSerializable(typeof(AppSettings))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}
