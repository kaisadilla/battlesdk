using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaSettings {
    [MoonSharpHidden]
    public const string CLASSNAME = "Settings";

    public static LuaColor default_text_color => new(Settings.DefaultTextColor);
    public static LuaColor default_text_shadow_color => new(Settings.DefaultTextShadowColor);
}
