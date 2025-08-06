using battlesdk.data;

namespace battlesdk.world.entities;

public abstract class Character : Entity {
    public CharacterMovement? AutonomousMovement { get; private set; } = null;

    /// <summary>
    /// True while the character is moving from one tile into another.
    /// </summary>
    public bool IsMoving { get; private set; } = false;
    /// <summary>
    /// True while the character is running.
    /// </summary>
    public bool IsRunning { get; protected set; } = false;
    /// <summary>
    /// True if the character is moving by jumping.
    /// </summary>
    public bool IsJumping { get; protected set; } = false;
    /// <summary>
    /// True if the player is jumping in place.
    /// </summary>
    public bool IsJumpingInPlace { get; protected set; } = false;
    /// <summary>
    /// A value from 0 to 1 representing the progress of the movement being made.
    /// </summary>
    public float MoveProgress { get; private set; } = 0f;
    /// <summary>
    /// The amount of times this character has moved.
    /// </summary>
    public int MoveCount { get; protected set; } = 0;

    /// <summary>
    /// True if this character has tried to move against impassable terrain
    /// this frame.
    /// </summary>
    public bool Collided { get; private set; } = false;

    /// <summary>
    /// The position the character was in before its last movement.
    /// </summary>
    public IVec2 PreviousPosition { get; private set; } = IVec2.Zero;

    public override Vec2 Subposition {
        get {
            if (IsMoving == false) return Position;

            return new Vec2(
                PreviousPosition.X.Lerp(Position.X, MoveProgress),
                PreviousPosition.Y.Lerp(Position.Y, MoveProgress)
            );
        }
    }

    public Character (int mapId, int entityId, IVec2 worldPos, string sprite)
        : base(mapId, entityId, worldPos, sprite)
    {

    }

    public Character (int mapId, int entityId, GameMap map, CharacterData data)
        : base(mapId, entityId, map, data)
    {
        if (data.Movement is not null) {
            AutonomousMovement = CharacterMovement.New(this, data.Movement);
        }
    }

    public override void FrameStart () {
        base.FrameStart();
        Collided = false;
    }

    public override void Update () {
        base.Update();

        if (IsMoving) {
            if (IsJumping) {
                MoveProgress += Time.DeltaTime / Constants.LEDGE_JUMP_SPEED;
            }
            else if (IsJumpingInPlace) {
                MoveProgress += Time.DeltaTime / (13f / 60f);
            }
            else if (IsRunning) {
                MoveProgress += Time.DeltaTime / Constants.RUN_SPEED;
            }
            else {
                MoveProgress += Time.DeltaTime / Constants.WALK_SPEED;
            }
        }

        if (MoveProgress >= 1f) {
            MoveProgress = 0;
            IsMoving = false;
            IsJumping = false;
            IsJumpingInPlace = false;
        }

        if (_interaction?.IsInteracting != true) {
            AutonomousMovement?.Update();
        }
    }

    public override void Interact (Direction from) {
        if (IsMoving && MoveProgress < 0.8f) return;

        base.Interact(from);
    }

    /// <summary>
    /// Sets this character's autonomous movement.
    /// </summary>
    /// <param name="movement">A movement provider, or null to have no movement.</param>
    public virtual void SetAutonomousMovement (CharacterMovement? movement) {
        AutonomousMovement = movement;
    }

    /// <summary>
    /// The character executes the move described. If the character is already
    /// moving, the call is ignored. To queue up moves, use
    /// <see cref="QueueMove(CharacterMove)"/> instead.
    /// </summary>
    /// <param name="direction">The direction in which to move.</param>
    /// <param name="ignoreCharacters">If true, the move will ignore characters.</param>
    public virtual void Move (Direction direction, bool ignoreCharacters) {
        if (G.World is null) return;
        if (IsMoving) return;

        var destination = direction switch {
            Direction.Down => Position + new IVec2(0, 1),
            Direction.Right => Position + new IVec2(1, 0),
            Direction.Up => Position + new IVec2(0, -1),
            Direction.Left => Position + new IVec2(-1, 0),
            _ => Position,
        };

        var originTiles = G.World.GetTilesAt(Position, Z);

        bool moveAllowed = true;
        Direction jumpDir = Direction.None;
        foreach (var t in originTiles) {
            if (t.ImpassableAt(direction)) {
                moveAllowed = false;
                break;
            }
        }
        if (moveAllowed) {
            var dstTiles = G.World.GetTilesAt(destination, Z);
            if (dstTiles.Count == 0) moveAllowed = false;

            foreach (var t in dstTiles) {
                if (t.ImpassableAt(direction.Opposite())) {
                    moveAllowed = false;
                    break;
                }
                if (t.Jump && t.JumpDirection != Direction.None) {
                    jumpDir = t.JumpDirection;
                }
            }
        }
        if (moveAllowed && ignoreCharacters == false) {
            var ch = G.World.GetCharacterAt(destination);
            if (ch is not null) moveAllowed = false;
        }

        SetDirection(direction);
        if (moveAllowed == false) {
            destination = Position;
        }

        PreviousPosition = Position;
        MoveCount++;
        if (jumpDir == Direction.None) {
            IsMoving = true;
            MoveProgress = 0; //Constants.WALK_SPEED* Time.DeltaTime;
            SetPosition(destination);
        }
        else {
            IsMoving = true;
            IsJumping = true;
            MoveProgress = 0;
            SetPosition(jumpDir switch {
                Direction.Down => destination + new IVec2(0, 1),
                Direction.Right => destination + new IVec2(1, 0),
                Direction.Up => destination + new IVec2(0, -1),
                Direction.Left => destination + new IVec2(-1, 0),
                _ => destination,
            });
        }

        if (moveAllowed == false) {
            Collided = true;
        }
    }

    /// <summary>
    /// Makes the character jump without moving to any tile.
    /// </summary>
    public virtual void JumpInPlace () {
        IsMoving = true;
        IsJumpingInPlace = true;
        MoveProgress = 0;
        PreviousPosition = Position;
        Audio.PlayJump();
    }

    public virtual void SetRunning (bool value) {
        IsRunning = value;
    }
}
