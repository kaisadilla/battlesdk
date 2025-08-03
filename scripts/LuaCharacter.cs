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
