using battlesdk.data;
using battlesdk.scripts;
using MoonSharp.Interpreter;
using NLog;

namespace battlesdk.world.entities;

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
    protected void EnqueueEnd () {
        ScriptLoop.EnqueueScriptEnd(() => {
            IsInteracting = false;
            InputManager.Pop();
        });
    }
}

public class ScriptEntityInteraction : EntityInteraction {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The Lua VM that will execute this interaction's script.
    /// </summary>
    private Script _lua = new();
    /// <summary>
    /// Contains the compiled script as a Lua function.
    /// </summary>
    private DynValue? _scriptFunc;

    public ScriptEntityInteraction (Entity entity, ScriptAsset script) : base(entity) {
        LoadScript(script);
    }

    public ScriptEntityInteraction (
        Entity entity, ScriptEntityInteractionData data
    ) : base(entity) {
        if (Registry.Scripts.TryGetElement(data.ScriptId, out var script) == false) {
            throw new InvalidDataException(
                $"Failed to retrieve script #'{data.ScriptId}'."
            );
        }

        LoadScript(script);
    }

    public void LoadScript (ScriptAsset script) {
        string srcStr;
        try {
            srcStr = script.GetSource();
        }
        catch (Exception ex) {
            _logger.Error(ex, "Failed to load script.");
            return;
        }

        Lua.RegisterGlobals(_lua);
        Lua.RegisterEntityInteraction(_lua, _target);

        _scriptFunc = _lua.LoadString(srcStr);
    }

    public override void Interact (Direction from) {
        if (IsInteracting == true) return;

        base.Interact(from);

        if (_scriptFunc is null) return;

        IsInteracting = true;

        _lua.Call(_scriptFunc);
        EnqueueEnd();
    }
}

public class MessageEntityInteraction : EntityInteraction {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private List<string> _textKeys;

    public MessageEntityInteraction (
        Entity entity, MessageEntityInteractionData data
    ) : base(entity) {
        _textKeys = [.. data.TextKeys];
    }

    public override void Interact (Direction from) {
        if (IsInteracting == true) return;
        IsInteracting = true;

        base.Interact(from);

        foreach (var k in _textKeys) {
            ScriptLoop.Enqueue(new MessageScriptEvent(Localization.Text(k)));
        }
        EnqueueEnd();
    }
}

public class DoorEntityInteraction : EntityInteraction {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private string _warpTo;
    private IVec2 _pos;

    public DoorEntityInteraction (Entity entity, string warpTo, IVec2 pos) : base(entity) {
        _warpTo = warpTo;
        _pos = pos;
        Trigger = InteractionTrigger.PlayerTouchesEntity;
    }

    public DoorEntityInteraction (
        Entity entity, MessageEntityInteractionData data
    ) : base(entity) {
    }

    public override void Interact (Direction from) {
        if (IsInteracting == true) return;
        IsInteracting = true;

        InputManager.PushBlock();

        Coroutine.Start(InteractionTask());
    }

    private CoroutineTask InteractionTask () {
        if (_target.Sprite is SpritesheetFile spr) {
            for (int i = 1; i <= 3; i++) {
                _target.SetSpriteIndex(i);
                yield return new WaitForSeconds(0.51f);
            }
        }

        EnqueueEnd();
    }
}
