namespace battlesdk.scripts.types;

using battlesdk.hud;
using MoonSharp.Interpreter;

[LuaApiClass]
public class LuaScriptHudElement : LuaHudElement {
    [MoonSharpHidden]
    public new const string CLASSNAME = "ScriptHudElement";

    private ScriptHudElement _element;

    public DynValue Result { get => _element.Result; }

    [MoonSharpHidden]
    public LuaScriptHudElement (ScriptHudElement element) : base(element) {
        _element = element;
    }

    public void set_result (DynValue result) {
        _element.SetResult(result);
    }

    public override string ToString () {
        return _element.Name;
    }
}
