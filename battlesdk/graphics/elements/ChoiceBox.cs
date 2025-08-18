using battlesdk.graphics.resources;
using NLog;

namespace battlesdk.graphics.elements;
public class ChoiceBox : IDisposable {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The texture used to draw this panel's frame.
    /// </summary>
    private IGraphicsSprite _frame;
    /// <summary>
    /// The primary font of the panel.
    /// </summary>
    private GraphicsFont _font;

    /// <summary>
    /// The position of the panel.
    /// </summary>
    private IVec2 _pos;
    /// <summary>
    /// The size of the panel.
    /// </summary>
    private IVec2 _size;
    /// <summary>
    /// The frame's actual padding.
    /// </summary>
    private IRect _padding;

    /// <summary>
    /// The sprite depicing each choice in the menu.
    /// </summary>
    private readonly List<IGraphicsSprite> _choices = [];
    /// <summary>
    /// The choice arrow sprite.
    /// </summary>
    private readonly IGraphicsSprite _arrow;
    /// <summary>
    /// The currently higlighted option.
    /// </summary>
    private int _cursor = 0;

    public IVec2 Position => _pos;
    public int Width => _size.X;
    public int Height => _size.Y;

    /// <summary>
    /// The choice that is currently selected.
    /// </summary>
    public int CurrentChoice => _cursor;

    public ChoiceBox (
        Renderer renderer,
        int frameId,
        int fontId,
        IVec2 pos,
        AnchorPoint anchor,
        List<string> choices
    ) {
        _frame = renderer.GetSprite(frameId);
        _font = renderer.GetFontOrDefault(fontId);

        int width = 40; // Min width = 40. TODO: Settingize.
        int height = 0;

        foreach (var c in choices) {
            var sprite = _font.RenderShadowedPlainText(c);
            width = Math.Max(width, sprite.Width);
            height += _font.Asset.LineHeight;
            _choices.Add(sprite);
        }

        _arrow = renderer.GetSprite(Registry.Sprites.GetId("ui/choice_arrow"));

        _size = new(width, height);

        if (_frame is GraphicsFrameSprite frameSprite) {
            _padding = frameSprite.Asset.Padding + new IRect(2, 9, 1, 5);
            _size += new IVec2(
                _padding.Left + _padding.Right, _padding.Top + _padding.Bottom
            );
        }

        if (anchor == types.AnchorPoint.TopLeft) {
            _pos = pos;
        }
        else if (anchor == types.AnchorPoint.TopRight) {
            _pos = pos - new IVec2(_size.X, 0);
        }
        else if (anchor == types.AnchorPoint.BottomLeft) {
            _pos = pos - new IVec2(0, _size.Y);
        }
        else if (anchor == types.AnchorPoint.BottomRight) {
            _pos = pos - new IVec2(_size.X, _size.Y);
        }
    }

    public void Draw () {
        _frame.Draw(_pos, _size);

        for (int i = 0; i < _choices.Count; i++) {
            _choices[i].Draw(new(
                _pos.X + _padding.Left,
                _pos.Y + _padding.Top + (i * _font.Asset.LineHeight)
            ));
        }

        _arrow.Draw(new(
            _pos.X + _padding.Left - 8,
            _pos.Y + _padding.Top + 3 + (_cursor * _font.Asset.LineHeight)
        ));
    }

    public void MoveUp () {
        _cursor--;
        if (_cursor < 0) _cursor += _choices.Count;
    }

    public void MoveDown () {
        _cursor++;
        _cursor %= _choices.Count;
    }

    public void Dispose () {
        _logger.Debug($"Destroyed {nameof(ChoiceBox)}.");
        foreach (var c in _choices) {
            c.Destroy();
        }
        GC.SuppressFinalize(this);
    }
}
