using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaHud : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "Hud";

    /// <summary>
    /// Displays a message on a textbox.
    /// </summary>
    [LuaApiCoroutine]
    [LuaApiFunctionParam(
        0, "text", typeof(string),
        "The localization key for the message to display."
    )]
    public static DynValue message (ScriptExecutionContext ctx, CallbackArguments args) {
        var luaCor = ctx.GetCallingCoroutine();
        string msg = args[0].String;

        var tb = Hud.ShowTextbox(Localization.Text(msg));
        tb.OnComplete += (s, evt) => luaCor.Resume("example");

        return DynValue.NewYieldReq([]);
    }

    /// <summary>
    /// Pauses execution of this script for the amount of time given (in ms).
    /// </summary>
    [LuaApiCoroutine]
    [LuaApiFunctionParam(
        0, "ms", typeof(int),
        "The amount of time, in milliseconds, to wait."
    )]
    public static DynValue wait (ScriptExecutionContext ctx, CallbackArguments args) {
        var luaCor = ctx.GetCallingCoroutine();
        int ms = (int)args[0].Number;

        Coroutine.Start(_WaitCor());
        return DynValue.NewYieldReq([]);

        CoroutineTask _WaitCor () {
            yield return new WaitForSeconds(ms / 1000f);
            luaCor.Resume();
        }
    }
}
