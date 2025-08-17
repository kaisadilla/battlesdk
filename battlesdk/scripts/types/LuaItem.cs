using battlesdk.data;
using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaItem : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "Item";

    [MoonSharpHidden]
    public ItemData Item { get; }
    
    /// <summary>
    /// The id of the category this item belongs to.
    /// </summary>
    public string category_id => Item.Category.Id;
    /// <summary>
    /// The item's price. This value can contain decimals.
    /// </summary>
    public float price => Item.Price;

    public LuaItem (ItemData item) {
        Item = item;
    }

    public override string ToString () {
        return Item.ToString();
    }

    public string str () => ToString();
}
