namespace battlesdk.graphics;
public class GraphicsAnimatableText : GraphicsText {
    public IRect Viewport { get; }
    public int Padding { get; }

    public GraphicsAnimatableText (
        Renderer renderer,
        int fontId,
        string text,
        IRect viewport,
        int padding
    ) : base(renderer, fontId, text, viewport.Right - viewport.Left)
    {
        Viewport = viewport;
        Padding = padding;
    }

    public unsafe void DrawAtViewport (int amount, float offset) {
        int y = Viewport.Top + Padding - ((int)(_font.Asset.LineHeight * offset));

        for (int i = 0; i < _glyphs.Count; i++) {
            if (i == amount) break;

            var g = _glyphs[i];

            IVec2 glyphPos = new(
                Viewport.Left + g.Position,
                _font.Asset.GetCorrectY(y + (g.Line * _font.Asset.LineHeight))
            );

            // The glyph is fully below the viewport, so it won't be rendered.
            if (glyphPos.Y >= Viewport.Bottom) continue;
            // The glyph is fully above the viewport, so it won't be rendered.
            if (glyphPos.Y + g.CharRect.h < Viewport.Top) continue;



            g.Font.DrawCharInViewport(glyphPos, Viewport, g.CharRect, g.Color, true);
        }
    }

    public unsafe void DrawAtViewport (float offset) {
        DrawAtViewport(_glyphs.Count, offset);
    }
}
