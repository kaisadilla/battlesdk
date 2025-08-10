using battlesdk.data;
using battlesdk.graphics;
using battlesdk.input;
using battlesdk.scripts.types;
using MoonSharp.Interpreter;
using NLog;

namespace battlesdk.hud;

public class ScriptHudElement : IHudElement, IInputListener {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private Renderer _renderer;
    private LuaScriptHost _lua;
    private bool _hasControl = false;

    public string Name { get; }
    public bool IsClosed { get; } = false;

    public bool BlockOtherInput { get; private set; } = true;

    public event EventHandler<EventArgs>? OnClose;

    private DynValue? _openFn;
    private DynValue? _updateFn;
    private DynValue? _drawFn;
    private DynValue? _handleInputFn;

    public ScriptHudElement (
        Renderer renderer, ScriptAsset script, Dictionary<string, object> parameters
    ) {
        _renderer = renderer;

        Name = $"[Script Hud Element: {script.Name}]";

        _lua = LuaScriptHost.HudElementScript(script, this);
        _lua.Run();

        _openFn = _lua.GetFunction("target", "open");
        _updateFn = _lua.GetFunction("target", "update");
        _drawFn = _lua.GetFunction("target", "draw");
        _handleInputFn = _lua.GetFunction("target", "handle_input");

        if (_openFn?.Type == DataType.Function) {
            _lua.RunAsync(_openFn);
        }
    }

    public void Update () {
        if (_updateFn?.Type == DataType.Function) {
            _lua.Run(_updateFn);
        }
    }

    public void Draw () {
        if (_drawFn?.Type == DataType.Function) {
            _lua.Run(_drawFn);
        }
    }

    public void HandleInput () {
        if (_handleInputFn?.Type == DataType.Function) {
            _lua.Run(_handleInputFn);
        }
    }

    public void CedeControl () {
        _hasControl = true;
        InputManager.Push(this);
    }

    public void Close () {
        if (_hasControl) InputManager.Pop();

        OnClose?.Invoke(this, EventArgs.Empty);
    }

    public void OnInputBlocked () {

    }
}
