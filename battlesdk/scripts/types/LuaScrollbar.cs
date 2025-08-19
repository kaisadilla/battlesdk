namespace battlesdk.scripts.types;

using battlesdk.graphics.elements;
using MoonSharp.Interpreter;
using NLog;

[LuaApiClass]
public class LuaScrollbar : ILuaType {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    [MoonSharpHidden]
    public const string CLASSNAME = "Scrollbar";

    private Scrollbar _element;

    public int width => _element.Width;
    public float Value => _element.Value;

    [MoonSharpHidden]
    public LuaScrollbar (Scrollbar textbox) {
        _element = textbox;
    }

    public void draw (LuaVec2 position) {
        _element.Draw(position.ToIVec2());
    }

    public void set_value (float value) {
        _element.SetValue(value);
    }

    public override string ToString () {
        return $"[Scrollbar value={Value}]";
    }

    public string str () => ToString();
}
