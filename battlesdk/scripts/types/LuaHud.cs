using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaHud : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "Hud";

    /// <summary>
    /// Shows a message on a textbox on the screen and waits until that textbox
    /// is closed.
    /// </summary>
    /// <param name="text"></param>
    public static void message_old (string text) {
        //var tb = Hud.ShowTextbox(text);
        //tb.Wait();
        ScriptLoop.Enqueue(new MessageScriptEvent(text));
    }

    public static DynValue message (ScriptExecutionContext ctx, CallbackArguments args) {
        string msg = args[0].String;

        var coroutine = ctx.GetCallingCoroutine();

        var tb = Hud.__TEXTBOX2(Localization.Text(msg));
        tb.OnComplete += (s, evt) => {
            coroutine.Resume("example");
            
        };
        //tb.ContinueWith(_ => Hud.Test.Enqueue(() => coroutine.Resume("example")));

        return DynValue.NewYieldReq([]);
    }

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
