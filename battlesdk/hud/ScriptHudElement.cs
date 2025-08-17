using battlesdk.data;
using battlesdk.graphics;
using battlesdk.input;
using battlesdk.scripts;
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
    public bool IsClosed { get; private set; } = false;

    public bool BlockOtherInput { get; private set; } = true;

    public event EventHandler<EventArgs>? OnClose;

    private DynValue? _openFn;
    private DynValue? _updateFn;
    private DynValue? _drawFn;
    private DynValue? _handleInputFn;

    public DynValue Result { get; private set; } = DynValue.Nil;

    public ScriptHudElement (
        Renderer renderer, ScriptAsset script
    ) {
        OnClose += (s, evt) => IsClosed = true;

        _renderer = renderer;

        Name = $"[Script Hud Element: {script.Name}]";

        _lua = LuaScriptHost.HudElementScript(script, this);
        _lua.Run();

        _openFn = _lua.GetFunction("target", "open");
        _updateFn = _lua.GetFunction("target", "update");
        _drawFn = _lua.GetFunction("target", "draw");
        _handleInputFn = _lua.GetFunction("target", "handle_input");
    }

    public void Open (LuaObject args) {
        if (_openFn?.Type == DataType.Function) {
            _lua.RunAsync(_openFn, args);
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
            _lua.RunAsync(_handleInputFn);
        }
    }

    public void SetResult (DynValue result) {
        Result = result;
    }

    public void TakeControl () {
        _hasControl = true;
        InputManager.Push(this);
    }

    public void Close () {
        if (IsClosed) return;

        if (_hasControl) InputManager.Pop();

        OnClose?.Invoke(this, EventArgs.Empty);
    }

    public void OnInputBlocked () {

    }
}
