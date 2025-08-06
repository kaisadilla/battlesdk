namespace battlesdk.data.definitions;

public enum EntityType {
    Npc,
}

public class EntityDefinition {
    public required EntityType Type { get; init; }
    public required string Sprite { get; init; }
    public required IVec2 Position { get; init; }
    public EntityInteractionDefinition? Interaction { get; init; } = null;
    public CharacterMovementDefinition? Movement { get; init; } = null;
}
