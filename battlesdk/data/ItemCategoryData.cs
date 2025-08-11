using battlesdk.data.definitions;

namespace battlesdk.data;
public class ItemCategoryData {
    public string Id { get; }
    public int BagSection { get; }
    public bool IsDefault { get; }

    public ItemCategoryData (string id, ItemCategoryDefinition def) {
        if (Registry.Bag.SectionIndices.TryGetValue(
            def.Section, out int sectionIndex
        ) == false) {
            throw new($"Invalid bag section: '{def.Section}'.");
        }

        Id = id;
        BagSection = sectionIndex;
        IsDefault = def.IsDefault ?? false;
    }
}
