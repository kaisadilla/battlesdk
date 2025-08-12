using battlesdk.data;
using battlesdk.scripts;
using MoonSharp.Interpreter;
using NLog;

namespace battlesdk.world.entities.interaction;

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

    private CoroutineTask CompleteLuaCoroutine (LuaCoroutine luaCor) {
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
