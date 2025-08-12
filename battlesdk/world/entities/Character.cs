using battlesdk.data;
using battlesdk.world.entities.interaction;

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

        if (Sprite is not null && Sprite is CharacterSpriteFile cs) {
            int frame = 0;
            if (IsMoving && MoveProgress >= 0.25f && MoveProgress < 0.75f) {
                frame = 1 + (MoveCount % 2);
            }

            if (IsRunning && IsMoving && IsJumping == false) {
                SpriteIndex = cs.GetRunningSprite(Direction, frame);
            }
            else {
                SpriteIndex = cs.GetWalkingSprite(Direction, frame);
            }
        }

        if (IsMoving) {
            if (IsJumping) {
                MoveProgress += Time.DeltaTime * Settings.LedgeJumpSpeed;
            }
            else if (IsJumpingInPlace) {
                MoveProgress += Time.DeltaTime / (13f / 60f);
            }
            else if (IsRunning) {
                MoveProgress += Time.DeltaTime * Settings.RunSpeed;
            }
            else {
                MoveProgress += Time.DeltaTime * Settings.WalkSpeed;
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

    public override void OnPrimaryAction (Direction from) {
        if (
            IsMoving
            && PreviousPosition != Position
            && MoveProgress < Constants.MOVE_INTERACTION_THRESHOLD
        ) return;

        base.OnPrimaryAction(from);
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
    /// moving, the call is ignored.
    /// Returns true if the move has been executed.
    /// </summary>
    /// <param name="direction">The direction in which to move.</param>
    /// <param name="ignoreCharacters">If true, the move will ignore characters.</param>
    public virtual bool TryMove (Direction direction, bool ignoreCharacters) {
        if (G.World is null) return false;
        if (IsMoving) return false;

        var destination = Position.OffsetAt(direction);

        if (TriggerMoveIntoInteractions(destination, direction.Opposite())) {
            return false;
        }

        bool moveAllowed = IsMoveAllowed(
            destination, direction, ignoreCharacters, out var jumpDir
        );

        SetDirection(direction);
        if (moveAllowed == false) destination = Position;

        Move(destination, jumpDir);

        if (moveAllowed == false) Collided = true;

        return moveAllowed;
    }

    /// <summary>
    /// Executes a move into the destination given. This move may be a jump.
    /// This method does not check the validity nor consequences of the move,
    /// it will execute the move exactly as described no matter what. For a
    /// natural, validated move use <see cref="TryMove(Direction, bool)"/> instead.
    /// </summary>
    /// <param name="destination">The tile to move to.</param>
    /// <param name="jump">The direction to jump towards. A value of
    /// <see cref="Direction.None"/> will result in no jump.</param>
    public void Move (IVec2 destination, Direction jump = Direction.None) {
        PreviousPosition = Position;
        MoveCount++;
        if (jump == Direction.None) {
            IsMoving = true;
            MoveProgress = 0;
            SetPosition(destination);
        }
        else {
            IsMoving = true;
            IsJumping = true;
            MoveProgress = 0;
            SetPosition(destination.OffsetAt(jump));
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

    /// <summary>
    /// If there's any interactable entity at the position given, triggers its
    /// interaction (if said interaction exists and is a
    /// <see cref="InteractionTrigger.PlayerTouchesEntity"/>. Returns true if
    /// an interaction was triggered.
    /// </summary>
    /// <param name="destination">The position to check for interactions.</param>
    /// <param name="from">The direction from which the player would interact.</param>
    /// <returns>True if an interaction happened.</returns>
    private bool TriggerMoveIntoInteractions (IVec2 destination, Direction from) {
        var entities = G.World.GetEntitiesAt(destination);
        foreach (var e in entities) {
            if (e.OnTryStepInto(this, from)) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns true if a move into the tile given, from the direction given,
    /// is allowed. Also outputs whether it would result in a jump.
    /// </summary>
    /// <param name="destination">The position to move to.</param>
    /// <param name="direction">The direction from which to move.</param>
    /// <param name="ignoreEntities">True if this move doesn't care about
    /// entities that may block the way.</param>
    /// <param name="jump">The jump that would be produced by trying to move
    /// into the destination tile. If no jump would occur, this value will be
    /// <see cref="Direction.None"/>.</param>
    /// <returns></returns>
    private bool IsMoveAllowed (
        IVec2 destination,
        Direction direction,
        bool ignoreEntities,
        out Direction jump
    ) {
        var originTiles = G.World.GetTilesAt(Position, Z);

        jump = Direction.None;
        foreach (var t in originTiles) {
            if (t.Properties.ImpassableAt(direction)) {
                return false;
            }
        }

        var dstTiles = G.World.GetTilesAt(destination, Z);
        if (dstTiles.Count == 0) return false;

        foreach (var t in dstTiles) {
            if (t.Properties.ImpassableAt(direction.Opposite())) {
                return false;
            }
            if (t.Properties.Jump && t.Properties.JumpDirection != Direction.None) {
                jump = t.Properties.JumpDirection;
            }
        }

        if (ignoreEntities == false) {
            var entities = G.World.GetEntitiesAt(destination);
            foreach (var e in entities) {
                return false;
            }
        }

        return true;
    }
}
