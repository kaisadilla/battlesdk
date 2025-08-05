using battlesdk.json;
using NLog;
using SDL;

namespace battlesdk.data;
public class FrameAsset : AssetFile {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The default display name for this text box.
    /// </summary>
    public string DisplayName { get; private init; }
    /// <summary>
    /// The sprite's width.
    /// </summary>
    public int Width { get; private init; }
    /// <summary>
    /// The sprite's height.
    /// </summary>
    public int Height { get; private init; }
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
    public IRect ContentPadding { get; private set; }

    public FrameAsset (string name, string path) : base(name, path) {
        unsafe {
            var surface = SDL3_image.IMG_Load(path);

            Width = surface->w;
            Height = surface->h;

            SDL3.SDL_DestroySurface(surface);
        }

        var jsonPath = System.IO.Path.ChangeExtension(path, "json");

        if (File.Exists(jsonPath) == false) {
            throw new Exception($"Failed to find metadata file '{jsonPath}'.");
        }

        var json = File.ReadAllText(jsonPath);
        var obj = Json.Parse<MetadataFile>(json);

        if (obj is null) {
            throw new Exception($"Failed to parse metadata file '{jsonPath}'.");
        }

        if (obj.X is null || obj.X.Length != 2 || obj.Y is null || obj.Y.Length != 2) {
            throw new Exception(
                "X and Y properties must exist and contain exactly 2 integers."
            );
        }

        if (obj.DisplayName is null) {
            _logger.Warn($"File '{jsonPath}' does not contain a default display name.");
        }

        DisplayName = obj.DisplayName ?? name;

        X = obj.X;
        Y = obj.Y;

        XMode = obj.XMode;
        YMode = obj.YMode;
        CenterMode = obj.CenterMode;

        TextPadding = obj.TextPadding;
        ContentPadding = obj.ContentPadding;
    }
}

file class MetadataFile {
    public string? DisplayName { get; init; }
    public int[]? X { get; init; }
    public int[]? Y { get; init; }
    public ResizeMode XMode { get; init; }
    public ResizeMode YMode { get; init; }
    public ResizeMode CenterMode { get; init; }
    public IRect TextPadding { get; init; }
    public IRect ContentPadding { get; init; }
}