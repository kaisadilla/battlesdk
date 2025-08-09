using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaHud : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "Hud";

    public static DynValue message (ScriptExecutionContext ctx, CallbackArguments args) {
        var luaCor = ctx.GetCallingCoroutine();
        string msg = args[0].String;

        var tb = Hud.ShowTextbox(Localization.Text(msg));
        tb.OnComplete += (s, evt) => luaCor.Resume("example");

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
