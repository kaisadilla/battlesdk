using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaControls {
    [MoonSharpHidden]
    public const string CLASSNAME = "Controls";

    public static bool get_key_down (int key) {
        return Controls.GetKeyDown((ActionKey)key);
    }

    public static bool get_key (int key) {
        return Controls.GetKey((ActionKey)key);
    }

    public static bool get_key_up (int key) {
        return Controls.GetKeyUp((ActionKey)key);
    }
}
