using battlesdk.data;
using battlesdk.world;
using battlesdk.world.entities;
using MoonSharp.Interpreter;

namespace battlesdk.scripts;

/// <summary>
/// Represents an instance of an entity in a Lua script. This class is not
/// intended to be used in C# for any purpose other than registering an entity
/// instance in a Lua script.
/// </summary>
public class LuaEntity {
    /// <summary>
    /// The entity this instance acts on.
    /// </summary>
    private readonly Entity _entity;
    /// <summary>
    /// If true, move actions queued into the script loop will ignore characters.
    /// </summary>
    private bool _ignoreCharacters = false;

    public LuaEntity (Entity entity) {
        _entity = entity;
    }

    /// <summary>
    /// Stores this instance in a global variable in the Lua VM given.
    /// </summary>
    /// <param name="script">The Lua VM.</param>
    /// <param name="asset">The script that will be executed in said VM.</param>
    /// <param name="varName">The name of the global variable to register.</param>
    public void Register (Script script, ScriptAsset asset, string varName) {
        Table tbl = new(script);

        // These functions are only valid when the entity is a character.
        if (_entity is Character) {
            tbl["move_up"] = (Action<DynValue>)MoveUp;
            tbl["move_down"] = (Action<DynValue>)MoveDown;
            tbl["move_left"] = (Action<DynValue>)MoveLeft;
            tbl["move_right"] = (Action<DynValue>)MoveRight;
            tbl["look_up"] = (Action)LookUp; // TODO: These 4 can be available for entities and shouldn't be treated as moves.
            tbl["look_down"] = (Action)LookDown;
            tbl["look_left"] = (Action)LookLeft;
            tbl["look_right"] = (Action)LookRight;
            tbl["look_towards_player"] = (Action)LookTowardsPlayer;
            tbl["jump"] = (Action<DynValue>)Jump;
            tbl["ignore_characters"] = (Action<DynValue>)IgnoreCharacters;
        }

        script.Globals[varName] = tbl;
    }

    private void MoveUp (DynValue arg) {
        int steps = arg.Type == DataType.Number ? (int)arg.Number : 1;

        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new(MoveKind.StepUp, _ignoreCharacters), steps
        ));
    }

    private void MoveDown (DynValue arg) {
        int steps = arg.Type == DataType.Number ? (int)arg.Number : 1;

        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new(MoveKind.StepDown, _ignoreCharacters), steps
        ));
    }

    private void MoveLeft (DynValue arg) {
        int steps = arg.Type == DataType.Number ? (int)arg.Number : 1;

        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new(MoveKind.StepLeft, _ignoreCharacters), steps
        ));
    }

    private void MoveRight (DynValue arg) {
        int steps = arg.Type == DataType.Number ? (int)arg.Number : 1;

        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new(MoveKind.StepRight, _ignoreCharacters), steps
        ));
    }

    private void LookUp () {
        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new(MoveKind.LookUp, _ignoreCharacters), 1
        ));
    }

    private void LookDown () {
        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new(MoveKind.LookDown, _ignoreCharacters), 1
        ));
    }

    private void LookLeft () {
        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new(MoveKind.LookLeft, _ignoreCharacters), 1
        ));
    }

    private void LookRight () {
        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new(MoveKind.LookRight, _ignoreCharacters), 1
        ));
    }

    private void LookTowardsPlayer () {
        var delta = G.World.Player.Position - _entity.Position;

        Direction dir;
        if (Math.Abs(delta.Y) > Math.Abs(delta.X)) {
            dir = delta.Y > 0 ? Direction.Down : Direction.Up;
        }
        else {
            dir = delta.X > 0 ? Direction.Right : Direction.Left;
        }

        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new((MoveKind)(dir + 4), _ignoreCharacters), 1
        ));
    }

    private void Jump (DynValue arg) {
        int times = arg.Type == DataType.Number ? (int)arg.Number : 1;

        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new(MoveKind.Jump, _ignoreCharacters), times
        ));
    }

    private void IgnoreCharacters (DynValue argVal) {
        bool val = argVal.Type == DataType.Boolean ? argVal.Boolean : true;
        _ignoreCharacters = val;
    }
}
