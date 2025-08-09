using NLog;
using SDL;

namespace battlesdk.graphics;

public class GraphicsText {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    protected unsafe SDL_Renderer* _renderer;
    /// <summary>
    /// The primary font of the text. Keep in mind that text may use multiple
    /// fonts - however, certain metrics, such as line height, are always based
    /// on the primary font.
    /// </summary>
    protected GraphicsFont _font;
    /// <summary>
    /// The maximum width of one line of text.
    /// </summary>
    protected readonly int _width;
    /// <summary>
    /// Each glyph contained by this text.
    /// </summary>
    protected readonly List<Glyph> _glyphs = [];

    /// <summary>
    /// <summary>
    /// The amount of characters this text has.
    /// </summary>
    public int CharCount => _glyphs.Count;
    /// The amount of lines this text has.
    /// </summary>
    public int LineCount { get; }

    public unsafe GraphicsText (
        Renderer renderer, int fontId, string text, int width
    ) {
        _renderer = renderer.SdlRenderer;
        _font = renderer.GetFontOrDefault(fontId);
        _width = width;

        var glyphGen = new GlyphGenerator(_renderer, _font, text, width);
        glyphGen.Generate();

        _glyphs = glyphGen.Glyphs;
        LineCount = glyphGen.Lines;
    }

    /// <summary>
    /// Draws all the text to the screen.
    /// </summary>
    /// <param name="pos">The position in the screen to draw at.</param>
    public unsafe void Draw (IVec2 pos) {
        int y = pos.Y;

        foreach (var g in _glyphs) {
            IVec2 glyphPos = new(
                pos.X + g.Position,
                _font.Asset.GetCorrectY(y + (g.Line * _font.Asset.LineHeight))
            );
            g.Font.DrawChar(glyphPos, g.CharRect, g.Color, true);
        }
    }

    /// <summary>
    /// Draws a line of text to the screen.
    /// </summary>
    /// <param name="pos">The position in the screen to draw at.</param>
    /// <param name="line">The line to draw.</param>
    public unsafe void DrawLine (IVec2 pos, int line) {
        foreach (var g in _glyphs) {
            if (g.Line != line) continue;

            IVec2 glyphPos = new(pos.X + g.Position, _font.Asset.GetCorrectY(pos.Y));
            g.Font.DrawChar(glyphPos, g.CharRect, g.Color, true);
        }
    }

    public unsafe void DrawPartial (IVec2 pos, int amount) {
        int y = pos.Y;

        for (int i = 0; i < _glyphs.Count; i++) {
            if (i == amount) break;
            
            var g = _glyphs[i];

            IVec2 glyphPos = new(
                pos.X + g.Position,
                _font.Asset.GetCorrectY(y + (g.Line * _font.Asset.LineHeight))
            );
            g.Font.DrawChar(glyphPos, g.CharRect, g.Color, true);
        }
    }

    /// <summary>
    /// Returns the line the character at the index given belongs to.
    /// </summary>
    /// <param name="charIndex">The index of the character to check.</param>
    public int GetCharLine (int charIndex) {
        return _glyphs[charIndex].Line;
    }
}

public unsafe struct Glyph {
    /// <summary>
    /// The font used by this glyph.
    /// </summary>
    public required GraphicsFont Font { get; init; }
    /// <summary>
    /// The position of the character in the atlas.
    /// </summary>
    public required SDL_FRect CharRect { get; init; }
    /// <summary>
    /// This glyph's color.
    /// </summary>
    public required SDL_Color Color { get; init; }

    /// <summary>
    /// The line this character is in.
    /// </summary>
    public required int Line { get; init; }
    /// <summary>
    /// The horizontal position inside its line, in pixels, this character is in.
    /// </summary>
    public required int Position { get; init; }
}

/// <summary>
/// This class generates the glyphs for the text given. This class is single-use.
/// </summary>
file class GlyphGenerator { // TODO: Texture aliases for fonts, rather than individual textures per character.
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private unsafe SDL_Renderer* _renderer;
    private GraphicsFont _font;
    /// <summary>
    /// The maximum width of one line of text.
    /// </summary>
    private readonly int _width;
    /// <summary>
    /// The text to render.
    /// </summary>
    private readonly string _text;

    /// <summary>
    /// The line of text we are currently in.
    /// </summary>
    private int _currentLine = 0;
    /// <summary>
    /// The width of the current line, so far.
    /// </summary>
    private int _lineWidth = 0;
    /// <summary>
    /// The width of the current word, so far.
    /// </summary>
    private int _wordWidth = 0;
    /// <summary>
    /// The glyphs in the word we are currently processing.
    /// </summary>
    private readonly List<GlyphData> _wordBuffer = [];

    /// <summary>
    /// A stack of colors that have been defined in the text.
    /// </summary>
    private readonly Stack<SDL_Color> _colors = [];
    /// <summary>
    /// The shadow's color.
    /// </summary>
    private SDL_Color _shadowColor = SdlColor(0, 0, 0, Constants.TEXT_SHADOW_ALPHA);

    /// <summary>
    /// Each glyph contained by this text.
    /// </summary>
    public List<Glyph> Glyphs { get; } = [];
    /// <summary>
    /// The amount of lines generated.
    /// </summary>
    public int Lines { get; private set; } = -1;

    public unsafe GlyphGenerator (
        SDL_Renderer* renderer, GraphicsFont font, string text, int width
    ) {
        _renderer = renderer;
        _font = font;
        _width = width;
        _text = text;

        //_colors.Push(SdlColor(48, 80, 200, 255));
        _colors.Push(SdlColor(85, 85, 93, 255));
    }

    public void Generate () {
        for (int i = 0; i < _text.Length; i++) {
            char c = _text[i];
            ProcessChar(c);
        }

        if (_wordBuffer.Count > 0) {
            if (_lineWidth + _wordWidth > _width) {
                StartNewLine();
            }
            CommitWord();
        }

        Lines = _currentLine + 1;
    }

    private unsafe void ProcessChar (char c) {
        // Character is newline, so it starts a new line, ending the word.
        // The newline character is not rendered.
        if (c == '\n') {
            CommitWord();
            StartNewLine();
        }
        // Character is a white space, so it ends the word.
        else if (char.IsWhiteSpace(c)) {
            // Word doesn't fit in this line, so we start a new one.
            if (_lineWidth + _wordWidth > _width) {
                StartNewLine();
            }
            // Word fits in this line, so we append the space to the word.
            CommitCharToWord(c);
            CommitWord();
        }
        else {
            CommitCharToWord(c);
        }
    }

    private void StartNewLine () {
        _currentLine++;
        _lineWidth = 0;
    }

    private unsafe void CommitCharToWord (char c) {
        var charRect = _font.GetChar(c);

        _wordBuffer.Add(new() {
            Font = _font,
            CharRect = charRect,
            Color = _colors.Peek(),
        });
        _wordWidth += (int)charRect.w;
    }

    private unsafe void CommitWord () {
        foreach (var gd in _wordBuffer) {
            Glyphs.Add(new() {
                Font = gd.Font,
                CharRect = gd.CharRect,
                Color = gd.Color,
                Line = _currentLine,
                Position = _lineWidth,
            });

            _lineWidth += (int)gd.CharRect.w;
        }

        _wordBuffer.Clear();
        _wordWidth = 0;
    }
}

file unsafe struct GlyphData {
    public required GraphicsFont Font { get; init; }
    public required SDL_FRect CharRect { get; init; }
    public required SDL_Color Color { get; init; }
}