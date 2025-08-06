using battlesdk.scripts.types;
using MoonSharp.Interpreter;
using NLog;

namespace battlesdk.scripts;
public static class Lua {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static readonly List<LuaEnum> _enums = [];

    public static void Init () {
        RegisterEnumTable<ActionKey>("ActionKey");

        UserData.RegisterType<LuaVec2>(InteropAccessMode.Preoptimized, LuaVec2.CLASSNAME);
        UserData.RegisterType<LuaRect>(InteropAccessMode.Preoptimized, LuaRect.CLASSNAME);
        UserData.RegisterType<LuaLogger>(InteropAccessMode.Preoptimized, LuaLogger.CLASSNAME);
        UserData.RegisterType<LuaControls>(InteropAccessMode.Preoptimized, LuaControls.CLASSNAME);
        UserData.RegisterType<LuaAudio>(InteropAccessMode.Preoptimized, LuaAudio.CLASSNAME);

        UserData.RegisterType<LuaRenderer>(InteropAccessMode.Preoptimized, LuaRenderer.CLASSNAME);
        UserData.RegisterType<LuaSprite>(InteropAccessMode.Preoptimized, LuaFrameSprite.CLASSNAME);
        UserData.RegisterType<LuaFrameSprite>(InteropAccessMode.Preoptimized, LuaFrameSprite.CLASSNAME);
    }

    public static void RegisterGlobals (Script script) {
        if (ScreenManager.MainRenderer is null) {
            throw new("Cannot load Lua scripts yet.");
        }

        foreach (var e in _enums) {
            var tbl = new Table(script);

            foreach (var f in e.Fields) {
                tbl[f.Name] = f.Value;
            }

            script.Globals[e.TableName] = tbl;
        }

        script.Globals["message"] = (Action<DynValue>)(arg => {
            if (arg.Type != DataType.String) {
                throw new ScriptRuntimeException($"Invalid type.");
            }
            Message(arg.String);
        });
        script.Globals["wait"] = (Action<DynValue>)(arg => {
            if (arg.Type != DataType.Number) {
                throw new ScriptRuntimeException($"Invalid type.");
            }
            Wait((int)arg.Number);
        });

        script.Globals["IVec2"] = UserData.CreateStatic(typeof(LuaVec2));
        script.Globals["Rect"] = UserData.CreateStatic(typeof(LuaRect));
        script.Globals["Logger"] = UserData.CreateStatic(typeof(LuaLogger));
        script.Globals["Controls"] = UserData.CreateStatic(typeof(LuaControls));
        script.Globals["Audio"] = UserData.CreateStatic(typeof(LuaAudio));

        script.Globals["renderer"] = new LuaRenderer(ScreenManager.MainRenderer);
    }

    public static void RegisterEnumTable<T> (string tableName) where T : Enum {
        var type = typeof(T);

        var values = new List<EnumField>();

        foreach (T value in Enum.GetValues(type)) {
            var name = Enum.GetName(type, value);
            if (name is null) continue;

            values.Add(new(
                name.ToSnakeCase(),
                DynValue.NewNumber(Convert.ToInt32(value))
            ));
        }

        _enums.Add(new(tableName, values));
    }

    #region Exposed Lua functions
    private static void Wait (int ms) {
        ScriptLoop.Enqueue(new WaitScriptEvent(ms));
    }

    private static void Message (string text) {
        ScriptLoop.Enqueue(new MessageScriptEvent(text));
    }
    #endregion Exposed Lua functions
}


readonly record struct EnumField(string Name, DynValue Value);

class LuaEnum (string tableName, List<EnumField> fields) {
    public string TableName { get; } = tableName;
    public List<EnumField> Fields { get; } = fields;
}