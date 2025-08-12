using battlesdk.animations;
using battlesdk.graphics.resources;
using NLog;

namespace battlesdk.graphics.elements;
public class AnimatableTextbox {
    private const float CHARS_PER_SECOND = 80;

    enum AnimationState {
        None,
        MovingLine,
        TypingCharacters,
    }

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static int _arrowId = -1;

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
    /// The position of the textbox.
    /// </summary>
    private IVec2 _pos;
    /// <summary>
    /// The size of the textbox.
    /// </summary>
    private IVec2 _size;

    /// <summary>
    /// The amount of lines that fit in the textbox.
    /// </summary>
    private int _visibleLines;

    private AnimationState _animState = AnimationState.None;
    /// <summary>
    /// The amount of characters currently shown.
    /// </summary>
    private float _charCount = 0;
    /// <summary>
    /// The line that is currently the first line shown in the textbox. This
    /// is a floating value because the textbox may be transitioning from one
    /// line to another.
    /// </summary>
    private float _currentFirstLine = 0f;
    /// <summary>
    /// The text renderer.
    /// </summary>
    private GraphicsAnimatableText _text;

    /// <summary>
    /// Equals true when the message contained by this textbox has been shown
    /// entirely.
    /// </summary>
    public bool IsMessageShown { get; private set; } = false;

    public IVec2 Position => _pos;
    public int Width => _size.X;
    public int Height => _size.Y;

    /// <summary>
    /// Triggers as soon as the last character of the message is printed in the
    /// screen. 
    /// </summary>
    public event EventHandler<EventArgs>? OnMessageShown;

    public unsafe AnimatableTextbox (
        Renderer renderer,
        int frameId,
        int fontId,
        IVec2 pos,
        IVec2 size,
        string text
    ) {
        OnMessageShown += (s, evt) => IsMessageShown = true;

        _frame = renderer.GetSprite(frameId);
        _font = renderer.GetFontOrDefault(fontId);
        _pos = pos;
        _size = size;

        if (_arrowId == -1 && Registry.Sprites.TryGetId("ui/pause_arrow", out _arrowId) == false) {
            _logger.Error("Failed to load 'ui/pause_arrow' texture.");
        }
        if (_arrowId != -1) {
            _arrow = renderer.GetSprite(_arrowId);
            _arrowAnim = new(0, 2, 0.6f);
        }

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

        _text = new(renderer, fontId, text, viewport, _font.Asset.LineOffset - 1);
        _visibleLines = (viewport.Bottom - viewport.Top) / _font.Asset.LineHeight;
        _animState = AnimationState.TypingCharacters;
    }

    // TODO: Replace with coroutines.
    public void Update () {
        if (_animState == AnimationState.MovingLine) {
            AnimMoveLine((int)_currentFirstLine + 1);
        }
        else if (_animState == AnimationState.TypingCharacters) {
            AnimTypingChars();
        }

        _arrowAnim?.Update();
    }

    public unsafe void Draw () {
        _frame.Draw(_pos, _size);
        _text.DrawAtViewport((int)_charCount, _currentFirstLine);

        if (_animState == AnimationState.None) {
            _arrow?.Draw(new(
                (_pos.X + _size.X) - 13,
                (_pos.Y + _size.Y) - 23 + (_arrowAnim?.Value ?? 0)
            ));
        }
    }

    /// <summary>
    /// Advances the textbox, if able; and returns true if the textbox could
    /// advance.
    /// </summary>
    public bool Next () {
        if (IsMessageShown) return false;
        if (_animState != AnimationState.None) return false;

        _animState = AnimationState.MovingLine;
        return true;
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
            if (i >= _text.CharCount) {
                OnMessageShown?.Invoke(this, EventArgs.Empty);
            }
            if (i >= _text.CharCount || _text.GetCharLine(i) > lastLine) {
                _charCount = i;
                _animState = AnimationState.None;
                break;
            }
        }
    }
}
