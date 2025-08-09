using System.Text.Json.Serialization;
using WallpaperSwitcher.Core.GlobalHotkey;

namespace WallpaperSwitcher.Core.Persistence;

/// <summary>
/// Provides JSON source generation context for high-performance serialization of hotkey-related types.
/// This class enables compile-time JSON serialization metadata generation for improved performance
/// and reduced runtime reflection overhead.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(HotkeyInfo))]
[JsonSerializable(typeof(HotkeyInfo[]))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}