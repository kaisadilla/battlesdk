namespace battlesdk.scripts.types;

using battlesdk.graphics;
using MoonSharp.Interpreter;
using NLog;

[LuaApiClass]
public class LuaFont : ILuaType {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    [MoonSharpHidden]
    public const string CLASSNAME = "Font";

    private GraphicsFont _font;

    /// <summary>
    /// The height of one line in this font.
    /// </summary>
    public int line_height => _font.Asset.LineHeight;

    [MoonSharpHidden]
    public LuaFont (GraphicsFont font) {
        _font = font;
    }

    public LuaPlainTextSprite? render_plain_text (string str) {
        var col = SdlColor(0, 0, 0, 255);

        try {
            var sprite = _font.RenderPlainText(str);
            return new(sprite);
        }
        catch (Exception ex) {
            _logger.ErrorEx(ex, "Failed to render plain text sprite.");
            return null;
        }
    }

    public LuaPlainTextSprite? render_plain_text_shadowed (string str) {
        try {
            var sprite = _font.RenderShadowedPlainText(str);
            return new(sprite);
        }
        catch (Exception ex) {
            _logger.ErrorEx(ex, "Failed to render shadowed plain text sprite.");
            return null;
        }
    }

    public override string ToString () {
        return $"<font '{_font.Asset.Name}'>";
    }
}
