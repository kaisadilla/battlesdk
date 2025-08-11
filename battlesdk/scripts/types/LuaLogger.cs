using MoonSharp.Interpreter;
using NLog;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaLogger {
    [MoonSharpHidden]
    public const string CLASSNAME = "Logger";

    [MoonSharpHidden]
    private static readonly Logger _luaLogger = LogManager.GetLogger("Lua script");

    public static void Trace (DynValue msg) {
        _luaLogger.Trace(DynValueToStr(msg));
    }

    public static void Debug (DynValue msg) {
        _luaLogger.Debug(DynValueToStr(msg));
    }

    public static void Info (DynValue msg) {
        _luaLogger.Info(DynValueToStr(msg));
    }

    public static void Error (DynValue msg) {
        _luaLogger.Error(DynValueToStr(msg));
    }

    public static void Fatal (DynValue msg) {
        _luaLogger.Fatal(DynValueToStr(msg));
    }

    private static string DynValueToStr (DynValue val) {
        return val.Type switch {
            DataType.Nil => "nil",
            DataType.Void => "<void>",
            DataType.Boolean => val.Boolean.ToString(),
            DataType.Number => val.Number.ToString(),
            DataType.String => val.String,
            DataType.Function => "<function>",
            DataType.Table => TableToStr(val.Table),
            DataType.Tuple => TupleToStr(val.Tuple),
            DataType.UserData => val.UserData.Object.ToString() ?? "<userdata>",
            DataType.Thread => val.ToString(),
            DataType.ClrFunction => val.ToString(),
            DataType.TailCallRequest => val.ToString(),
            DataType.YieldRequest => val.ToString(),
            _ => val.ToString(),
        };
    }

    private static string TableToStr (Table tbl) {
        List<string> fields = [];

        foreach (var pair in tbl.Pairs) {
            fields.Add($"{pair.Key} = {DynValueToStr(pair.Value)}");
        }

        return $"(table: {string.Join(", ", fields)})";
    }

    private static string TupleToStr (DynValue[] tuple) {
        List<string> values = [];

        foreach (var el in tuple) {
            values.Add(DynValueToStr(el));
        }

        return $"({string.Join(", ", values)})";
    }
}
