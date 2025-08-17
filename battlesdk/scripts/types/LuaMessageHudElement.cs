namespace battlesdk.scripts.types;

using battlesdk.hud;
using MoonSharp.Interpreter;

[LuaApiClass]
public class LuaMessageHudElement : LuaHudElement {
    [MoonSharpHidden]
    public new const string CLASSNAME = "MessageHudElement";

    private MessageHudElement _element;

    public bool is_message_complete => _element.Textbox.IsMessageShown;

    [MoonSharpHidden]
    public LuaMessageHudElement (MessageHudElement element) : base(element) {
        _element = element;
    }

    public override string ToString () {
        return $"[HudElement]";
    }

    public string str () => ToString();
}
