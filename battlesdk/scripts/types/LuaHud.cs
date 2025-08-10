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
        string text = args[0].String;

        var msg = Hud.ShowMessage(Localization.Text(text));
        msg.OnClose += (s, evt) => luaCor.Resume();

        return DynValue.NewYieldReq([]);
    }

    [LuaApiCoroutine]
    [LuaApiFunctionParam(0, "message", typeof(string), "The content of the textbox.")]
    [LuaApiFunctionParam(
        1,
        "choices",
        typeof(List<string>),
        "An array of keys for localized strings. Each string represents one choice.")
    ]
    [LuaApiFunctionParam(
        2,
        "can_be_cancelled",
        typeof(bool?),
        "True if the choice can be cancelled."
    )]
    [LuaApiFunctionParam(
        3,
        "default_choice",
        typeof(int?),
        "The default choice if the player cancels the choice. A value of -1 indicates no choice."
    )]
    public static DynValue choice_message (ScriptExecutionContext ctx, CallbackArguments args) {
        var luaCor = ctx.GetCallingCoroutine();
        string msg = args[0].String;
        List<string> choices = args[1].ToObject<List<string>>();
        bool canBeCancelled = args.Count >= 3 ? args[2].Boolean : true;
        int defaultChoice = args.Count >= 4 ? (int)(args[3].Number - 1) : -1; // Lua index to C# index.

        var choice = Hud.ShowChoiceMessage(
            Localization.Text(msg), choices, canBeCancelled, defaultChoice
        );
        choice.OnClose += (s, evt) => {
            var c = choice.Choice;
            if (c != -1) c++; // C# index to Lua index.
            luaCor.Resume(c);
        };

        return DynValue.NewYieldReq([]);
    }

    public static LuaHudElement script_element (string scriptName) {
        if (Registry.Scripts.TryGetId(scriptName, out int scriptId) == false) {
            throw new ScriptRuntimeException("Invalid script name.");
        }

        var el = Hud.ShowScriptElement(scriptId, false);
        return new(el);
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
