namespace battlesdk.scripts.types;

using battlesdk.graphics.elements;
using MoonSharp.Interpreter;
using NLog;

[LuaApiClass]
public class LuaChoiceBox : ILuaType {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    [MoonSharpHidden]
    public const string CLASSNAME = "ChoiceBox";

    private ChoiceBox _element;

    public LuaVec2 position => new(_element.Position);
    public int width => _element.Width;
    public int height => _element.Height;

    [MoonSharpHidden]
    public LuaChoiceBox (ChoiceBox choiceBox) {
        _element = choiceBox;
    }

    public void draw () {
        _element.Draw();
    }

    public void move_up () {
        _element.MoveUp();
    }

    public void move_down () {
        _element.MoveDown();
    }


    public override string ToString () {
        return $"[ChoiceBox]";
    }

    public string str () => ToString();
}
