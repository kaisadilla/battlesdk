using MoonSharp.Interpreter;
using NLog;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaScript : ILuaType {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    [MoonSharpHidden]
    public const string CLASSNAME = "Script";

    /// <summary>
    /// Pauses execution of this script for the amount of time given (in seconds).
    /// </summary>
    [LuaApiCoroutine]
    [LuaApiFunctionParam(
        0, "seconds", typeof(float),
        "The amount of time, in seconds, to wait."
    )]
    public static DynValue wait (ScriptExecutionContext ctx, CallbackArguments args) {
        var luaCor = ctx.GetCallingCoroutine();
        float seconds = (float)args[0].Number;

        Coroutine.Start(_WaitCor());
        return DynValue.NewYieldReq([]);

        CoroutineTask _WaitCor () {
            yield return new WaitForSeconds(seconds);
            luaCor.Resume();
        }
    }

    /// <summary>
    /// Pauses execution of this script until the next frame.
    /// </summary>
    [LuaApiCoroutine]
    public static DynValue wait_for_next_frame (ScriptExecutionContext ctx, CallbackArguments args) {
        var luaCor = ctx.GetCallingCoroutine();

        Coroutine.Start(_WaitCor());
        return DynValue.NewYieldReq([]);

        CoroutineTask _WaitCor () {
            yield return null;
            luaCor.Resume();
        }
    }

    public override string ToString () {
        return "[Script]";
    }

    public string str () => ToString();
}
