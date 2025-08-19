global using LuaCoroutine = MoonSharp.Interpreter.Coroutine;
using battlesdk.graphics.elements;
using battlesdk.hud;
using battlesdk.screen;
using battlesdk.scripts.types;
using battlesdk.world.entities;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using NLog;

namespace battlesdk.scripts;
public static class Lua {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static readonly List<LuaEnum> _enums = [];

    private delegate string LocCallback (string key, params object[] args);

    public static void Init () {
        RegisterEnumTable<ActionKey>("ActionKey");
        RegisterEnumTable<Direction>("Direction");
        RegisterEnumTable<AnchorPoint>("AnchorPoint");

        UserData.RegisterType<LuaLogger>(InteropAccessMode.Preoptimized, LuaLogger.CLASSNAME);
        UserData.RegisterType<LuaScript>(InteropAccessMode.Preoptimized, LuaScript.CLASSNAME);
        UserData.RegisterType<LuaFmt>(InteropAccessMode.Preoptimized, LuaFmt.CLASSNAME);
        UserData.RegisterType<LuaSettings>(InteropAccessMode.Preoptimized, LuaSettings.CLASSNAME);
        UserData.RegisterType<LuaControls>(InteropAccessMode.Preoptimized, LuaControls.CLASSNAME);
        UserData.RegisterType<LuaAudio>(InteropAccessMode.Preoptimized, LuaAudio.CLASSNAME);
        UserData.RegisterType<LuaHud>(InteropAccessMode.Preoptimized, LuaHud.CLASSNAME);
        UserData.RegisterType<LuaScreen>(InteropAccessMode.Preoptimized, LuaScreen.CLASSNAME);
        UserData.RegisterType<LuaData>(InteropAccessMode.Preoptimized, LuaData.CLASSNAME);
        UserData.RegisterType<LuaG>(InteropAccessMode.Preoptimized, LuaG.CLASSNAME);
        UserData.RegisterType<LuaGameSettings>(InteropAccessMode.Preoptimized, LuaGameSettings.CLASSNAME);

        UserData.RegisterType<LuaVec2>(InteropAccessMode.Preoptimized, LuaVec2.CLASSNAME);
        UserData.RegisterType<LuaRect>(InteropAccessMode.Preoptimized, LuaRect.CLASSNAME);
        UserData.RegisterType<LuaColor>(InteropAccessMode.Preoptimized, LuaColor.CLASSNAME);
        UserData.RegisterType<LuaObject>(new LuaObject.Descriptor());
        UserData.RegisterType<LuaList>(InteropAccessMode.Preoptimized, LuaList.CLASSNAME);
        UserData.RegisterType<LuaInventoryItem>(InteropAccessMode.Preoptimized, LuaInventoryItem.CLASSNAME);
        UserData.RegisterType<LuaItem>(InteropAccessMode.Preoptimized, LuaItem.CLASSNAME);

        UserData.RegisterType<LuaRenderer>(InteropAccessMode.Preoptimized, LuaRenderer.CLASSNAME);
        UserData.RegisterType<LuaSprite>(InteropAccessMode.Preoptimized, LuaFrameSprite.CLASSNAME);
        UserData.RegisterType<LuaFrameSprite>(InteropAccessMode.Preoptimized, LuaFrameSprite.CLASSNAME);
        UserData.RegisterType<LuaPlainTextSprite>(InteropAccessMode.Default, LuaPlainTextSprite.CLASSNAME);
        UserData.RegisterType<LuaFont>(InteropAccessMode.Preoptimized, LuaFont.CLASSNAME);
        UserData.RegisterType<LuaTextbox>(InteropAccessMode.Preoptimized, LuaTextbox.CLASSNAME);
        UserData.RegisterType<LuaAnimatableTextbox>(InteropAccessMode.Preoptimized, LuaAnimatableTextbox.CLASSNAME);
        UserData.RegisterType<LuaChoiceBox>(InteropAccessMode.Preoptimized, LuaChoiceBox.CLASSNAME);
        UserData.RegisterType<LuaScrollbar>(InteropAccessMode.Preoptimized, LuaScrollbar.CLASSNAME);
        UserData.RegisterType<LuaScriptGraphicElement>(InteropAccessMode.Preoptimized, LuaScriptGraphicElement.CLASSNAME);
        UserData.RegisterType<LuaHudElement>(InteropAccessMode.Preoptimized, LuaHudElement.CLASSNAME);
        UserData.RegisterType<LuaMessageHudElement>(InteropAccessMode.Preoptimized, LuaMessageHudElement.CLASSNAME);
        UserData.RegisterType<LuaScriptHudElement>(InteropAccessMode.Preoptimized, LuaScriptHudElement.CLASSNAME);
        UserData.RegisterType<LuaEntity>(InteropAccessMode.Preoptimized, LuaEntity.CLASSNAME);
        UserData.RegisterType<LuaInventory>(InteropAccessMode.Preoptimized, LuaInventory.CLASSNAME);
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

        script.Globals[LuaVec2.CLASSNAME] = UserData.CreateStatic<LuaVec2>();
        script.Globals[LuaRect.CLASSNAME] = UserData.CreateStatic<LuaRect>();
        script.Globals[LuaColor.CLASSNAME] = UserData.CreateStatic<LuaColor>();
        script.Globals[LuaObject.CLASSNAME] = UserData.CreateStatic<LuaObject>();
        script.Globals[LuaList.CLASSNAME] = UserData.CreateStatic<LuaList>();
        script.Globals[LuaInventoryItem.CLASSNAME] = UserData.CreateStatic<LuaInventoryItem>();
        script.Globals[LuaLogger.CLASSNAME] = UserData.CreateStatic<LuaLogger>();
        script.Globals[LuaScript.CLASSNAME] = UserData.CreateStatic<LuaScript>();
        script.Globals[LuaFmt.CLASSNAME] = UserData.CreateStatic<LuaFmt>();
        script.Globals[LuaSettings.CLASSNAME] = UserData.CreateStatic<LuaSettings>();
        script.Globals[LuaControls.CLASSNAME] = UserData.CreateStatic<LuaControls>();
        script.Globals[LuaAudio.CLASSNAME] = UserData.CreateStatic<LuaAudio>();
        script.Globals[LuaHud.CLASSNAME] = UserData.CreateStatic<LuaHud>();
        script.Globals[LuaScreen.CLASSNAME] = UserData.CreateStatic<LuaScreen>();
        script.Globals[LuaData.CLASSNAME] = UserData.CreateStatic<LuaData>();
        script.Globals[LuaG.CLASSNAME] = UserData.CreateStatic<LuaG>();
        script.Globals[LuaGameSettings.CLASSNAME] = UserData.CreateStatic<LuaGameSettings>();
        script.Globals[LuaEntity.CLASSNAME] = UserData.CreateStatic<LuaEntity>();
        script.Globals[LuaScriptGraphicElement.CLASSNAME] = UserData.CreateStatic<LuaScriptGraphicElement>();

        script.Globals["loc"] = (LocCallback)Localization.Text;

        script.Globals["renderer"] = new LuaRenderer(Screen.MainRenderer);

        script.Options.ScriptLoader = new FileSystemScriptLoader() {
            ModulePaths = [
                "res/scripts/?.lua",
                "res/scripts/?/?.lua",
            ],
        };

        foreach (var s in Registry.Scripts) {
            if (s.Name.StartsWith("global/") == false) continue;

            script.DoString(s.GetSource());
        }
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
            function target:update() end
            function target:draw() end
            function target:handle_input() end"
        );
    }

    public static void RegisterHudElementHandler (Script script, ScriptHudElement element) {
        Table tbl = new(script);
        tbl["close"] = (Action)element.Close;
        tbl["set_result"] = (Action<DynValue>)element.SetResult;

        script.Globals["target"] = tbl;

        script.DoString(
            @"function target:open() end
            function target:update() end
            function target:draw() end
            function target:handle_input() end"
        );
    }

    public static void RegisterGraphicElementHandler (Script script, ScriptGraphicElement element) {
        Table tbl = new(script);

        script.Globals["target"] = tbl;

        script.DoString(
            @"function target:open() end
            function target:update() end
            function target:draw() end"
        );
    }

    public static void RegisterTransitionHandler (Script script, ScriptTransition transition) {
        Table tbl = new(script);

        script.Globals["target"] = tbl;

        script.DoString(
            @"function target:update() end"
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