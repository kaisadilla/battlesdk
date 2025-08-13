using battlesdk.graphics.resources;
using NLog;

namespace battlesdk.graphics.elements;
public class Textbox {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The texture used to draw this textbox's frame.
    /// </summary>
    private IGraphicsSprite _frame;
    /// <summary>
    /// The primary font of the textbox.
    /// </summary>
    private GraphicsFont _font;

    /// <summary>
    /// The position of the textbox.
    /// </summary>
    private IVec2 _pos;
    /// <summary>
    /// The size of the textbox.
    /// </summary>
    private IVec2 _size;
    /// <summary>
    /// The position of the text in the screen.
    /// </summary>
    private IVec2 _textPos;
    private AnchorPoint _anchor;

    /// <summary>
    /// The amount of lines that fit in the textbox.
    /// </summary>
    private int _visibleLines;

    /// <summary>
    /// The text renderer.
    /// </summary>
    private GraphicsRichText _text;

    public IVec2 Position => _pos;
    public int Width => _size.X;
    public int Height => _size.Y;

    public unsafe Textbox (
        Renderer renderer,
        int frameId,
        int fontId,
        IVec2 pos,
        IVec2 size,
        string text
    ) {
        _frame = renderer.GetSprite(frameId);
        _font = renderer.GetFontOrDefault(fontId);
        _pos = pos;
        _size = size;

        IRect viewport;

        if (_frame is GraphicsFrameSprite frameSprite) {
            viewport = new() {
                Top = pos.Y + frameSprite.Asset.Padding.Top + 2,
                Bottom = (pos.Y + size.Y - 2) - frameSprite.Asset.Padding.Bottom,
                Left = pos.X + frameSprite.Asset.Padding.Left + 6,
                Right = (pos.X + size.X) - frameSprite.Asset.Padding.Right - 6,
            };
        }
        else {
            viewport = new();
        }

        _text = new(renderer, fontId, text, viewport.Right - viewport.Left);
        _textPos = new(viewport.Left, viewport.Top);

        _visibleLines = (viewport.Bottom - viewport.Top) / _font.Asset.LineHeight;
    }

    public unsafe void Draw () {
        _frame.Draw(_pos.Anchored(_size, _anchor), _size);
        _text.Draw(_textPos.Anchored(_size, _anchor));
    }

    public void SetAnchor (AnchorPoint anchor) {
        _anchor = anchor;
    }
}
