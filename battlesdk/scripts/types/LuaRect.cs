using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaRect : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "Rect";

    public float left;
    public float right;
    public float top;
    public float bottom;

    public LuaRect (float top, float left, float bottom, float right) {
        this.left = left;
        this.right = right;
        this.top = top;
        this.bottom = bottom;
    }

    public LuaRect (IRect rect) {
        left = rect.Left;
        right = rect.Right;
        top = rect.Top;
        bottom = rect.Bottom;
    }

    public LuaRect (Rect rect) {
        left = rect.Left;
        right = rect.Right;
        top = rect.Top;
        bottom = rect.Bottom;
    }

    public static LuaRect @new (float top, float left, float bottom, float right) {
        return new(top, left, bottom, right);
    }

    [MoonSharpHidden]
    public IRect ToIRect () {
        return new((int)top, (int)left, (int)bottom, (int)right);
    }

    [MoonSharpHidden]
    public Rect ToRect () {
        return new(top, left, bottom, right);
    }

    public override string ToString () {
        return ToRect().ToString();
    }

    public string str () => ToString();
}
