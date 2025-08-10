namespace battlesdk.scripts.types;

using battlesdk.graphics;
using MoonSharp.Interpreter;

[LuaApiClass]
public class LuaSprite : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "Sprite";

    private IGraphicsSprite _sprite;

    /// <summary>
    /// This sprite's width.
    /// </summary>
    public int width => _sprite.Width;
    /// <summary>
    /// This sprite's height.
    /// </summary>
    public int weight => _sprite.Height;

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
