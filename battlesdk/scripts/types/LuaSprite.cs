namespace battlesdk.scripts.types;

using battlesdk.graphics;
using MoonSharp.Interpreter;

[LuaApiClass]
public class LuaSprite : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "Sprite";

    private IGraphicsSprite _sprite;

    [MoonSharpHidden]
    public LuaSprite (IGraphicsSprite sprite) {
        _sprite = sprite;
    }

    public void draw (LuaVec2 pos) {
        _sprite.Draw(pos.ToIVec2());
    }

    public void draw (LuaVec2 pos, LuaVec2 size) {
        _sprite.Draw(pos.ToIVec2(), size.ToIVec2());
    }

    public override string ToString () {
        return $"<sprite>";
    }
}
