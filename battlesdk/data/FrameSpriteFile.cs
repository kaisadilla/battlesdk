using battlesdk.data.definitions;
using NLog;

namespace battlesdk.data;
public class FrameSpriteFile : SpriteFile {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The horizontal cutoff points of the textbox. This array will always
    /// contain two values.
    /// </summary>
    public int[] X { get; private init; }
    /// <summary>
    /// The vertical cutoff points of the textbox. This array will always
    /// contain two values.
    /// </summary>
    public int[] Y { get; private init; }
    /// <summary>
    /// The strategy used to resize the box's horizontal borders.
    /// </summary>
    public ResizeMode XMode { get; private init; }
    /// <summary>
    /// The strategy used to resize the box's vertical borders.
    /// </summary>
    public ResizeMode YMode { get; private init; }
    /// <summary>
    /// The strategy used to resize the box's center.
    /// </summary>
    public ResizeMode CenterMode { get; private init; }
    /// <summary>
    /// The padding applied to text inside this frame. Ideally, this padding
    /// defines the area inside the frame where text rendered will not be drawn
    /// on top of the frame's borders, and the gap between the border and the
    /// text will look good.
    /// </summary>
    public IRect TextPadding { get; private set; }
    /// <summary>
    /// The padding applied to content inside this frame. Ideally, this padding
    /// defines the area inside the frame where things will not be drawn on top
    /// of the frame's borders.
    /// </summary>
    public IRect Padding { get; private set; }

    public FrameSpriteFile (string name, string path, SpriteMetadataDefinition def)
        : base(name, path)
    {
        if (def.X is null || def.X.Count != 2 || def.Y is null || def.Y.Count != 2) {
            throw new Exception(
                "Fields 'x' and 'y' must exist and contain exactly 2 integers."
            );
        }
        if (def.XMode is null) {
            _logger.Warn($"Field 'x_mode' for {path} is not defined.");
        }
        if (def.YMode is null) {
            _logger.Warn($"Field 'y_mode' for {path} is not defined.");
        }
        if (def.CenterMode is null) {
            _logger.Warn($"Field 'center_mode' for {path} is not defined.");
        }
        if (def.TextPadding is null) {
            _logger.Warn($"Field 'text_padding' for {path} is not defined.");
        }
        if (def.Padding is null) {
            _logger.Warn($"Field 'content_padding' for {path} is not defined.");
        }

        X = [.. def.X];
        Y = [.. def.Y];

        XMode = def.XMode ?? ResizeMode.Stretch;
        YMode = def.YMode ?? ResizeMode.Stretch;
        CenterMode = def.CenterMode ?? ResizeMode.Stretch;

        TextPadding = def.TextPadding ?? new(0, 0, 0, 0);
        Padding = def.Padding ?? new(0, 0, 0, 0);
    }
}
