using battlesdk.data;
using battlesdk.graphics;
using battlesdk.input;
using battlesdk.scripts;
using MoonSharp.Interpreter;
using NLog;

namespace battlesdk.screen;

public class ScriptScreenLayer : IScreenLayer, IInputListener {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public bool IsTransparent => true;

    public bool BlockOtherInput => true;

    private Renderer _renderer;
    /// <summary>
    /// The Lua VM that will execute this interaction's script.
    /// </summary>
    private Script _lua = new();
    /// <summary>
    /// Contains the compiled script as a Lua function.
    /// </summary>
    private DynValue? _scriptFunc;

    // These fields contain specific functions in the script.
    private DynValue? _openFunc;
    private DynValue? _drawFunc;
    private DynValue? _handleInputFunc;

    public ScriptScreenLayer (Renderer renderer, ScriptAsset script) {
        _renderer = renderer;

        LoadScript(script);
    }

    private void LoadScript (ScriptAsset asset) {
        string srcStr;

        try {
            srcStr = asset.GetSource();
        }
        catch (Exception ex) {
            _logger.ErrorEx(ex, "Failed to load script.");
            return;
        }
        
        Lua.RegisterGlobals(_lua);

        Table tbl = new(_lua);

        tbl["open"] = () => { };
        tbl["draw"] = () => { };
        tbl["handle_input"] = () => { };
        tbl["close"] = (Action)Close;

        _lua.Globals["Screen"] = tbl;

        _scriptFunc = _lua.LoadString(srcStr);
        _lua.Call(_scriptFunc);

        _openFunc = _lua.Globals.Get("Screen").Table.Get("open");
        _drawFunc = _lua.Globals.Get("Screen").Table.Get("draw");
        _handleInputFunc = _lua.Globals.Get("Screen").Table.Get("handle_input");
    }

    public void Open () {
        Screen.Push(this);
        InputManager.Push(this);
        if (_openFunc is not null) _lua.Call(_openFunc);
    }

    public unsafe void Draw () {
        if (_drawFunc is not null) _lua.Call(_drawFunc);
    }

    public unsafe void HandleInput () {
        if (_handleInputFunc is not null) _lua.Call(_handleInputFunc); // TODO: Not working.
    }

    public void OnInputBlocked () {

    }

    public void Close () {
        Screen.Pop();
        InputManager.Pop();
    }
}
