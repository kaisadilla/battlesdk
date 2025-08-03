using battlesdk.graphics;
using NLog;
using SDL;

namespace battlesdk.hud;
public class Textbox {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private unsafe SDL_Renderer* _renderer;
    private string _text;
    private GraphicsTextboxTexture _textboxTex;
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
    /// The amount of characters currently shown.
    /// </summary>
    private int _charCount = 0;
    /// <summary>
    /// The current offset of the text in the textbox's viewport.
    /// </summary>
    private float _currentOffset = 0f;

    private GraphicsAnimatableText _txt;

    public unsafe Textbox (
        Renderer renderer,
        int textboxId,
        int fontId,
        IVec2 pos,
        IVec2 size,
        string text
    ) {
        _renderer = renderer.SdlRenderer;
        _text = text;
        _textboxTex = renderer.GetTextboxOrDefault(textboxId);
        _pos = pos;
        _size = size;

        _font = renderer.GetFontOrDefault(fontId);

        int xOffset = 3;
        IRect viewport = new() {
            Top = pos.Y + _textboxTex.File.Padding.Top - xOffset,
            Bottom = (pos.Y + size.Y + xOffset) - _textboxTex.File.Padding.Bottom,
            Left = pos.X + _textboxTex.File.Padding.Left,
            Right = (pos.X + size.X) - _textboxTex.File.Padding.Right,
        };

        _txt = new(renderer, fontId, text, viewport, xOffset);
    }

    public unsafe void Draw () {
        _textboxTex.Draw(_pos, _size);

        _txt.DrawAtViewport(_charCount, _currentOffset);
    }
}
