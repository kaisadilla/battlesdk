using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaObject : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "Object";

    private Dictionary<string, object?> _tbl = [];

    [MoonSharpHidden]
    public LuaObject () {

    }

    [MoonSharpHidden]
    public LuaObject (Table tbl) {
        foreach (var kv in tbl.Pairs) {
            _tbl[kv.Key.Type == DataType.String ? kv.Key.String : kv.Key.ToString()] = kv.Value.Type switch {
                //DataType.Table => @new(kv.Value.Table),
                _ => kv.Value
            };
        }
    }

    public static DynValue @new (ScriptExecutionContext ctx, CallbackArguments args) {
        var tbl = args[0].Table;

        return DynValue.FromObject(ctx.OwnerScript, new LuaObject(tbl));
    }

    public object? this[string key] {
        get => _tbl.TryGetValue(key, out var val) ? val : null;
        set => _tbl[key] = value;
    }

    public DynValue pairs (ScriptExecutionContext ctx, CallbackArguments args) {
        var keys = _tbl.Keys.ToList();
        int index = 0;

        DynValue iter (ScriptExecutionContext ctx, CallbackArguments args) {
            if (index >= keys.Count) return DynValue.Nil;
            string key = keys[index++];

            return DynValue.NewTuple(
                DynValue.NewString(key),
                DynValue.FromObject(ctx.GetScript(), _tbl[key])
            );
        }

        return DynValue.NewTuple(
            DynValue.NewCallback(iter),
            DynValue.Nil,
            DynValue.Nil
        );
    }

    public override string ToString () {
        return "Object"; // TODO
    }

    public string str () => ToString();

    public class Descriptor : IUserDataDescriptor {
        public string Name => CLASSNAME;

        public Type Type => typeof(LuaObject);

        public string AsString (object obj) {
            return obj.ToString() ?? "[UserData]";
        }

        public DynValue Index (Script script, object obj, DynValue index, bool isDirectIndexing) {
            var self = (LuaObject)obj;

            if (index.Type != DataType.String) return DynValue.Nil;

            string key = index.String;

            if (key == "new") {
                return DynValue.NewCallback(@new);
            }

            if (self._tbl.TryGetValue(key, out var val)) {
                return DynValue.FromObject(script, val);
            }

            return DynValue.Nil;
        }

        public bool IsTypeCompatible (Type type, object obj) {
            return type.IsInstanceOfType(obj);
        }

        public DynValue? MetaIndex (Script script, object obj, string metaname) {
            return null;
        }

        public bool SetIndex (Script script, object obj, DynValue index, DynValue value, bool isDirectIndexing) {
            var self = (LuaObject)obj;

            if (index.Type == DataType.String) {
                self._tbl[index.String] = value;
                return true;
            }

            return false;
        }
    }
}
