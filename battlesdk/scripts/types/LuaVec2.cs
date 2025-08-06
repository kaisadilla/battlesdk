using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

public class LuaVec2 : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "Vec2";

    public float x;
    public float y;

    public LuaVec2 (float x, float y) {
        this.x = x;
        this.y = y;
    }

    public static LuaVec2 @new (int x, int y) {
        return new(x, y);
    }

    public string to_string () {
        return ToString();
    }

    [MoonSharpHidden]
    public override string ToString () {
        return $"({x}, {y})";
    }

    [MoonSharpHidden]
    public IVec2 ToIVec2 () {
        return new((int)x, (int)y);
    }

    [MoonSharpHidden]
    public Vec2 ToVec2 () {
        return new(x, y);
    }

    public static LuaVec2 operator + (LuaVec2 a, LuaVec2 b) {
        return new(a.x + b.x, a.y + b.y);
    }

    public static LuaVec2 operator * (int mult, LuaVec2 vec) {
        return new(vec.x * mult, vec.y * mult);
    }

    public static LuaVec2 operator * (LuaVec2 vec, int mult) {
        return new(vec.x * mult, vec.y * mult);
    }
}
