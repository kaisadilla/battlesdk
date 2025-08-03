using battlesdk.graphics;
using NLog;
using SDL;

namespace battlesdk.hud;
public class Textbox {
    private const float CHARS_PER_SECOND = 80;

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private unsafe SDL_Renderer* _renderer;
    private string _message;
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
    /// The maximum amount of lines that fit in the textbox.
    /// </summary>
    private int _maxLines;

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
        _renderer = renderer.SdlRenderer;
        _message = text;
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

        _txtRenderer = new(renderer, fontId, text, viewport, xOffset);

        _maxLines = (viewport.Bottom - viewport.Top) / _font.Asset.LineHeight;

        _animState = AnimationState.TypingCharacters;
    }

    public void Update () {
        if (Time.TotalTime < 10f) return;

        if (_animState == AnimationState.None) {
            if (Controls.GetKeyDown(ActionKey.Primary)) {
                Audio.PlaySound("tap_short");

                if ((int)_currentFirstLine == (_txtRenderer.LineCount - _maxLines)) {
                    OnComplete?.Invoke(this, EventArgs.Empty);
                }
                else {
                    _animState = AnimationState.MovingLine;
                }
            }
        }
        else if (_animState == AnimationState.MovingLine) {
            AnimMoveLine();
        }
        else if (_animState == AnimationState.TypingCharacters) {
            AnimTypingChars();
        }
    }

    public unsafe void Draw () {
        _textboxTex.Draw(_pos, _size);

        _txtRenderer.DrawAtViewport((int)_charCount, _currentFirstLine);
    }

    private void AnimMoveLine () {
        int absoluteLine = (int)_currentFirstLine;
        _currentFirstLine += 6 * Time.DeltaTime;

        if (_currentFirstLine > absoluteLine + 1) {
            _currentFirstLine = absoluteLine + 1;
            _animState = AnimationState.TypingCharacters;
        }
    }

    private void AnimTypingChars () {
        int lastLine = (int)(_currentFirstLine) + (_maxLines - 1);

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