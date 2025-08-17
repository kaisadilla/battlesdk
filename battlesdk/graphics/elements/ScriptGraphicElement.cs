using battlesdk.data;
using battlesdk.scripts;
using battlesdk.scripts.types;
using MoonSharp.Interpreter;
using NLog;
using System.Diagnostics.CodeAnalysis;

namespace battlesdk.graphics.elements;
public class ScriptGraphicElement {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly Renderer _renderer;
    private readonly LuaScriptHost _lua;

    private readonly DynValue? _openFn;
    private readonly DynValue? _drawFn;
    private readonly DynValue? _updateFn;
    private readonly Dictionary<string, DynValue> _funcs = [];
    public string Name { get; }

    public ScriptGraphicElement (Renderer renderer, ScriptAsset script, LuaObject args) {
        _renderer = renderer;

        Name = $"[Script Element: {script.Name}]";

        _lua = LuaScriptHost.GraphicElementScript(script, this);
        _lua.Run();

        _openFn = _lua.GetFunction("target", "open");
        _drawFn = _lua.GetFunction("draw", "open");
        _updateFn = _lua.GetFunction("update", "open");

        foreach (var name in _lua.GetDefinedFunctions("target")) {
            _funcs[name] = _lua.GetFunction("target", name);
        }

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

    public bool TryGetFunc (string name, [NotNullWhen(true)] out DynValue? func) {
        return _funcs.TryGetValue(name, out func);
    }

    public void CallAsync (string name, LuaObject? args = null) {
        if (_funcs.TryGetValue(name, out var func)) {
            _lua.RunAsync(func, args);
        }
    }

    public DynValue Call (string name, LuaObject? args = null) {
        if (_funcs.TryGetValue(name, out var func)) {
            return _lua.Run(func, args);
        }

        return DynValue.Nil;
    }
}
