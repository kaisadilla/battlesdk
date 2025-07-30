namespace battlesdk.world;

public abstract class Character {
    public IVec2 Position { get; private set; }
    public Direction Direction { get; private set; } = Direction.Down;

    public int Sprite { get; private set; }

    /// <summary>
    /// The z position of the character, which indicates the height at which it
    /// currently is.
    /// </summary>
    public int Z { get; private set; } = 0;
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
    /// A value from 0 to 1 representing the progress of the movement being made.
    /// </summary>
    public float MoveProgress { get; private set; } = 0f;

    /// <summary>
    /// True if this character has tried to move against impassable terrain
    /// this frame.
    /// </summary>
    public bool Collided { get; private set; } = false;

    public Vec2 Subposition {
        get {
            if (IsMoving == false) return Position;

            int offset = IsJumping ? 2 : 1;

            return (Vec2)Position + (Direction switch {
                Direction.Down => new(0, -offset),
                Direction.Right => new(-offset, 0),
                Direction.Up => new(0, offset),
                Direction.Left => new(offset, 0),
                _ => Vec2.Zero,
            } * (1f - MoveProgress));
        }
    }

    public Character (IVec2 position, string sprite) {
        Position = position;
        Sprite = Registry.CharSprites.Indices[sprite];
    }

    public virtual void OnFrameStart () {
        Collided = false;
    }

    public virtual void Update () {
        if (IsMoving) {
            if (IsJumping) {
                MoveProgress += Time.DeltaTime / Constants.LEDGE_JUMP_SPEED;
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
        }
    }

    public virtual void SetDirection (Direction direction) {
        Direction = direction;
    }

    public virtual void Move (Direction direction) {
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
        var dstTiles = G.World.GetTilesAt(destination, Z);

        bool moveAllowed = dstTiles.Count > 0;
        Direction jumpDir = Direction.None;
        if (moveAllowed) {
            foreach (var t in originTiles) {
                if (t.ImpassableAt(direction)) {
                    moveAllowed = false;
                    break;
                }
            }
        }
        if (moveAllowed) {
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

        Direction = direction;
        if (moveAllowed) {
            if (jumpDir == Direction.None) {
                IsMoving = true;
                MoveProgress = 0; //Constants.WALK_SPEED* Time.DeltaTime;
                Position = destination;
            }
            else {
                IsMoving = true;
                IsJumping = true;
                MoveProgress = 0;
                Position = jumpDir switch {
                    Direction.Down => destination + new IVec2(0, 1),
                    Direction.Right => destination + new IVec2(1, 0),
                    Direction.Up => destination + new IVec2(0, -1),
                    Direction.Left => destination + new IVec2(-1, 0),
                    _ => destination,
                };
            }
            LandAtTile();
        }
        else {
            Collided = true;
        }
    }

    protected void LandAtTile () {
        if (G.World is null) return;

        var zWarp = G.World.GetZWarpAt(Position);
        if (zWarp is null) return;

        Z = zWarp.Value;
    }
}
