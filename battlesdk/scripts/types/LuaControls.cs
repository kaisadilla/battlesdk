using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

public class LuaControls {
    [MoonSharpHidden]
    public const string CLASSNAME = "Controls";

    public static bool get_key_down (DynValue key) {
        if (key.Type != DataType.Number) {
            throw new ScriptRuntimeException("Invalid parameter type.");
        }

        return Controls.GetKeyDown((ActionKey)key.Number);
    }

    public static bool get_key (DynValue key) {
        if (key.Type != DataType.Number) {
            throw new ScriptRuntimeException("Invalid parameter type.");
        }

        return Controls.GetKey((ActionKey)key.Number);
    }

    public static bool get_key_up (DynValue key) {
        if (key.Type != DataType.Number) {
            throw new ScriptRuntimeException("Invalid parameter type.");
        }

        return Controls.GetKeyUp((ActionKey)key.Number);
    }
}
