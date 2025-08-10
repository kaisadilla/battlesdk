using battlesdk.world;
using System.Text.Json.Serialization;

namespace battlesdk.data.definitions;

public enum CharacterMovementType {
    Route,
    Random,
    [JsonStringEnumMemberName("look_around")]
    LookAround,
}

public class CharacterMovementDefinition {
    public required CharacterMovementType Type { get; init; }
    public List<MoveKind>? Route { get; init; } = null;
    [JsonPropertyName("ignore_entities")]
    public bool? IgnoreEntities { get; init; } = null;
}
