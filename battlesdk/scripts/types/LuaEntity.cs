using battlesdk.scripts.types;
using battlesdk.world.entities;
using MoonSharp.Interpreter;
using NLog;

namespace battlesdk.scripts;

[LuaApiClass]
public class LuaEntity {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

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

    private int GetTimes (CallbackArguments args) {
        if (args.Count > 0 && args[0].Type == DataType.Number) {
            return (int)args[0].Number;
        }

        return 1;
    }

    public DynValue move_up (ScriptExecutionContext ctx, CallbackArguments args) {
        return ExecuteMove(
            ctx, args, ch => ch.TryMove(Direction.Up, _ignoreCharacters)
        );
    }

    public DynValue move_down (ScriptExecutionContext ctx, CallbackArguments args) {
        return ExecuteMove(
            ctx, args, ch => ch.TryMove(Direction.Down, _ignoreCharacters)
        );
    }

    public DynValue move_left (ScriptExecutionContext ctx, CallbackArguments args) {
        return ExecuteMove(
            ctx, args, ch => ch.TryMove(Direction.Left, _ignoreCharacters)
        );
    }

    public DynValue move_right (ScriptExecutionContext ctx, CallbackArguments args) {
        return ExecuteMove(
            ctx, args, ch => ch.TryMove(Direction.Right, _ignoreCharacters)
        );
    }
    
    public void look_up () {
        _entity.SetDirection(Direction.Up);
    }
    
    public void look_down () {
        _entity.SetDirection(Direction.Down);
    }
    
    public void look_left () {
        _entity.SetDirection(Direction.Left);
    }
    
    public void look_right () {
        _entity.SetDirection(Direction.Right);
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

        _entity.SetDirection(dir);
    }

    public DynValue Jump (ScriptExecutionContext ctx, CallbackArguments args) {
        return ExecuteMove(
            ctx, args, ch => ch.JumpInPlace()
        );
    }

    public void ignore_characters (bool val) {
        _ignoreCharacters = val;
    }

    private DynValue ExecuteMove (
        ScriptExecutionContext ctx, CallbackArguments args, Action<Character> callback
    ) {
        if (_entity is not Character ch) {
            _logger.Error("[move_up] Entity is not a character.");
            return DynValue.Nil;
        }

        var luaCor = ctx.GetCallingCoroutine();
        int totalTimes = GetTimes(args);
        int times = 0;
        Coroutine.Start(_Cor());

        CoroutineTask _Cor () {
            while (times < totalTimes) {
                if (ch.IsMoving) {
                    yield return null;
                }
                else {
                    callback(ch);
                    times++;
                }
            }
            while (ch.IsMoving) {
                yield return null;
            }

            luaCor.Resume();
        }

        return DynValue.NewYieldReq([]);
    }
}
