namespace battlesdk.scripts.types;

using battlesdk.graphics.elements;
using MoonSharp.Interpreter;
using NLog;

[LuaApiClass]
public class LuaTextbox : ILuaType {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    [MoonSharpHidden]
    public const string CLASSNAME = "Textbox";

    private Textbox _element;

    public LuaVec2 position => new(_element.Position);
    public int width => _element.Width;
    public int height => _element.Height;

    [MoonSharpHidden]
    public LuaTextbox (Textbox textbox) {
        _element = textbox;
    }

    public void draw () {
        _element.Draw();
    }

    public void set_anchor (int anchor) {
        _element.SetAnchor((AnchorPoint)anchor);
    }


    public override string ToString () {
        return $"[Textbox]";
    }

    public string str () => ToString();
}
