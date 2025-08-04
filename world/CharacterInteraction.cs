using battlesdk.data;
using battlesdk.scripts;
using battlesdk.world.entities;
using MoonSharp.Interpreter;
using NLog;

namespace battlesdk.world;

public class CharacterInteraction {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// True if the player is currently interacting with this character.
    /// </summary>
    public bool IsInteracting { get; private set; } = false;

    private Script _lua = new();
    private DynValue? _scriptFunc;

    public CharacterInteraction (Character character, ScriptAsset script) {
        string srcStr;
        try {
            srcStr = script.GetSource();
        }
        catch (Exception ex) {
            _logger.Error("Failed to load script.");
            return;
        }

        LuaGlobalFunctions.RegisterGlobals(_lua, script);

        LuaCharacter luaCh = new(character);
        luaCh.Register(_lua, script, "target");

        _scriptFunc = _lua.LoadString(srcStr);
    }

    public void Interact () {
        if (_scriptFunc is null) return;
        if (IsInteracting == true) return;

        Audio.PlaySound("beep_short");
        InputManager.PushBlock();

        IsInteracting = true;

        _lua.Call(_scriptFunc);

        ScriptLoop.EnqueueScriptEnd(() => {
            IsInteracting = false;
            InputManager.Pop();
        });
    }
}
