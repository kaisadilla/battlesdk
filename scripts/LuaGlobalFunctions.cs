using battlesdk.data;
using MoonSharp.Interpreter;
using NLog;

namespace battlesdk.scripts;
public static class LuaGlobalFunctions {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public static void RegisterGlobals (Script script, ScriptAsset asset) {
        script.Globals["message"] = (Action<DynValue>)(arg => {
            if (arg.Type != DataType.String) {
                throw new ScriptRuntimeException($"[{asset.Name}] Invalid type.");
            }
            Message(arg.String);
        });
    }

    public static void Message (string text) {
        ScriptLoop.Enqueue(new MessageScriptEvent(text));
    }
}
