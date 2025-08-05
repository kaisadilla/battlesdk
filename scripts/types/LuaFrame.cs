namespace battlesdk.scripts.types;

using battlesdk.graphics;
using MoonSharp.Interpreter;

public class LuaFrame : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "Frame";

    private GraphicsFrameSprite _frame;

    [MoonSharpHidden]
    public LuaFrame (GraphicsFrameSprite frame) {
        _frame = frame;
    }

    public void draw (LuaIVec2 pos, LuaIVec2 size) {
        _frame.Draw(pos.ToNative(), size.ToNative());
    }
}
