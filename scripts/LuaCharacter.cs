using battlesdk.data;
using battlesdk.world;
using battlesdk.world.entities;
using MoonSharp.Interpreter;

namespace battlesdk.scripts;

public class LuaCharacter {
    private readonly Character _character;

    private bool _ignoreCharacters = false;

    public LuaCharacter (Character character) {
        _character = character;
    }

    public void Register (Script script, ScriptAsset asset, string varName) {
        Table tbl = new(script);

        tbl["move_up"] = (Action<DynValue>)MoveUp;
        tbl["move_down"] = (Action<DynValue>)MoveDown;
        tbl["move_left"] = (Action<DynValue>)MoveLeft;
        tbl["move_right"] = (Action<DynValue>)MoveRight;
        tbl["look_up"] = (Action)LookUp;
        tbl["look_down"] = (Action)LookDown;
        tbl["look_left"] = (Action)LookLeft;
        tbl["look_right"] = (Action)LookRight;
        tbl["look_towards_player"] = (Action)LookTowardsPlayer;
        tbl["jump"] = (Action<DynValue>)Jump;
        tbl["ignore_characters"] = (Action<DynValue>)IgnoreCharacters;

        script.Globals[varName] = tbl;
    }

    private void MoveUp (DynValue arg) {
        int steps = arg.Type == DataType.Number ? (int)arg.Number : 1;

        ScriptLoop.Enqueue(new MoveScriptEvent(
            _character, new(MoveKind.StepUp, _ignoreCharacters), steps
        ));
    }

    private void MoveDown (DynValue arg) {
        int steps = arg.Type == DataType.Number ? (int)arg.Number : 1;

        ScriptLoop.Enqueue(new MoveScriptEvent(
            _character, new(MoveKind.StepDown, _ignoreCharacters), steps
        ));
    }

    private void MoveLeft (DynValue arg) {
        int steps = arg.Type == DataType.Number ? (int)arg.Number : 1;

        ScriptLoop.Enqueue(new MoveScriptEvent(
            _character, new(MoveKind.StepLeft, _ignoreCharacters), steps
        ));
    }

    private void MoveRight (DynValue arg) {
        int steps = arg.Type == DataType.Number ? (int)arg.Number : 1;

        ScriptLoop.Enqueue(new MoveScriptEvent(
            _character, new(MoveKind.StepRight, _ignoreCharacters), steps
        ));
    }

    private void LookUp () {
        ScriptLoop.Enqueue(new MoveScriptEvent(
            _character, new(MoveKind.LookUp, _ignoreCharacters), 1
        ));
    }

    private void LookDown () {
        ScriptLoop.Enqueue(new MoveScriptEvent(
            _character, new(MoveKind.LookDown, _ignoreCharacters), 1
        ));
    }

    private void LookLeft () {
        ScriptLoop.Enqueue(new MoveScriptEvent(
            _character, new(MoveKind.LookLeft, _ignoreCharacters), 1
        ));
    }

    private void LookRight () {
        ScriptLoop.Enqueue(new MoveScriptEvent(
            _character, new(MoveKind.LookRight, _ignoreCharacters), 1
        ));
    }

    private void LookTowardsPlayer () {
        var delta = G.World.Player.Position - _character.Position;

        Direction dir;
        if (Math.Abs(delta.X) > Math.Abs(delta.Y)) {
            dir = delta.X > 0 ? Direction.Right : Direction.Left;
        }
        else {
            dir = delta.Y > 0 ? Direction.Down : Direction.Up;
        }

        ScriptLoop.Enqueue(new MoveScriptEvent(
            _character, new((MoveKind)(dir + 4), _ignoreCharacters), 1
        ));
    }

    private void Jump (DynValue arg) {
        int times = arg.Type == DataType.Number ? (int)arg.Number : 1;

        ScriptLoop.Enqueue(new MoveScriptEvent(
            _character, new(MoveKind.Jump, _ignoreCharacters), times
        ));
    }

    private void IgnoreCharacters (DynValue argVal) {
        bool val = argVal.Type == DataType.Boolean ? argVal.Boolean : true;
        _ignoreCharacters = val;
    }
}
