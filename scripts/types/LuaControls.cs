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
}
