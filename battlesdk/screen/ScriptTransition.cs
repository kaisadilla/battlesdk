using battlesdk.data;
using battlesdk.graphics;
using battlesdk.scripts;
using MoonSharp.Interpreter;

namespace battlesdk.screen;
public class ScriptTransition : ITransition {
    private LuaScriptHost _lua;

    private DynValue? _drawFn;

    public string Name { get; }

    public ScriptTransition (Renderer renderer, ScriptAsset script) {
        Name = $"[Script Transition: {script.Name}]";

        _lua = LuaScriptHost.TransitionScript(script, this);
        _lua.Run();

        _drawFn =_lua.GetFunction("target", "draw");
    }

    public void Draw (float progress) {
        if (_drawFn?.Type == DataType.Function) {
            _lua.Run(_drawFn, DynValue.NewNumber(progress));
        }
    }
}
