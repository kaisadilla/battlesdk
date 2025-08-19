using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaColor : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "Color";

    public int r;
    public int g;
    public int b;
    public int a;

    public LuaColor (int r, int g, int b, int a) {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    public LuaColor (ColorRGBA col) {
        r = col.R;
        g = col.G;
        b = col.B;
        a = col.A;
    }

    public static LuaColor @new (int r, int g, int b, int a) {
        return new(r, g, b, a);
    }

    [MoonSharpHidden]
    public ColorRGBA ToNative () {
        return new(r, g, b, a);
    }

    public override string ToString () {
        return ToNative().ToString();
    }

    public string str () => ToString();
}
