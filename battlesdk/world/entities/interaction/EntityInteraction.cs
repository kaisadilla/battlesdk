using battlesdk.data;
using NLog;

namespace battlesdk.world.entities.interaction;

public enum InteractionTrigger {
    /// <summary>
    /// This interaction triggers when the player, while being next to the
    /// entity and looking towards it, presses the <see cref="ActionKey.Primary"/>
    /// button.
    /// </summary>
    ActionButton,
    /// <summary>
    /// This interaction triggers when the player tries to move into the
    /// position the entity is in.This is the reserve of
    /// <see cref="EntityTouchesPlayer"/>.
    /// </summary>
    PlayerTouchesEntity,
    /// <summary>
    /// This interaction triggers when this entity tries to move into the
    /// position the player is in. This is the reserve of
    /// <see cref="PlayerTouchesEntity"/>.
    /// </summary>
    EntityTouchesPlayer,
}

public abstract class EntityInteraction {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The event that triggers this interaction.
    /// </summary>
    public InteractionTrigger Trigger { get; protected set; }

    /// <summary>
    /// True if this entity's interaction is currently ongoing.
    /// </summary>
    public bool IsInteracting { get; protected set; } = false;

    /// <summary>
    /// If true, this entity will change its direction to look towards the
    /// player when the player interacts with it.
    /// </summary>
    public bool LookToSource { get; init; } = true;

    protected Entity _target;

    public EntityInteraction (Entity entity) {
        _target = entity;
    }

    public static EntityInteraction New (Entity entity, EntityInteractionData data) {
        return data switch {
            ScriptEntityInteractionData d => new ScriptEntityInteraction(entity, d),
            MessageEntityInteractionData d => new MessageEntityInteraction(entity, d),
            _ => throw new NotImplementedException(),
        };
    }

    public virtual void Interact (Direction from) {
        InputManager.PushBlock();
        Audio.PlayBeepShort();

        if (LookToSource) {
            _target.SetDirection(from);
        }
    }

    public virtual void OnPlayerTouchEvent (Direction from) {
        if (Trigger == InteractionTrigger.PlayerTouchesEntity) {
            Interact(from);
        }
    }

    /// <summary>
    /// Enqueues the end of this interaction in the script loop. This callback
    /// will set the <see cref="IsInteracting"/> flag to false and pop an
    /// input listener, which means it should only be used if the base
    /// <see cref="Interact(Direction)"/> method has been called, or an input
    /// listener has been pushed manually.
    /// </summary>
    protected void End () {
        _logger.Debug("NPC is no longer interacting.");
        IsInteracting = false;
        InputManager.Pop();
    }
}
