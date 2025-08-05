using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

public class LuaIVec2 : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "IVec2";

    public int x;
    public int y;

    public LuaIVec2 (int x, int y) {
        this.x = x;
        this.y = y;
    }

    public static LuaIVec2 @new (int x, int y) {
        return new(x, y);
    }

    public override string ToString () {
        return $"({x}, {y})";
    }

    [MoonSharpHidden]
    public IVec2 ToNative () {
        return new(x, y);
    }
}
