using System.Text.Json.Serialization;

namespace battlesdk.data.definitions;

public enum SpriteType {
    Normal,
    Spritesheet,
    Frame,
    Character
}

public class SpriteMetadataDefinition {
    public SpriteType? Type { get; init; }

    [JsonPropertyName("sprite_size")]
    public IVec2? SpriteSize { get; init; }

    public List<string>? Names { get; init; }

    public List<string>? Sheets { get; init; }

    public List<int>? X { get; init; }
    public List<int>? Y { get; init; }

    [JsonPropertyName("x_mode")]
    public ResizeMode? XMode { get; init; }

    [JsonPropertyName("y_mode")]
    public ResizeMode? YMode { get; init; }

    [JsonPropertyName("center_mode")]
    public ResizeMode? CenterMode { get; init; }

    [JsonPropertyName("text_padding")]
    public IRect? TextPadding { get; init; }
    public IRect? Padding { get; init; }

    /// <summary>
    /// Returns a new definition that is a copy of this one, with its values
    /// overriden by the definition given when said definition has them.
    /// </summary>
    public SpriteMetadataDefinition With (SpriteMetadataDefinition? prioritary) {
        SpriteMetadataDefinition clone = new() {
            Type = prioritary?.Type ?? Type,
            SpriteSize = prioritary?.SpriteSize ?? SpriteSize,
            Names = prioritary?.Names ?? Names,
            Sheets = prioritary?.Sheets ?? Sheets,
            X = prioritary?.X ?? X,
            Y = prioritary?.Y ?? Y,
            XMode = prioritary?.XMode ?? XMode,
            YMode = prioritary?.YMode ?? YMode,
            CenterMode = prioritary?.CenterMode ?? CenterMode,
            TextPadding = prioritary?.TextPadding ?? TextPadding,
            Padding = prioritary?.Padding ?? Padding,
        };
        
        return clone;
    }
}
