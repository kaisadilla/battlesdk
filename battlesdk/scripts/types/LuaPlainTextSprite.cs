namespace battlesdk.scripts.types;

using battlesdk.graphics.resources;
using MoonSharp.Interpreter;

[LuaApiClass]
public class LuaPlainTextSprite : LuaSprite {
    [MoonSharpHidden]
    public new const string CLASSNAME = "PlainTextSprite";

    private readonly GraphicsPlainTextSprite _sprite;

    [MoonSharpHidden]
    public LuaPlainTextSprite (GraphicsPlainTextSprite sprite) : base(sprite) {
        _sprite = sprite;
    }

    ~LuaPlainTextSprite () {
        _sprite.Destroy();
    }

    public void set_anchor (int anchor) {
        _sprite.SetAnchor((AnchorPoint)anchor);
    }

    public void set_color (LuaColor color) {
        _sprite.SetColor(color.ToNative());
    }

    public void set_shadow_color (LuaColor color) {
        _sprite.SetShadowColor(color.ToNative());
    }

    public override string ToString () {
        return $"<plain text sprite>";
    }

}
