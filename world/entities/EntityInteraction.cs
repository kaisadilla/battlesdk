using battlesdk.data;
using battlesdk.scripts;
using MoonSharp.Interpreter;
using NLog;

namespace battlesdk.world.entities;

public abstract class EntityInteraction {
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
            _ => throw new NotImplementedException(),
        };
    }

    public virtual void Interact (Direction from) {
        Audio.PlayBeepShort();

        if (LookToSource) {
            _target.SetDirection(from);
        }
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

        LuaGlobalFunctions.RegisterGlobals(_lua, script);

        LuaEntity luaCh = new(_target);
        luaCh.Register(_lua, script, "target");
        LuaEntity luaPlayer = new(G.World.Player);
        luaPlayer.Register(_lua, script, "player");

        _scriptFunc = _lua.LoadString(srcStr);
    }

    public override void Interact (Direction from) {
        if (IsInteracting == true) return;

        base.Interact(from);

        if (_scriptFunc is null) return;

        IsInteracting = true;

        InputManager.PushBlock();

        _lua.Call(_scriptFunc);

        ScriptLoop.EnqueueScriptEnd(() => {
            IsInteracting = false;
            InputManager.Pop();
        });
    }
}
