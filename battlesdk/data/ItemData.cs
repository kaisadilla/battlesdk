using battlesdk.data.definitions;
using NLog;

namespace battlesdk.data;
public class ItemData {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public string Id { get; }
    public ItemCategoryData Category { get; }
    public float Price { get; }

    public ItemData (string id, ItemDefinition def) {
        Id = id;

        if (Registry.ItemCategories.TryGetValue(def.Category, out var cat) == false) {
            _logger.Error(
                $"Invalid item category: '{def.Category}'. " +
                $"{Registry.DefaultItemCategory.Id} will be used instead."
            );
            Category = Registry.DefaultItemCategory;
        }
        else {
            Category = cat;
        }

        Price = def.Price;
    }

    public override string ToString () {
        return $"[Item '{Id}' - Cat: {Category.Id}, Price: {Price}]";
    }
}
