using MoonSharp.Interpreter;
using NLog;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaData {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    [MoonSharpHidden]
    public const string CLASSNAME = "Data";

    public static LuaItem? get_item (string item_id) {
        if (Registry.Items.TryGetValue(item_id, out var item)) {
            return new(item);
        }
        
        return null;
    }
}
