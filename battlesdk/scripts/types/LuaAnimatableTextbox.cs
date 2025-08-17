namespace battlesdk.scripts.types;

using battlesdk.graphics.elements;
using MoonSharp.Interpreter;
using NLog;

[LuaApiClass]
public class LuaAnimatableTextbox : ILuaType {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    [MoonSharpHidden]
    public const string CLASSNAME = "AnimatableTextbox";

    private AnimatableTextbox _element;

    public LuaVec2 position => new(_element.Position);
    public int width => _element.Width;
    public int height => _element.Height;

    [MoonSharpHidden]
    public LuaAnimatableTextbox (AnimatableTextbox textbox) {
        _element = textbox;
    }

    public void draw () {
        _element.Draw();
    }

    public void set_anchor (int anchor) {
        throw new NotImplementedException();
    }


    public override string ToString () {
        return $"[AnimatableTextbox]";
    }

    public string str () => ToString();
}
