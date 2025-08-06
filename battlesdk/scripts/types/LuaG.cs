using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaG {
    [MoonSharpHidden]
    public const string CLASSNAME = "G";

    public static bool dex_unlocked => G.DexUnlocked;
}
