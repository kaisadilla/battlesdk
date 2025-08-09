using System.Text.Json.Serialization;

namespace battlesdk.data.definitions;

public class FontDefinition {
    /// <summary>
    /// The localization key to the font's display name.
    /// </summary>
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; init; }
    /// <summary>
    /// The size, in pixels, at which to load the font.
    /// </summary>
    public int? Size { get; init; }
    /// <summary>
    /// The height of a line in this font, in pixels. This is the exact distance,
    /// in pixels, between two consecutive lines rendered in this font.
    /// </summary>
    [JsonPropertyName("line_height")]
    public int? LineHeight { get; init; }
    /// <summary>
    /// The distance from the top of the line, to the actual top of the topmost
    /// characters in the font. This is, when rendering a reference character
    /// (usually uppercase A) at a point P, the vertical gap between that point
    /// and the actual topmost pixel of the character.
    /// 
    /// This property is used to correctly render all fonts at the same place
    /// regardless of their internal metrics..
    /// </summary>
    [JsonPropertyName("native_line_offset")]
    public int? NativeLineOffset { get; init; }
    /// <summary>
    /// Given a point P, the vertical gap between that point and where the text
    /// should be actually rendered to look good.
    /// </summary>
    [JsonPropertyName("line_offset")]
    public int? LineOffset { get; init; }
}
