using System.Text.Json.Serialization;

namespace battlesdk.data.definitions;

public enum EntityType {
    Npc,
    Warp,
}

public enum WarpType {
    Door,
}

public class EntityDefinition {
    public required EntityType Type { get; init; }
    public required string Sprite { get; init; }
    public required IVec2 Position { get; init; }
    public EntityInteractionDefinition? Interaction { get; init; } = null;
    public CharacterMovementDefinition? Movement { get; init; } = null;

    [JsonPropertyName("warp_type")]
    public WarpType? WarpType { get; init; }
    [JsonPropertyName("target_map")]
    public string? TargetMap { get; init; }
    [JsonPropertyName("target_position")]
    public IVec2? TargetPosition { get; init; }
}
