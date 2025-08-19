using battlesdk.data;
using battlesdk.graphics;
using battlesdk.input;
using battlesdk.scripts;
using MoonSharp.Interpreter;
using NLog;

namespace battlesdk.screen;

public class ScriptScreenLayer : IScreenLayer, IInputListener {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public string Name { get; }
    public bool IsTransparent => true;
    public bool BlockOtherInput => true;

    private Renderer _renderer;
    private LuaScriptHost _lua;

    // These fields contain specific functions in the script.
    private DynValue? _openFunc;
    private DynValue? _updateFunc;
    private DynValue? _drawFunc;
    private DynValue? _handleInputFunc;

    private bool _isClosed = true;

    public ScriptScreenLayer (Renderer renderer, ScriptAsset script) {
        _renderer = renderer;
        Name = $"Script Layer: {script.Name}";

        _lua = LuaScriptHost.ScreenScript(script, this);
        _lua.Run();
        _openFunc = _lua.GetFunction("target", "open");
        _updateFunc = _lua.GetFunction("target", "update");
        _drawFunc = _lua.GetFunction("target", "draw");
        _handleInputFunc = _lua.GetFunction("target", "handle_input");
    }

    public void Open () {
        _isClosed = false;
        Screen.Push(this);
        InputManager.Push(this);
        if (_openFunc is not null) _lua.RunAsync(_openFunc);
    }

    public unsafe void Update () {
        if (_updateFunc is not null) _lua.Run(_updateFunc);
    }

    public unsafe void Draw () {
        if (_drawFunc is not null) _lua.Run(_drawFunc);
    }

    public unsafe void HandleInput () {
        if (_handleInputFunc is not null) _lua.RunAsync(_handleInputFunc); // TODO: Not working.
    }

    public void OnInputBlocked () {

    }

    public void Close () {
        if (_isClosed) return;

        _isClosed = true;
        Screen.Pop();
        InputManager.Pop();
    }
}
