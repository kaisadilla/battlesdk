using battlesdk.data;
using battlesdk.scripts.types;
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

public class ScriptEntityInteraction : EntityInteraction {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private LuaScriptHost _lua;

    public ScriptEntityInteraction (Entity entity, ScriptAsset script) : base(entity) {
        _lua = LuaScriptHost.EntityInteractionScript(script, _target);
    }

    public ScriptEntityInteraction (
        Entity entity, ScriptEntityInteractionData data
    ) : base(entity) {
        if (Registry.Scripts.TryGetElement(data.ScriptId, out var script) == false) {
            throw new InvalidDataException(
                $"Failed to retrieve script #'{data.ScriptId}'."
            );
        }

        _lua = LuaScriptHost.EntityInteractionScript(script, _target);
    }

    public override void Interact (Direction from) {
        if (IsInteracting == true) return;

        base.Interact(from);

        IsInteracting = true;

        var id = _lua.RunAsync();
        _lua.OnExecutionComplete += _Callback;

        void _Callback (object? sender, LuaScriptHost.EventArgs args) {
            if (args.Id != id) return;

            End();
            _lua.OnExecutionComplete -= _Callback;
        }
    }

    private CoroutineTask CompleteLuaCoroutine (MoonSharp.Interpreter.Coroutine luaCor) {
        luaCor.Resume();
        while (true) {
            if (luaCor.State == CoroutineState.Dead) {
                End();
                break;
            }
            else {
                yield return null;
            }
        }
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

        ShowNextTextbox(0);
    }

    private void ShowNextTextbox (int index) {
        if (index >= _textKeys.Count) {
            End();
            return;
        }

        var tb = Hud.ShowMessage(Localization.Text(_textKeys[index]));
        tb.OnClose += (s, evt) => ShowNextTextbox(index + 1);
    }
}

public class DoorEntityInteraction : EntityInteraction {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private int _worldId;
    private int _mapId;
    private IVec2 _targetPos;

    public DoorEntityInteraction (Entity entity, WarpData data) : base(entity) {
        Trigger = InteractionTrigger.PlayerTouchesEntity; // TODO: Not hardcoded.
        _worldId = data.WorldId;
        _mapId = data.MapId;
        _targetPos = data.TargetPosition;

        if (data.TargetEntity is not null) {
            MapAsset map;
            if (_worldId != -1) {
                map = Registry.Maps[Registry.Worlds[_worldId].Maps[_mapId].Id];
            }
            else {
                map = Registry.Maps[_mapId];
            }

            bool found = false;
            foreach (var e in map.EnumerateEntities()) {
                if (e.Name is not null && e.Name == data.TargetEntity) {
                    _targetPos = e.Position;
                    found = true;
                    break;
                }
            }

            if (found == false) {
                _logger.Error(
                    $"Couldn't find entity named '{data.TargetEntity}' in map {map.Name}."
                );
            }
        }
    }

    public override void Interact (Direction from) {
        if (IsInteracting == true) return;
        IsInteracting = true;

        InputManager.PushBlock();

        if (_target is Warp warp) {
            if (warp.WarpType == WarpType.Door) {
                Coroutine.Start(DoorAnimation(from));
            }
            else if (warp.WarpType == WarpType.Hall) {
                Coroutine.Start(HallAnimation(from));
            }
        }
    }

    private CoroutineTask HallAnimation (Direction from) {
        G.World.Player.SetDirection(from.Opposite());

        PlayEntrySound();
        G.World.Player.SetRunning(false);
        G.World.Player.Move(G.World.Player.Position.OffsetAt(from.Opposite()));
        Screen.FadeToBlack(0.5f);
        yield return new WaitForSeconds(0.2f);

        G.World.Player.IsInvisible = true;
        yield return new WaitForSeconds(0.5f);

        if (_worldId == -1) {
            var map = Registry.Maps[_mapId];
            G.World.TransferTo(map, _targetPos);
        }
        else {
            var world = Registry.Worlds[_worldId];
            G.World.TransferTo(world, _targetPos);
        }

        End();
    }

    private CoroutineTask DoorAnimation (Direction from) {
        G.World.Player.SetDirection(from.Opposite());

        PlayEntrySound();

        if (_target.Sprite is SpritesheetFile) {
            for (int i = 1; i <= 3; i++) {
                _target.SetSpriteIndex(i);
                yield return new WaitForSeconds(0.12f);
            }
        }

        G.World.Player.SetRunning(false);
        G.World.Player.Move(G.World.Player.Position.OffsetAt(from.Opposite()));
        yield return new WaitForSeconds(0.4f);
        G.World.Player.IsInvisible = true;
        yield return new WaitForSeconds(0.2f);

        Screen.FadeToBlack(0.5f);
        if (_target.Sprite is SpritesheetFile) {
            for (int i = 2; i >= 0; i--) {
                _target.SetSpriteIndex(i);
                yield return new WaitForSeconds(0.12f);
            }
        }

        yield return new WaitForSeconds(0.25f);

        if (_worldId == -1) {
            var map = Registry.Maps[_mapId];
            G.World.TransferTo(map, _targetPos);
        }
        else {
            var world = Registry.Worlds[_worldId];
            G.World.TransferTo(world, _targetPos);
        }

        End();
    }

    private void PlayEntrySound () {
        if (_target is Warp warp && warp.EntrySound is not null) {
            Audio.Play(warp.EntrySound.Value);
        }
    }
}
