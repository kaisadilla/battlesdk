using battlesdk.json;
using NLog;

namespace battlesdk.data;
public class FontAsset : AssetFile {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The default display name for this font.
    /// </summary>
    public string DisplayName { get; private init; }
    /// <summary>
    /// The size used to load this font, in pixels. Text using this font will
    /// be rendered at that size.
    /// </summary>
    public int Size { get; private set; } = 12;
    /// <summary>
    /// The height of a line in this font, in pixels. This is the exact distance,
    /// in pixels, between two consecutive lines rendered in this font.
    /// </summary>
    public int LineHeight { get; private set; } = 16;

    public FontAsset (string name, string path) : base(name, path) {
        DisplayName = name;

        var jsonPath = System.IO.Path.ChangeExtension(path, "json");

        if (File.Exists(jsonPath) == false) {
            _logger.Warn($"Failed to find metadata file '{jsonPath}'.");
            return;
        }

        var json = File.ReadAllText(jsonPath);
        var obj = Json.Parse<MetadataFile>(json);

        if (obj is null) {
            _logger.Warn($"Failed to parse metadata file '{jsonPath}'.");
            return;
        }

        if (obj.DisplayName is null) {
            _logger.Warn($"File '{jsonPath}' does not contain a default display name.");
        }
        else {
            DisplayName = obj.DisplayName;
        }

        if (obj.Size is null || obj.Size == 0) {
            _logger.Warn($"File '{jsonPath}' does not contain a valid size.");
        }
        else {
            Size = obj.Size.Value;
        }

        if (obj.LineHeight is null || obj.LineHeight == 0) {
            _logger.Warn($"File '{jsonPath}' does not contain a valid line height.");
        }
        else {
            LineHeight = obj.LineHeight.Value;
        }
    }
}

file class MetadataFile {
    public string? DisplayName { get; init; }
    public int? Size { get; init; }
    public int? LineHeight { get; init; }
}