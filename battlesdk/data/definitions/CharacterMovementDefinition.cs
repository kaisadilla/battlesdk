using battlesdk.world;

namespace battlesdk.data.definitions;

public enum CharacterMovementType {
    Route,
    Random,
    LookAround,
}

public class CharacterMovementDefinition {
    public required CharacterMovementType Type { get; init; }
    public List<MoveKind>? Route { get; init; } = null;
}