using System.Text.Json.Serialization;

namespace battlesdk.data.definitions;
public class ItemCategoryDefinition {
    public required string Section { get; init; }

    [JsonPropertyName("is_default")]
    public bool? IsDefault { get; init; }
}
