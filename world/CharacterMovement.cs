using battlesdk.data;
using battlesdk.world.entities;

namespace battlesdk.world;
public abstract class CharacterMovement {
    /// <summary>
    /// The character this autonomous movement controls.
    /// </summary>
    protected Character _character;

    /// <summary>
    /// If true, the character will not play its movement animation while moving.
    /// </summary>
    public bool MoveAnimation { get; init; } = true;
    /// <summary>
    /// If true, the character will update its direction to look towards the
    /// direction it is moving.
    /// </summary>
    public bool LookForward { get; init; } = true;
    /// <summary>
    /// If true, the character will ignore collision with other characters,
    /// becoming able to move through them.
    /// </summary>
    public bool IgnoreCharacters { get; init; } = true;

    protected CharacterMovement (Character character) {
        _character = character;
    }

    public static CharacterMovement New (
        Character character, CharacterMovementData data
    ) {
        return data switch {
            RouteCharacterMovementData r
                => new RouteCharacterMovement(character, r),
            RandomCharacterMovementData r
                => new RandomCharacterMovement(character, r),
            LookAroundCharacterMovementData r
                => new LookAroundCharacterMovement(character, r),
            _ => throw new NotImplementedException()
        };
    }

    public virtual void Update () { }
}

public class RouteCharacterMovement : CharacterMovement {
    /// <summary>
    /// A list of movements the character will make. This route will loop
    /// once it's finished.
    /// </summary>
    public List<MoveKind> Route { get; private init; }

    private int _cursor = 0;

    public RouteCharacterMovement (Character character, List<MoveKind> route) : base(character) {
        Route = route;
    }

    public RouteCharacterMovement (
        Character character, RouteCharacterMovementData data
    ) : base(character)
    {
        Route = [.. data.Route];
    }

    public override void Update () {
        if (_character.IsMoving) return;

        _character.Move((Direction)Route[_cursor], IgnoreCharacters);
        _cursor++;
        _cursor %= Route.Count;
    }
}

public class RandomCharacterMovement : CharacterMovement {
    private IVec2 _origin;

    /// <summary>
    /// The maximum amount of tiles, in each axis, that the character can
    /// deviate from its original position.
    /// </summary>
    public int MaxDistance { get; init; } = 4;
    /// <summary>
    /// The gap, in seconds, between each time the character moves.
    /// </summary>
    public float Gap { get; init; } = 3f;
    /// <summary>
    /// The variation, in seconds, of the gap between moves.
    /// </summary>
    public float Variation { get; init; } = 2f;

    private float _moveCd = float.MinValue;

    public RandomCharacterMovement (Character character) : base(character) {
        _origin = character.Position;
    }

    public RandomCharacterMovement (
        Character character, RandomCharacterMovementData data
    ) : base(character)
    {
        _origin = character.Position;
    }

    public override void Update () {
        if (_moveCd == float.MinValue) {
            _moveCd = GetCd();
        }

        if (_moveCd > 0f) {
            _moveCd -= Time.DeltaTime;
        }

        if (_moveCd <= 0f) {
            _moveCd = GetCd();

            var dir = (Direction)(Random.Shared.Next((int)Direction.None));

            if (
                (dir == Direction.Left && (_origin.X - _character.Position.X) > MaxDistance)
                || (dir == Direction.Right && (_character.Position.X - _origin.X) > MaxDistance)
                || (dir == Direction.Up && (_origin.Y - _character.Position.Y) > MaxDistance)
                || (dir == Direction.Down && (_character.Position.Y - _origin.Y) > MaxDistance)
            ) {
                dir = dir.Opposite();
            }

            if (Random.Shared.NextDouble() < 0.25) {
                _character.SetDirection(dir);
            }
            else {
                _character.Move(dir, IgnoreCharacters);
            }

        }
    }

    private float GetCd () {
        return Gap + (float)((Random.Shared.NextDouble() - 0.5) * 2 * Variation);
    }
}

public class LookAroundCharacterMovement : CharacterMovement {
    /// <summary>
    /// The gap, in seconds, between each time the character moves.
    /// </summary>
    public float Gap { get; init; } = 3f;
    /// <summary>
    /// The variation, in seconds, of the gap between moves.
    /// </summary>
    public float Variation { get; init; } = 2f;

    private float _moveCd = float.MinValue;

    public LookAroundCharacterMovement (Character character) : base(character) {}

    public LookAroundCharacterMovement (
        Character character, LookAroundCharacterMovementData data
    ) : base(character)
    {}

    public override void Update () {
        if (_moveCd == float.MinValue) {
            _moveCd = GetCd();
        }

        if (_moveCd > 0f) {
            _moveCd -= Time.DeltaTime;
        }

        if (_moveCd <= 0f) {
            _moveCd = GetCd();

            var dir = (Direction)(Random.Shared.Next((int)Direction.None));
            _character.SetDirection(dir);
        }
    }

    private float GetCd () {
        return Gap + (float)((Random.Shared.NextDouble() - 0.5) * 2 * Variation);
    }
}