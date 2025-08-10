global using LuaCoroutine = MoonSharp.Interpreter.Coroutine;
using battlesdk.hud;
using battlesdk.screen;
using battlesdk.scripts.types;
using battlesdk.world.entities;
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
        UserData.RegisterType<LuaColor>(InteropAccessMode.Preoptimized, LuaColor.CLASSNAME);
        UserData.RegisterType<LuaLogger>(InteropAccessMode.Preoptimized, LuaLogger.CLASSNAME);
        UserData.RegisterType<LuaFmt>(InteropAccessMode.Preoptimized, LuaFmt.CLASSNAME);
        UserData.RegisterType<LuaControls>(InteropAccessMode.Preoptimized, LuaControls.CLASSNAME);
        UserData.RegisterType<LuaAudio>(InteropAccessMode.Preoptimized, LuaAudio.CLASSNAME);
        UserData.RegisterType<LuaHud>(InteropAccessMode.Preoptimized, LuaHud.CLASSNAME);
        UserData.RegisterType<LuaScreen>(InteropAccessMode.Preoptimized, LuaScreen.CLASSNAME);
        UserData.RegisterType<LuaG>(InteropAccessMode.Preoptimized, LuaG.CLASSNAME);

        UserData.RegisterType<LuaRenderer>(InteropAccessMode.Preoptimized, LuaRenderer.CLASSNAME);
        UserData.RegisterType<LuaSprite>(InteropAccessMode.Preoptimized, LuaFrameSprite.CLASSNAME);
        UserData.RegisterType<LuaFrameSprite>(InteropAccessMode.Preoptimized, LuaFrameSprite.CLASSNAME);
        UserData.RegisterType<LuaPlainTextSprite>(InteropAccessMode.Preoptimized, LuaPlainTextSprite.CLASSNAME);
        UserData.RegisterType<LuaFont>(InteropAccessMode.Preoptimized, LuaFont.CLASSNAME);
        UserData.RegisterType<LuaHudElement>(InteropAccessMode.Preoptimized, LuaHudElement.CLASSNAME);
        UserData.RegisterType<LuaEntity>(InteropAccessMode.Preoptimized, LuaEntity.CLASSNAME);
    }

    public static void RegisterGlobals (Script script) {
        if (Screen.MainRenderer is null) {
            throw new("Cannot load Lua scripts yet.");
        }

        foreach (var e in _enums) {
            var tbl = new Table(script);

            foreach (var f in e.Fields) {
                tbl[f.Name] = f.Value;
            }

            script.Globals[e.TableName] = tbl;
        }

        script.Globals[LuaVec2.CLASSNAME] = UserData.CreateStatic(typeof(LuaVec2));
        script.Globals[LuaRect.CLASSNAME] = UserData.CreateStatic(typeof(LuaRect));
        script.Globals[LuaColor.CLASSNAME] = UserData.CreateStatic(typeof(LuaColor));
        script.Globals[LuaLogger.CLASSNAME] = UserData.CreateStatic(typeof(LuaLogger));
        script.Globals[LuaFmt.CLASSNAME] = UserData.CreateStatic(typeof(LuaFmt));
        script.Globals[LuaControls.CLASSNAME] = UserData.CreateStatic(typeof(LuaControls));
        script.Globals[LuaAudio.CLASSNAME] = UserData.CreateStatic(typeof(LuaAudio));
        script.Globals[LuaHud.CLASSNAME] = UserData.CreateStatic(typeof(LuaHud));
        script.Globals[LuaScreen.CLASSNAME] = UserData.CreateStatic(typeof(LuaScreen));
        script.Globals[LuaG.CLASSNAME] = UserData.CreateStatic(typeof(LuaG));
        script.Globals[LuaEntity.CLASSNAME] = UserData.CreateStatic(typeof(LuaEntity));

        script.Globals["loc"] = (Func<string, string>)((txt) => Localization.Text(txt));

        script.Globals["renderer"] = new LuaRenderer(Screen.MainRenderer);
    }

    public static void RegisterEntityInteraction (Script script, Entity target) {
        LuaEntity luaTarget = new(target);
        LuaEntity luaPlayer = new(G.World.Player);

        script.Globals["target"] = luaTarget;
        script.Globals["player"] = luaPlayer;
    }

    public static void RegisterScreenHandler (Script script, ScriptScreenLayer screen) {
        Table tbl = new(script);
        tbl["close"] = (Action)screen.Close;

        script.Globals["target"] = tbl;

        script.DoString(
            @"function target:open() end
            function target:draw() end
            function target:handle_input() end"
        );
    }

    public static void RegisterHudElementHandler (Script script, ScriptHudElement element) {
        Table tbl = new(script);
        tbl["close"] = (Action)element.Close;

        script.Globals["target"] = tbl;

        script.DoString(
            @"function target:open() end
            function target:update() end
            function target:draw() end
            function target:handle_input() end"
        );
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
}


readonly record struct EnumField(string Name, DynValue Value);

class LuaEnum (string tableName, List<EnumField> fields) {
    public string TableName { get; } = tableName;
    public List<EnumField> Fields { get; } = fields;
}