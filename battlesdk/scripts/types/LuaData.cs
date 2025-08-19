using MoonSharp.Interpreter;
using NLog;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaData {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    [MoonSharpHidden]
    public const string CLASSNAME = "Data";

    public static List<string> message_frames => Data.Misc.MessageFrames;
    public static List<string> box_frames => Data.Misc.BoxFrames;

    public static LuaItem? get_item (string item_id) {
        if (Registry.Items.TryGetValue(item_id, out var item)) {
            return new(item);
        }
        
        return null;
    }

    /// <summary>
    /// Returns the name of the pocket the item given naturally belongs to.
    /// </summary>
    /// <param name="item_id">The id of the item to check.</param>
    /// <returns></returns>
    /// <exception cref="ScriptRuntimeException"></exception>
    public static string get_item_pocket (string item_id) {
        if (Registry.Items.TryGetValue(item_id, out var item) == false) {
            throw new ScriptRuntimeException($"Item '{item_id}' does not exist.");
        }

        return Registry.Bag.SectionNames[item.Category.BagSection];
    }
}
