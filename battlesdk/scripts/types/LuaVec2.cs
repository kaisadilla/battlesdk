using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaVec2 : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "Vec2";

    public float x;
    public float y;

    /// <summary>
    /// The vector (0, 0).
    /// </summary>
    public static LuaVec2 zero { get; } = new(0, 0);

    public LuaVec2 (float x, float y) {
        this.x = x;
        this.y = y;
    }

    public LuaVec2 (IVec2 v) {
        x = v.X;
        y = v.Y;
    }

    public static LuaVec2 @new (int x, int y) {
        return new(x, y);
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

    public static LuaVec2 operator - (LuaVec2 a, LuaVec2 b) {
        return new(a.x - b.x, a.y - b.y);
    }

    public static LuaVec2 operator * (int mult, LuaVec2 vec) {
        return new(vec.x * mult, vec.y * mult);
    }

    public static LuaVec2 operator * (LuaVec2 vec, int mult) {
        return new(vec.x * mult, vec.y * mult);
    }

    [MoonSharpHidden]
    public override string ToString () {
        return ToVec2().ToString();
    }

    public string str () => ToString();
}
