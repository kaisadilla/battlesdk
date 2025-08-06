namespace battlesdk.scripts.types;

using battlesdk.graphics;
using MoonSharp.Interpreter;

[LuaApiClass]
public class LuaFrameSprite : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "FrameSprite";

    private GraphicsFrameSprite _frame;

    public LuaRect padding => new(_frame.Asset.Padding);

    [MoonSharpHidden]
    public LuaFrameSprite (GraphicsFrameSprite frame) {
        _frame = frame;
    }

    public void draw (LuaVec2 pos, LuaVec2 size) {
        _frame.Draw(pos.ToIVec2(), size.ToIVec2());
    }

    public override string ToString () {
        return $"<frame {_frame.Asset.Name}>";
    }
}
