using battlesdk.data;

namespace battlesdk.world.entities;

public abstract class Entity {
    /// <summary>
    /// The behavior called when interacting with this entity via the primary
    /// action. If this entity has no interaction, this value is null.
    /// </summary>
    protected EntityInteraction? _interaction = null;

    /// <summary>
    /// The id of the map asset that provided this entity.
    /// </summary>
    public int MapId { get; protected set; }
    /// <summary>
    /// The id of the entity data that provided this entity.
    /// </summary>
    public int EntityId { get; protected set; }

    /// <summary>
    /// This entity's position in the world.
    /// </summary>
    public IVec2 Position { get; private set; }
    /// <summary>
    /// The direction this entity is looking at.
    /// </summary>
    public Direction Direction { get; private set; } = Direction.Down;

    /// <summary>
    /// The id of this entity's sprite.
    /// </summary>
    public int Sprite { get; private set; }

    /// <summary>
    /// This entity's Z position, which indicates the height at which it
    /// currently is. Note: use <see cref="VisualZ"/> to get the z position the
    /// character visually is.
    /// </summary>
    public int Z { get; private set; } = 0;
    /// <summary>
    /// The visual z-index of the entity. This is usually the same as its
    /// logical Z position, but can difer if the character has changed its Z
    /// index in its last move.
    /// </summary>
    public int VisualZ { get; private set; } = 0;

    /// <summary>
    /// The position the entity visually is, taking into consideration when
    /// it's moving.
    /// </summary>
    public virtual Vec2 Subposition => Position;

    public Entity (int mapId, int entityId, IVec2 worldPos, string sprite) {
        MapId = mapId;
        EntityId = entityId;
        Position = worldPos;
        if (Registry.CharSprites.TryGetId(sprite, out var spriteId)) {
            Sprite = spriteId;
        }
        else {
            throw new Exception($"Sprite '{sprite}' doesn't exist.");
        }
    }

    public Entity (int mapId, int entityId, GameMap map, EntityData data) {
        MapId = mapId;
        EntityId = entityId;
        Position = map.GetWorldPos(data.Position);
        Sprite = data.Sprite;

        if (data.Interaction is not null) {
            _interaction = EntityInteraction.New(this, data.Interaction);
        }
    }

    public virtual void FrameStart () {

    }

    public virtual void Update () {

    }

    /// <summary>
    /// The player interacts with this entity. The behavior of this interaction
    /// depends on the entity. For example, an NPC may open a textbox with a
    /// line of text if configured like so.
    /// </summary>
    public virtual void Interact (Direction from) {
        if (_interaction is null) return;

        _interaction?.Interact(from);
    }

    /// <summary>
    /// Instantly moves this entity to a new position.
    /// </summary>
    /// <param name="worldPos">The position in the world to move this entity to.</param>
    public virtual void SetPosition (IVec2 worldPos) {
        Position = worldPos;
        HandleLandAtPosition();
    }

    /// <summary>
    /// Instantly turns this entity to the given direction.
    /// </summary>
    /// <param name="direction">A direction for this entity to face.</param>
    public virtual void SetDirection (Direction direction) {
        Direction = direction;
    }

    /// <summary>
    /// Returns the position directly in front of this entity - that is, the
    /// closest tile in the direction the entity is looking at right now.
    /// If its direction is invalid, this method returns the entity's position.
    /// </summary>
    public virtual IVec2 GetPositionInFront () {
        return Direction switch {
            Direction.Down => Position + new IVec2(0, 1),
            Direction.Right => Position + new IVec2(1, 0),
            Direction.Up => Position + new IVec2(0, -1),
            Direction.Left => Position + new IVec2(-1, 0),
            _ => Position,
        };
    }

    /// <summary>
    /// Triggers effects linked to arriving to the position this entity is
    /// currently at.
    /// </summary>
    protected void HandleLandAtPosition () {
        var zWarp = G.World.GetZWarpAt(Position);
        if (zWarp is null) {
            VisualZ = Z;
            return;
        }

        VisualZ = Math.Max(Z, zWarp.Value);
        Z = zWarp.Value;
    }
}
