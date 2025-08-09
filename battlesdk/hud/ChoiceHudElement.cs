
using battlesdk.graphics;
using battlesdk.input;

namespace battlesdk.hud;
public class ChoiceHudElement : IHudElement, IInputListener {
    /// <summary>
    /// The texture used to draw this panel's frame.
    /// </summary>
    private IGraphicsSprite _frame;
    /// <summary>
    /// The primary font of the panel.
    /// </summary>
    private GraphicsFont _font;

    private readonly bool _canBeCancelled;
    private readonly int _defaultChoice;

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

    public bool BlockOtherInput => true;
    public bool IsClosed { get; private set; } = false;
    /// <summary>
    /// The choice made by the player.
    /// </summary>
    public int Choice { get; private set; } = -1;


    public event EventHandler<EventArgs>? OnClose;

    public ChoiceHudElement (
        Renderer renderer,
        int frameId,
        int fontId,
        IVec2 pos,
        Position anchor,
        List<string> choices,
        bool canBeCancelled = false,
        int defaultChoice = -1
    ) {
        OnClose += (s, evt) => IsClosed = true;

        _frame = renderer.GetSprite(frameId) ?? throw new("Failed to get frame.");
        _font = renderer.GetFontOrDefault(fontId);
        _canBeCancelled = canBeCancelled;
        _defaultChoice = defaultChoice;

        int width = 0;
        int height = 0;

        foreach (var c in choices) {
            var sprite = _font.RenderShadowedPlainText(Localization.Text(c));
            width = Math.Max(width, sprite.Width);
            height += _font.Asset.LineHeight;
            _choices.Add(sprite);
        }

        _arrow = renderer.GetSprite(Registry.Sprites.GetId("ui/choice_arrow"))
            ?? throw new("Failed to get arrow sprite.");

        _size = new(width, height);

        if (_frame is GraphicsFrameSprite frameSprite) {
            _padding = frameSprite.Asset.Padding + new IRect(2, 9, 2, 5);
            _size += new IVec2(
                _padding.Left + _padding.Right, _padding.Top + _padding.Bottom
            );
        }

        if (anchor == Position.TopLeft) {
            _pos = pos;
        }
        else if (anchor == Position.TopRight) {
            _pos = pos - new IVec2(_size.X, 0);
        }
        else if (anchor == Position.BottomLeft) {
            _pos = pos - new IVec2(0, _size.Y);
        }
        else if (anchor == Position.BottomRight) {
            _pos = pos - new IVec2(_size.X, _size.Y);
        }

        InputManager.Push(this);
    }

    public void Update () {

    }

    public void Draw () {
        if (IsClosed) return;

        _frame.Draw(_pos, _size);

        for (int i = 0; i <  _choices.Count; i++) {
            _choices[i].Draw(new(
                _pos.X + _padding.Left,
                _pos.Y + _padding.Top + (i * _font.Asset.LineHeight)
            ));
        }

        _arrow.Draw(new(
            _pos.X + _padding.Left - 8,
            _pos.Y + _padding.Top + 2 + (_cursor * _font.Asset.LineHeight)
        ));
    }

    public void HandleInput () {
        if (Controls.GetKeyDown(ActionKey.Up)) {
            Audio.PlayBeepShort();
            _cursor--;
            if (_cursor < 0) _cursor += _choices.Count;
        }
        else if (Controls.GetKeyDown(ActionKey.Down)) {
            Audio.PlayBeepShort();
            _cursor++;
            _cursor %= _choices.Count;
        }
        else if (Controls.GetKeyDown(ActionKey.Primary)) {
            Audio.PlayBeepShort();
            Choice = _cursor;
            Close();
        }
        else if (Controls.GetKeyDown(ActionKey.Secondary)) {
            if (_canBeCancelled) {
                Audio.PlayBeepShort();
                Choice = _defaultChoice;
                Close();
            }
            else {

            }
        }
    }

    public void OnInputBlocked () {

    }

    public void Close () {
        if (IsClosed) return;

        InputManager.Pop();
        OnClose?.Invoke(this, EventArgs.Empty);
    }
}
