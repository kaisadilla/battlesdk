using NLog;

namespace battlesdk.game;
public class Inventory {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    
    /// <summary>
    /// Contains a list of dictionaries. Each index in the list represents the
    /// index of a bag section. Each dictionary contains a list of keys, which
    /// correspond to ids of items contained in that section; and maps each
    /// item id with the amount this inventory has.
    /// </summary>
    private readonly List<List<InventoryItem>> _sections = [];

    private readonly HashSet<string> _favorites = [];

    public Inventory () {
        for (int i = 0; i < Registry.Bag.SectionNames.Count; i++) {
            _sections.Add([]);
        }
    }

    public static Inventory Load () {
        Inventory inv = new();

        return inv;
    }

    /// <summary>
    /// Gets the amount of the given item contained in this inventory.
    /// </summary>
    /// <param name="itemId">The id of the item to check.</param>
    public int GetAmount (string itemId) {
        foreach (var s in _sections) {
            foreach (var i in s) {
                if (i.ItemId == itemId) return i.Amount;
            }
        }

        return 0;
    }

    /// <summary>
    /// Adds to this inventory an amount of a specific item. If this would make
    /// the amount go beyond <see cref="Settings.MaxItem"/>, its amount is
    /// capped at that value. Returns the amount of items actually added.
    /// </summary>
    /// <param name="itemId">The id of the item to add to.</param>
    /// <param name="amount">The amount of items to add.</param>
    public int AddAmount (string itemId, int amount) {
        if (amount == 0) return 0;

        // If the player already has this item, increase its amount.
        foreach (var s in _sections) {
            for (var i = 0; i < s.Count; i++) {
                if (s[i].ItemId != itemId) continue;

                var upd = Math.Min(Settings.MaxItem, s[i].Amount + amount);
                s[i] = new(itemId, upd);
                return upd;
            }
        }

        if (Registry.Items.TryGetValue(itemId, out var item) == false) {
            _logger.Error($"Item '{itemId}' does not exist.");
            return -1;
        }

        // Else, create an entry for this item.
        _sections[item.Category.BagSection].Add(new(item.Id, amount));
        return amount;
    }

    /// <summary>
    /// Removes from this inventory an amount of a specific item. If this makes
    /// the amount reach 0 or below, the item is completely removed from the
    /// inventory. Returns the amount of items actually removed.
    /// </summary>
    /// <param name="itemId">The id of the item to add to.</param>
    /// <param name="amount">The amount of items to add.</param>
    public int RemoveAmount (string itemId, int amount) {
        if (amount == 0) return 0;

        foreach (var s in _sections) {
            for (var i = 0; i < s.Count; i++) {
                if (s[i].ItemId != itemId) continue;

                int upd = s[i].Amount - amount;

                if (upd > 0) {
                    s[i] = new(itemId, upd);
                    return upd;
                }
                else {
                    s.RemoveAt(i);
                    return 0;
                }
            }
        }

        return 0;
    }

    /// <summary>
    /// Marks the given item as a favorite, if it isn't already.
    /// </summary>
    /// <param name="itemId">The id of the item to make a favorite.</param>
    public void AddFavorite (string itemId) {
        _favorites.Add(itemId);
    }

    /// <summary>
    /// Makes the item no longer a favoite.
    /// </summary>
    /// <param name="itemId">The id of the item to remove from favorites.</param>
    public void RemoveFavorite (string itemId) {
        _favorites.Remove(itemId);
    }

    /// <summary>
    /// Returns the items in the given section.
    /// </summary>
    /// <param name="section">The index of the section where the items are.</param>
    public IReadOnlyList<InventoryItem> GetItemsAt (int section) {
        return _sections[section];
    }

    /// <summary>
    /// Returns a list of favorite items that this inventory contains (i.e.
    /// favorite items not contained in this inventory are not included).
    /// </summary>
    /// <returns></returns>
    public List<InventoryItem> GetFavoriteItems () {
        List<InventoryItem> items = [];

        foreach (var s in _sections) {
            foreach (var i in s) {
                if (_favorites.Contains(i.ItemId)) {
                    items.Add(i);
                }
            }
        }

        return items;
    }
}

public readonly struct InventoryItem (string itemId, int amount) {
    public string ItemId { get; } = itemId;
    public int Amount { get; } = amount;

    public override string ToString () {
        return $"{ItemId} x{Amount}";
    }
}