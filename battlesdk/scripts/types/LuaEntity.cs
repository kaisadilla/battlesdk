using battlesdk.scripts.types;
using battlesdk.world;
using battlesdk.world.entities;
using MoonSharp.Interpreter;

namespace battlesdk.scripts;

[LuaApiClass]
public class LuaEntity {
    [MoonSharpHidden]
    public const string CLASSNAME = "Entity";

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

    public void move_up (int? times) {
        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new(MoveKind.StepUp, _ignoreCharacters), times ?? 1
        ));
    }

    public void move_down (int? times) {
        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new(MoveKind.StepDown, _ignoreCharacters), times ?? 1
        ));
    }

    public void move_left (int? times) {
        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new(MoveKind.StepLeft, _ignoreCharacters), times ?? 1
        ));
    }

    public void move_right (int? times) {
        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new(MoveKind.StepRight, _ignoreCharacters), times ?? 1
        ));
    }

    public void look_up () {
        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new(MoveKind.LookUp, _ignoreCharacters), 1
        ));
    }

    public void look_down () {
        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new(MoveKind.LookDown, _ignoreCharacters), 1
        ));
    }

    public void look_left () {
        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new(MoveKind.LookLeft, _ignoreCharacters), 1
        ));
    }

    public void look_right () {
        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new(MoveKind.LookRight, _ignoreCharacters), 1
        ));
    }

    public void look_towards_player () {
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

    public void jump (int? times) {
        ScriptLoop.Enqueue(new MoveScriptEvent(
            (Character)_entity, new(MoveKind.Jump, _ignoreCharacters), times ?? 1
        ));
    }

    public void ignore_characters (bool val) {
        _ignoreCharacters = val;
    }
}
