namespace battlesdk.data.definitions;

public enum EntityInteractionType {
    Script,
    Message,
}

public class EntityInteractionDefinition {
    public required EntityInteractionType Type { get; init; }
    public string? Script { get; init; } = null;
    public string? Text { get; init; } = null;
}