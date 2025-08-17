using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaList : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "List";

    private List<object?> _list = [];

    private LuaList () {

    }

    private LuaList (Table arr) {
        int index = 1;

        while (true) {
            DynValue val = arr.Get(index);

            if (val.IsNil()) break;

            _list.Add(val.Type switch {
                DataType.Table => new LuaObject(val.Table),
                _ => val
            });

            index++;
        }
    }

    public static LuaList @new () {
        return new();
    }

    public static LuaList @new (Table tbl) {
        return new(tbl);
    }

    public object? this[int key] {
        get => _list[key];
        set => _list[key] = value;
    }

    public DynValue ipairs (ScriptExecutionContext ctx, CallbackArguments args) {
        int index = 0;

        DynValue iter (ScriptExecutionContext ctx, CallbackArguments args) {
            index++; // Note: LuaList starts as 1, just like Lua's arrays.

            if (index > _list.Count) return DynValue.Nil;

            return DynValue.NewTuple(
                DynValue.NewNumber(index),
                DynValue.FromObject(ctx.GetScript(), _list[index - 1])
            );
        }

        return DynValue.NewTuple(
            DynValue.NewCallback(iter),
            DynValue.Nil,
            DynValue.Nil
        );
    }

    public override string ToString () {
        return "List"; // TODO
    }

    public string str () => ToString();
}
