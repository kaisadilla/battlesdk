namespace battlesdk.scripts.types;

using battlesdk.graphics.elements;
using MoonSharp.Interpreter;

[LuaApiClass]
public class LuaScriptGraphicElement {
    [MoonSharpHidden]
    public const string CLASSNAME = "ScriptElement";

    private readonly ScriptGraphicElement _element;

    [MoonSharpHidden]
    public LuaScriptGraphicElement (ScriptGraphicElement element) {
        _element = element;
    }

    public void draw () {
        _element.Draw();
    }

    public void update () {
        _element.Update();
    }

    public void call (string func, LuaObject? args) {
        _element.CallAsync(func, args);
    }

    public override string ToString () {
        return $"[ScriptGraphicsElement]";
    }

    public string str () => ToString();
}
