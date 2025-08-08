using battlesdk.world.entities;
using System.Text.Json.Serialization;

namespace battlesdk.data.definitions;

public enum EntityType {
    Npc,
    Warp,
}

public class EntityDefinition {
    /// <summary>
    /// This entity's type.
    /// </summary>
    public required EntityType Type { get; init; }
    /// <summary>
    /// A name for this entity. If present, the entity can be identified by
    /// that name.
    /// </summary>
    public string? Name { get; init; }
    /// <summary>
    /// The sprite to use for this entity, if any.
    /// </summary>
    public string? Sprite { get; init; }
    /// <summary>
    /// The position in the map the entity is in.
    /// </summary>
    public required IVec2 Position { get; init; }
    /// <summary>
    /// A description of the interaction this entity has.
    /// </summary>
    public EntityInteractionDefinition? Interaction { get; init; } = null;
    /// <summary>
    /// If the entity is a character, its movement.
    /// </summary>
    public CharacterMovementDefinition? Movement { get; init; } = null;

    /// <summary>
    /// If this entity is a warp, the kind of warp that it is.
    /// </summary>
    [JsonPropertyName("warp_type")]
    public WarpType? WarpType { get; init; }
    /// <summary>
    /// If this entity is a warp, the sound played when the player interacts
    /// with it.
    /// </summary>
    [JsonPropertyName("entry_sound")]
    public string? EntrySound { get; init; }
    /// <summary>
    /// If this entity is a warp, the map this warp transports to.
    /// </summary>
    [JsonPropertyName("target_map")]
    public string? TargetMap { get; init; }
    /// <summary>
    /// If this entity is a warp, the position in the target map it transports to.
    /// </summary>
    [JsonPropertyName("target_position")]
    public IVec2? TargetPosition { get; init; }
    /// <summary>
    /// If this entity is a warp, the entity to whose position this warp
    /// transports to. This field has precedence over <see cref="TargetPosition"/>.
    /// </summary>
    [JsonPropertyName("target_entity")]
    public string? TargetEntity { get; init; }
}
