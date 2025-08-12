using battlesdk.game;
using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaInventoryItem : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "InventoryItem";

    public string item_id;
    public int amount;

    public LuaInventoryItem (InventoryItem item) {
        item_id = item.ItemId;
        amount = item.Amount;
    }
    public LuaInventoryItem (string item_id, int amount) {
        this.item_id = item_id;
        this.amount = amount;
    }

    public static LuaInventoryItem @new (string item_id, int amount) {
        return new(item_id, amount);
    }

    [MoonSharpHidden]
    public InventoryItem ToNative () {
        return new(item_id, amount);
    }

    public override string ToString () {
        return ToNative().ToString();
    }

    public string str () => ToString();
}
