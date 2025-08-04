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
        script.Globals["wait"] = (Action<DynValue>)(arg => {
            if (arg.Type != DataType.Number) {
                throw new ScriptRuntimeException($"[{asset.Name}] Invalid type.");
            }
            Wait((int)arg.Number);
        });
    }

    public static void Wait (int ms) {
        ScriptLoop.Enqueue(new WaitScriptEvent(ms));
    }

    public static void Message (string text) {
        ScriptLoop.Enqueue(new MessageScriptEvent(text));
    }
}
