using battlesdk.world;
using battlesdk.world.entities;

namespace battlesdk.scripts;
public static class ScriptLoop {
    private static readonly Queue<ScriptEvent> _events = [];
    private static ScriptEvent? _current = null;

    public static void Update () {
        if (_current is null) {
            _events.TryDequeue(out _current);
        }

        UpdateCurrentEvent();
    }

    public static void Enqueue (ScriptEvent evt) {
        _events.Enqueue(evt);
    }

    public static void EnqueueScriptEnd (Action callback) {
        _events.Enqueue(new ScriptEndScriptEvent(callback));
    }

    private static void UpdateCurrentEvent () {
        if (_current is null) return;

        _current.Update();

        if (_current.Complete) {
            _events.TryDequeue(out _current);
            UpdateCurrentEvent();
        }
    }
}

public abstract class ScriptEvent {
    public bool Complete { get; protected set; } = false;

    public virtual void Update () { }
}

/// <summary>
/// Represents the end of a user script.
/// </summary>
public class ScriptEndScriptEvent : ScriptEvent {
    private Action _callback;

    public ScriptEndScriptEvent (Action callback) {
        _callback = callback;
    }

    public override void Update () {
        _callback();
        Complete = true;
    }
}

public class MoveScriptEvent : ScriptEvent {
    private Character _character;
    private CharacterMove _move;
    private int _amount;
    private int _count = 0;

    public MoveScriptEvent (Character character, CharacterMove move, int amount) {
        _character = character;
        _move = move;
        _amount = amount;
    }

    public override void Update () {
        // When we haven't applied all the moves yet:
        if (_count < _amount) {
            // If the character is moving, we wait.
            if (_character.IsMoving) return;

            // Else, we apply a new move.
            if (_move.Move <= MoveKind.StepLeft) {
                _character.Move((Direction)_move.Move, _move.IgnoreCharacters);
            }
            else if (_move.Move <= MoveKind.LookLeft) {
                _character.SetDirection((Direction)((int)_move.Move - 4));
            }
            else if (_move.Move == MoveKind.Jump) {
                _character.JumpInPlace();
            }

            _count++;
            return;
        }
        
        // If there's no more moves to apply, all we have to do is wait for it
        // to end.
        if (_character.IsMoving == false) {
            Complete = true;
        }
    }
}

public class MessageScriptEvent : ScriptEvent {
    private readonly string _text;
    private Task? _textboxTask = null;

    public MessageScriptEvent (string text) {
        _text = text;
    }

    public override void Update () {
        if (_textboxTask is null) {
            _textboxTask = Hud.ShowTextbox(_text);
        }

        if (_textboxTask?.IsCompleted == true) {
            Complete = true;
        }
    }
}