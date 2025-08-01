namespace battlesdk.world;

/// <summary>
/// Represents a move that can be executed by a character.
/// </summary>
/// <param name="Move">The kind of move to do.</param>
/// <param name="IgnoreCharacters">If true, the move can ignore characters.</param>
public readonly record struct CharacterMove (
    MoveKind Move,
    bool IgnoreCharacters
);

public enum MoveKind {
    StepDown,
    StepRight,
    StepUp,
    StepLeft,
    LookDown,
    LookRight,
    LookUp,
    LookLeft,
    Jump,
}