using battlesdk.world.entities.interaction;

namespace battlesdk.data.definitions;

public enum EntityInteractionType {
    Script,
    Message,
}

public class EntityInteractionDefinition {
    public required EntityInteractionType Type { get; init; }
    public InteractionTrigger? Trigger { get; init; }
    public string? Script { get; init; } = null;
    public string? TextKey { get; init; } = null;
    public List<string>? TextKeys { get; init; } = null;
}