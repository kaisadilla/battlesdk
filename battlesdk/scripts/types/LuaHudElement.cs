namespace battlesdk.scripts.types;

using battlesdk.hud;
using MoonSharp.Interpreter;

[LuaApiClass]
public class LuaHudElement : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "HudElement";

    private IHudElement _element;

    public bool is_closed => _element.IsClosed;

    [MoonSharpHidden]
    public LuaHudElement (IHudElement element) {
        _element = element;
    }

    public void draw () {
        _element.Draw();
    }

    public void update () {
        _element.Update();
    }

    public void handle_input () {
        _element.HandleInput();
    }

    public void close () {
        _element.Close();
    }

    public override string ToString () {
        return $"[HudElement]";
    }

    public string str () => ToString();
}
