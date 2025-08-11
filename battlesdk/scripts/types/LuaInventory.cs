using battlesdk.game;
using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaInventory : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "Inventory";

    [MoonSharpHidden]
    private readonly Inventory _inventory;

    [MoonSharpHidden]
    public LuaInventory (Inventory inventory) {
        _inventory = inventory;
    }

    public int get_amount (string item_id) {
        return _inventory.GetAmount(item_id);
    }

    public int add_amount (string item_id, int amount) {
        return _inventory.AddAmount(item_id, amount);
    }

    public int remove_amount (string item_id, int amount) {
        return _inventory.RemoveAmount(item_id, amount);
    }

    public void add_favorite (string item_id) {
        _inventory.AddFavorite(item_id);
    }

    public void remove_favorite (string item_id) {
        _inventory.RemoveFavorite(item_id);
    }

    public List<LuaInventoryItem>? get_items_at (int index) {
        return _inventory.GetItemsAt(index - 1).Select(i => new LuaInventoryItem(i)).ToList();
    }

    public List<LuaInventoryItem> get_favorite_items () {
        return _inventory.GetFavoriteItems().Select(i => new LuaInventoryItem(i)).ToList();
    }

    public override string ToString () {
        return $"<inventory>";
    }
}
