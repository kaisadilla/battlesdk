using battlesdk.data.definitions;
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
    /// <summary>
    /// The distance from the top of the line, to the actual top of the topmost
    /// characters in the font. This is, when rendering a reference character
    /// (usually uppercase A) at a point P, the vertical gap between that point
    /// and the actual topmost pixel of the character.
    /// 
    /// This property is used to correctly render all fonts at the same place
    /// regardless of their internal metrics..
    /// </summary>
    public int NativeLineOffset { get; private set; } = 0;
    /// <summary>
    /// Given a point P, the vertical gap between that point and where the text
    /// should be actually rendered to look good.
    /// </summary>
    public int LineOffset { get; private set; } = 4;

    public FontAsset (string name, string path) : base(name, path) {
        DisplayName = name;

        var jsonPath = System.IO.Path.ChangeExtension(path, "json");

        if (File.Exists(jsonPath) == false) {
            _logger.Warn($"Failed to find metadata file '{jsonPath}'.");
            return;
        }

        var json = File.ReadAllText(jsonPath);
        var obj = Json.Parse<FontDefinition>(json);

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

        if (obj.NativeLineOffset is null) {
            _logger.Warn($"'{jsonPath}' - Missing field: 'native_line_offset'.");
        }
        else {
            NativeLineOffset = obj.NativeLineOffset.Value;
        }

        if (obj.LineOffset is null) {
            _logger.Warn($"'{jsonPath}' - Missing field: 'line_offset'.");
        }
        else {
            LineOffset = obj.LineOffset.Value;
        }
    }

    /// <summary>
    /// Given a vertical position, returns the vertical position where this
    /// font needs to be rendered to look as intended, assuming a line height
    /// of <see cref="LineHeight"/> is being respected (i.e. each line is
    /// rendered one line height below the previous one).
    /// </summary>
    /// <param name="y">The reference y position.</param>
    /// <returns></returns>
    public int GetCorrectY (int y) {
        return y - NativeLineOffset + LineOffset;
    }
}
