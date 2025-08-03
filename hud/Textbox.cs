using battlesdk.animations;
using battlesdk.graphics;
using battlesdk.input;
using NLog;

namespace battlesdk.hud;
public class Textbox : IInputListener {
    private const float CHARS_PER_SECOND = 80;

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static int _arrowId = -1;

    /// <summary>
    /// The texture used to draw this textbox's frame.
    /// </summary>
    private GraphicsTextboxFrame _frame;
    /// <summary>
    /// The primary font of the textbox.
    /// </summary>
    private GraphicsFont _font;
    /// <summary>
    /// The arrow at the right of the textbox.
    /// </summary>
    private GraphicsTexture? _arrow;
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

    public bool BlockOtherInput => true;

    /// <summary>
    /// This event triggers when the textbox is finished and should be closed.
    /// </summary>
    public event EventHandler<EventArgs>? OnComplete;

    public unsafe Textbox (
        Renderer renderer,
        int textboxId,
        int fontId,
        IVec2 pos,
        IVec2 size,
        string text
    ) {
        _frame = renderer.GetTextboxOrDefault(textboxId);
        _pos = pos;
        _size = size;

        _font = renderer.GetFontOrDefault(fontId);

        if (_arrowId == -1 && Registry.UiSprites.TryGetId("pause_arrow", out _arrowId) == false) {
            _logger.Error("Failed to load 'pause_arrow' texture.");
        }
        if (_arrowId != -1) {
            _arrow = renderer.GetUiTex(_arrowId);
            _arrowAnim = new(0, 2, 0.6f);
        }

        // The viewport of this textbox is defined by its position and size,
        // the padding of the frame chosen; and an offset used to make line
        // transitions look better. The number chosen for this offset is
        // arbitrary based on personal preference.
        int xOffset = 3;
        IRect viewport = new() {
            Top = pos.Y + _frame.File.Padding.Top - xOffset,
            Bottom = (pos.Y + size.Y + xOffset) - _frame.File.Padding.Bottom,
            Left = pos.X + _frame.File.Padding.Left,
            Right = (pos.X + size.X) - _frame.File.Padding.Right,
        };

        _txtRenderer = new(renderer, fontId, text, viewport, xOffset);

        _visibleLines = (viewport.Bottom - viewport.Top) / _font.Asset.LineHeight;

        _animState = AnimationState.TypingCharacters;

        InputManager.Subscribe(this);
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
            Audio.PlaySound("beep_short");

            if ((int)_currentFirstLine >= (_txtRenderer.LineCount - _visibleLines)) {
                InputManager.Unsubscribe();
                OnComplete?.Invoke(this, EventArgs.Empty);
            }
            else {
                _animState = AnimationState.MovingLine;
            }
        }
    }

    public unsafe void Draw () {
        _frame.Draw(_pos, _size);

        _txtRenderer.DrawAtViewport((int)_charCount, _currentFirstLine);

        if (_animState == AnimationState.None) {
            _arrow?.Draw(new(
                (_pos.X + _size.X) - 13,
                (_pos.Y + _size.Y) - 23 + (_arrowAnim?.Value ?? 0)
            ));
        }
    }

    /// <summary>
    /// Returns a Task that will be completed when this textbox is closed.
    /// </summary>
    public Task WaitUntilClose () {
        var tcs = new TaskCompletionSource();

        OnComplete += _Handler;
        return tcs.Task;

        void _Handler (object? s, EventArgs evt) {
            OnComplete -= _Handler;
            tcs.TrySetResult();
        }
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