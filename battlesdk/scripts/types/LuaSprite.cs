namespace battlesdk.scripts.types;

using battlesdk.graphics.resources;
using MoonSharp.Interpreter;

[LuaApiClass]
public class LuaSprite : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "Sprite";

    private IGraphicsSprite _sprite;
    private AnchorPoint _anchor = AnchorPoint.TopLeft;

    /// <summary>
    /// This sprite's width.
    /// </summary>
    public int width => _sprite.Width;
    /// <summary>
    /// This sprite's height.
    /// </summary>
    public int height => _sprite.Height;

    [MoonSharpHidden]
    public LuaSprite (IGraphicsSprite sprite) {
        _sprite = sprite;
    }

    /// <summary>
    /// Draws the sprite at the position given, anchored at the top left.
    /// </summary>
    /// <param name="pos">The position at which to draw the sprite.</param>
    public void draw (LuaVec2 pos) {
        _sprite.Draw(pos.ToIVec2());
    }

    /// <summary>
    /// Draws the sprite at the position given, with the anchor given.
    /// </summary>
    /// <param name="pos">The position at which to draw the sprite.</param>
    /// <param name="anchor">The anchor to use.</param>
    public void draw (LuaVec2 pos, AnchorPoint anchor) {
        var ipos = pos.ToIVec2();
        if (anchor == AnchorPoint.TopRight || anchor == AnchorPoint.BottomRight) {
            ipos -= new IVec2(width, 0);
        }
        if (anchor == AnchorPoint.BottomLeft || anchor == AnchorPoint.BottomRight) {
            ipos -= new IVec2(0, height);
        }

        _sprite.Draw(ipos);
    }

    /// <summary>
    /// Draws the sprite at the position given, with the size given.
    /// </summary>
    /// <param name="pos">The position at which to draw the sprite.</param>
    /// <param name="size">The size in the screen of the drawn sprite.</param>
    public void draw (LuaVec2 pos, LuaVec2 size) {
        _sprite.Draw(pos.ToIVec2().Anchored(size.ToIVec2(), _anchor), size.ToIVec2());
    }

    public void set_anchor (int anchor) {
        _anchor = (AnchorPoint)anchor;
    }

    public override string ToString () {
        return $"[Sprite]";
    }

    public string str () => ToString();
}
