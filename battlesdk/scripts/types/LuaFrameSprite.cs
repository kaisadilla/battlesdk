namespace battlesdk.scripts.types;

using battlesdk.graphics.resources;
using MoonSharp.Interpreter;

[LuaApiClass]
public class LuaFrameSprite : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "FrameSprite";

    private GraphicsFrameSprite _frame;
    private AnchorPoint _anchor = AnchorPoint.TopLeft;

    public LuaRect padding => new(_frame.Asset.Padding);

    [MoonSharpHidden]
    public LuaFrameSprite (GraphicsFrameSprite frame) {
        _frame = frame;
    }

    public void draw (LuaVec2 pos, LuaVec2 size) {
        _frame.Draw(
            pos.ToIVec2().Anchored(size.ToIVec2(), _anchor),
            size.ToIVec2()
        );
    }

    public void set_anchor (int anchor) {
        _anchor = (AnchorPoint)anchor;
    }

    public override string ToString () {
        return $"[Frame {_frame.Asset.Name}]";
    }

    public string str () => ToString();
}
