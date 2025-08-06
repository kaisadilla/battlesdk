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

    [MoonSharpHidden]
    public LuaFont (GraphicsFont font) {
        _font = font;
    }

    public LuaSprite? render_plain_text (string str) {
        var col = SdlColor(0, 0, 0, 255);

        try {
            var sprite = _font.RenderPlainText(str);
            return new(sprite);
        }
        catch (Exception ex) {
            _logger.Error(ex, "Failed to render plain text sprite.");
            return null;
        }
    }

    public LuaSprite? render_plain_text_shadowed (string str) {
        try {
            var sprite = _font.RenderShadowedPlainText(str);
            return new(sprite);
        }
        catch (Exception ex) {
            _logger.Error(ex, "Failed to render shadowed plain text sprite.");
            return null;
        }
    }

    public override string ToString () {
        return $"<font '{_font.Asset.Name}'>";
    }
}
