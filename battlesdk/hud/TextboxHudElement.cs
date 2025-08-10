using battlesdk.animations;
using battlesdk.graphics;
using battlesdk.input;
using NLog;

namespace battlesdk.hud;
public class TextboxHudElement : IInputListener, IHudElement {
    private const float CHARS_PER_SECOND = 80;

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static int _arrowId = -1;

    private bool _hasControl = false;
    /// <summary>
    /// The texture used to draw this textbox's frame.
    /// </summary>
    private IGraphicsSprite _frame;
    /// <summary>
    /// The primary font of the textbox.
    /// </summary>
    private GraphicsFont _font;
    /// <summary>
    /// The arrow at the right of the textbox.
    /// </summary>
    private IGraphicsSprite? _arrow;
    private UpDownAnimation? _arrowAnim;

    /// <summary>
    /// If true, the player can close this textbox.
    /// </summary>
    private bool _closeable;

    /// <summary>
    /// The position of the textbox.
    /// </summary>
    private IVec2 _pos;
    /// <summary>
    /// The size of the textbox.
    /// </summary>
    private IVec2 _size;

    /// <summary>
    /// The amount of lines that fit in the textbox in its natural position
    /// (i.e. not while it's animating).
    /// </summary>
    private int _visibleLines;

    private AnimationState _animState = AnimationState.None;
    /// <summary>
    /// The amount of characters currently shown.
    /// </summary>
    private float _charCount = 0;
    /// <summary>
    /// The line that is currently the first line shown in the textbox. This
    /// if a floating value because the textbox may be transitioning from one
    /// line to another.
    /// </summary>
    private float _currentFirstLine = 0f;
    /// <summary>
    /// The text renderer.
    /// </summary>
    private GraphicsAnimatableText _txtRenderer;

    public string Name => "Textbox Hud Element";
    public bool BlockOtherInput => true;

    /// <summary>
    /// Equals true when the message contained by this textbox has been shown
    /// entirely.
    /// </summary>
    public bool IsMessageShown { get; private set; } = false;
    public bool IsClosed { get; private set; } = false;

    /// <summary>
    /// Triggers as soon as the last character of the message is printed in the
    /// screen. To track when the textbox should be closed, subscribe to
    /// <see cref="OnClose"/> instead.
    /// </summary>
    public event EventHandler<EventArgs>? OnMessageShown;
    public event EventHandler<EventArgs>? OnClose;

    public unsafe TextboxHudElement (
        Renderer renderer,
        int frameId,
        int fontId,
        IVec2 pos,
        IVec2 size,
        bool closeable,
        string text
    ) {
        OnMessageShown += (s, evt) => IsMessageShown = true;
        OnClose += (s, evt) => IsClosed = true;

        _frame = renderer.GetSprite(frameId) ?? throw new("Invalid frame");
        _pos = pos;
        _size = size;
        _closeable = closeable;

        _font = renderer.GetFontOrDefault(fontId);

        if (_arrowId == -1 && Registry.Sprites.TryGetId("ui/pause_arrow", out _arrowId) == false) {
            _logger.Error("Failed to load 'ui/pause_arrow' texture.");
        }
        if (_arrowId != -1) {
            _arrow = renderer.GetSprite(_arrowId);
            _arrowAnim = new(0, 2, 0.6f);
        }

        // The viewport of this textbox is defined by its position and size,
        // the padding of the frame chosen; and an offset used to make line
        // transitions look better. The number chosen for this offset is
        // arbitrary based on personal preference.
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

        _txtRenderer = new(renderer, fontId, text, viewport, _font.Asset.LineOffset - 1);

        _visibleLines = (viewport.Bottom - viewport.Top) / _font.Asset.LineHeight;

        _animState = AnimationState.TypingCharacters;
    }

    public void CedeControl () {
        _hasControl = true;
        InputManager.Push(this);
    }

    public void Update () {
        if (Time.TotalTime < 10f) return;

        if (_animState == AnimationState.MovingLine) {
            AnimMoveLine((int)_currentFirstLine + 1);
        }
        else if (_animState == AnimationState.TypingCharacters) {
            AnimTypingChars();
        }

        _arrowAnim?.Update();
    }

    public void HandleInput () {
        if (_animState != AnimationState.None) return;

        if (Controls.GetKeyDown(ActionKey.Primary)) {
            Audio.PlayBeepShort();

            if (IsMessageShown) {
                if (_closeable) Close();
            }
            else {
                _animState = AnimationState.MovingLine;
            }
        }
    }

    public void OnInputBlocked () {

    }

    public unsafe void Draw () {
        if (IsClosed) return;

        _frame.Draw(_pos, _size);

        _txtRenderer.DrawAtViewport((int)_charCount, _currentFirstLine);

        if (
            _animState == AnimationState.None
            && (IsMessageShown == false || _closeable)
        ) {
            _arrow?.Draw(new(
                (_pos.X + _size.X) - 13,
                (_pos.Y + _size.Y) - 23 + (_arrowAnim?.Value ?? 0)
            ));
        }
    }

    public void Close () {
        if (_hasControl) InputManager.Pop();
        OnClose?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Plays one frame of the animation to move the textbox to the line given.
    /// </summary>
    /// <param name="target">The line to move to.</param>
    private void AnimMoveLine (int target) {
        int absoluteLine = (int)_currentFirstLine;
        _currentFirstLine += 6 * Time.DeltaTime;

        if (_currentFirstLine >= target) {
            _currentFirstLine = target;
            _animState = AnimationState.TypingCharacters;
        }
    }

    /// <summary>
    /// Plays one frame of the animation to type the text into the screen.
    /// </summary>
    private void AnimTypingChars () {
        int lastLine = (int)(_currentFirstLine) + (_visibleLines - 1);

        var oldCount = _charCount;
        _charCount += CHARS_PER_SECOND * Time.DeltaTime;

        for (int i = (int)oldCount; i < (int)_charCount; i++) {
            if (i >= _txtRenderer.CharCount) {
                OnMessageShown?.Invoke(this, EventArgs.Empty);
            }
            if (i >= _txtRenderer.CharCount || _txtRenderer.GetCharLine(i) > lastLine) {
                _charCount = i;
                _animState = AnimationState.None;
                break;
            }
        }
    }

    enum AnimationState {
        None,
        MovingLine,
        TypingCharacters,
    }
}