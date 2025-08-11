using battlesdk.data;
using battlesdk.hud;
using battlesdk.screen;
using battlesdk.world.entities;
using MoonSharp.Interpreter;
using NLog;

namespace battlesdk.scripts;
public class LuaScriptHost {
    public class EventArgs (int id) : System.EventArgs {
        /// <summary>
        /// The id of this execution.
        /// </summary>
        public int Id { get; } = id;
    }

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private IdProvider _idProvider = new();

    /// <summary>
    /// The Lua VM that will execute this script.
    /// </summary>
    private Script _lua = new();

    /// <summary>
    /// Contains the compiled script as a Lua function.
    /// </summary>
    private DynValue? _scriptFunc;

    /// <summary>
    /// An event that triggers when an execution of this script has successfully
    /// completed. This event identifies the execution that triggered it.
    /// </summary>
    public event EventHandler<EventArgs>? OnExecutionComplete;

    private LuaScriptHost () {}

    public static LuaScriptHost EntityInteractionScript (
        ScriptAsset asset, Entity target
    ) {
        var host = new LuaScriptHost();

        var src = asset.GetSource();

        Lua.RegisterGlobals(host._lua);
        Lua.RegisterEntityInteraction(host._lua, target);

        host._scriptFunc = host._lua.LoadString(src);
        return host;
    }

    public static LuaScriptHost ScreenScript (
        ScriptAsset asset, ScriptScreenLayer screen
    ) {
        var host = new LuaScriptHost();

        var src = asset.GetSource();

        Lua.RegisterGlobals(host._lua);
        Lua.RegisterScreenHandler(host._lua, screen);

        host._scriptFunc = host._lua.LoadString(src);
        return host;
    }

    public static LuaScriptHost HudElementScript (
        ScriptAsset asset, ScriptHudElement hudElement
    ) {
        var host = new LuaScriptHost();

        var src = asset.GetSource();

        Lua.RegisterGlobals(host._lua);
        Lua.RegisterHudElementHandler(host._lua, hudElement);

        host._scriptFunc = host._lua.LoadString(src);
        return host;
    }

    public static LuaScriptHost TransitionScript (
        ScriptAsset asset, ScriptTransition transition
    ) {
        var host = new LuaScriptHost();

        var src = asset.GetSource();

        Lua.RegisterGlobals(host._lua);
        Lua.RegisterTransitionHandler(host._lua, transition);

        host._scriptFunc = host._lua.LoadString(src);
        return host;
    }

    /// <summary>
    /// Runs the main script given in this host as a script coroutine, which 
    /// means that its execution will not end after this call is made. Use
    /// <see cref="OnExecutionComplete"/> to keep track of when this execution's
    /// been completed. Returns an id that uniquely identifies this execution.
    /// </summary>
    public int RunAsync () {
        if (_scriptFunc is null) return -1;
        return RunAsync(_scriptFunc);
    }

    /// <summary>
    /// Runs the function given in this host as a script coroutine, which means
    /// that its execution will not end after this call is made. Use
    /// <see cref="OnExecutionComplete"/> to keep track of when this execution's
    /// been completed. Returns an id that uniquely identifies this execution.
    /// </summary>
    /// <param name="func">The function to execute.</param>
    public int RunAsync (DynValue func) {
        var id = _idProvider.NextId();
        var luaCor = _lua.CreateCoroutine(func);

        Coroutine.Start(CompleteLuaCoroutine(luaCor.Coroutine, id));

        return id;
    }

    /// <summary>
    /// Runs the main script given in this host concurrently, as such, its
    /// execution ends before this function returns; but the script will crash
    /// if it uses any asynchronous API functions (those that implement Lua
    /// coroutines).
    /// </summary>
    public void Run () {
        if (_scriptFunc is null) return;
        Run(_scriptFunc);
    }

    /// <summary>
    /// Runs the function given in this host concurrently, as such, its execution
    /// ends before this function returns; but the script will crash if it uses
    /// any asynchronous API functions (those that implement Lua coroutines).
    /// </summary>
    public void Run (DynValue func, params DynValue[] args) {
        _lua.Call(func, args);
    }

    public DynValue GetFunction (string table, string name) {
        return _lua.Globals.Get(table).Table.Get(name);
    }

    private CoroutineTask CompleteLuaCoroutine (LuaCoroutine luaCor, int id) {
        luaCor.Resume();

        while (luaCor.State != CoroutineState.Dead) {
            yield return null;
        }

        OnExecutionComplete?.Invoke(this, new(id));
    }
}
