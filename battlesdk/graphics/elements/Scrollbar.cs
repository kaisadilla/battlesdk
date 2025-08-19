using battlesdk.graphics.resources;
using NLog;

namespace battlesdk.graphics.elements;
public class Scrollbar {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private IGraphicsSprite _track;
    private IGraphicsSprite _thumb;

    /// <summary>
    /// The track's width.
    /// </summary>
    private int _width;
    /// <summary>
    /// The range, in pixels, of x positions at which the thumb can be placed.
    /// This is equal to the width of the track minus the width of the thumb.
    /// </summary>
    private int _range;

    public int Width => _width;

    public float Value { get; private set; }

    public Scrollbar (Renderer renderer, int width, float value) {
        if (Registry.Sprites.TryGetId("ui/elements/scrollbar_track", out var trackId) == false) {
            throw new Exception("Couldn't load track sprite."); // TODO: Better exceptions.
        }
        if (Registry.Sprites.TryGetId("ui/elements/scrollbar_thumb", out var thumbId) == false) {
            throw new Exception("Couldn't load thumb sprite."); // TODO: Better exceptions.
        }

        _track = renderer.GetSprite(trackId);
        _thumb = renderer.GetSprite(thumbId);
        _width = width;
        _range = width - _thumb.Width;

        Value = value;
    }

    public void Draw (IVec2 position) {
        _track.Draw(position, new(_width, _track.Height));
        _thumb.Draw(position + new IVec2((int)Math.Round(_range * Value), 0));
    }

    public void SetValue (float value) {
        Value = value;
    }
}
