using NLog;
using TiledCS;

namespace battlesdk.data;
public class TileProperties {
    public const string PROP_IMPASSABLE = "Impassable";
    public const string PROP_IMPASSABLE_DIRECTIONS = "ImpassableDirections";


    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private TileAnimation? _anim { get; }

    /// <summary>
    /// The tileset this tile belongs to.
    /// </summary>
    public Tileset Tileset { get; }
    /// <summary>
    /// The index of this tile in the tileset.
    /// </summary>
    public int TileIndex { get; }

    public int Impassable { get; } = 0;
    public bool Jump { get; } = false;
    public Direction JumpDirection { get; } = Direction.None;

    /// <summary>
    /// The index of the tile that is drawn below the entity that stands in
    /// this tile. -1 if nothing is drawn.
    /// </summary>
    public int OnStepUnderTile { get; } = -1;
    /// <summary>
    /// The index of the tile that is drawn on top of an entity that stands
    /// in this tile. -1 if nothing is drawn.
    /// </summary>
    public int OnStepOverTile { get; } = -1;

    public bool ImpassableDown => (Impassable & (int)DirectionMask.Down) != 0;
    public bool ImpassableRight => (Impassable & (int)DirectionMask.Right) != 0;
    public bool ImpassableUp => (Impassable & (int)DirectionMask.Up) != 0;
    public bool ImpassableLeft => (Impassable & (int)DirectionMask.Left) != 0;

    public bool ImpassableAt (Direction direction) {
        return (Impassable & (int)direction.ToMask()) != 0;
    }

    public TileProperties (Tileset tileset, int tileIndex, TiledTile? tiledTile) {
        Tileset = tileset;
        TileIndex = tileIndex;

        if (tiledTile is null) return;

        int impassable = 0;
        bool loopAnim = true;

        foreach (var prop in tiledTile.properties) {
            if (prop.name == "ImpassableDirections") {
                if (int.TryParse(prop.value, out impassable) == false) {
                    _logger.Error("Tile property 'ImpassableDirections' has an invalid value.");
                }
            }
            else if (prop.name == "Impassable") {
                if (bool.TryParse(prop.value, out var isImpassable) == false) {
                    _logger.Error("Tile property 'Impassable' has an invalid value.");
                }

                impassable = isImpassable ? (int)(DirectionMask.Down
                    | DirectionMask.Right
                    | DirectionMask.Up
                    | DirectionMask.Left
                ) : 0;
            }
            else if (prop.name == "Jump") {
                if (bool.TryParse(prop.value, out var jump) == false) {
                    _logger.Error("Tile property 'Jump' has an invalid value.");
                }

                Jump = jump;
            }
            else if (prop.name == "JumpDirection") {
                if (int.TryParse(prop.value, out var jumpDir) == false) {
                    _logger.Error("Tile property 'JumpDirection' has an invalid value.");
                }
                else if (jumpDir < 0 || jumpDir > (int)Direction.None) {
                    _logger.Error("Tile property 'JumpDirection' has an invalid value.");
                }
                else {
                    JumpDirection = (Direction)jumpDir;
                }
            }
            else if (prop.name == "LoopAnim") {
                if (bool.TryParse(prop.value, out loopAnim) == false) {
                    _logger.Error("Tile property 'LoopAnim' has an invalid value.");
                }
            }
            else if (prop.name == "OnStepUnderTile") {
                if (int.TryParse(prop.value, out int tileId) == false) {
                    _logger.Error("Tile property 'OnStepUnderTile' has an invalid value.");
                }
                else {
                    OnStepUnderTile = tileId;
                }
            }
            else if (prop.name == "OnStepOverTile") {
                if (int.TryParse(prop.value, out int tileId) == false) {
                    _logger.Error("Tile property 'OnStepOverTile' has an invalid value.");
                }
                else {
                    OnStepOverTile = tileId;
                }
            }
        }

        Impassable = impassable;

        if (tiledTile.animation is not null && tiledTile.animation.Length > 0) {
            List<int> frameTileIds = [];
            List<int> frameDurations = [];
            int totalDuration = 0;
            foreach (var frame in tiledTile.animation) {
                frameTileIds.Add(frame.tileid);
                frameDurations.Add(frame.duration);
                totalDuration += frame.duration;
            }

            _anim = new() {
                FrameTileIds = frameTileIds,
                FrameDurations = frameDurations,
                TotalDuration = totalDuration,
                LoopAnim = loopAnim,
            };
        }
    }

    /// <summary>
    /// Returns the tileId drawn by this tile, according to its animation (or
    /// lack of).
    /// </summary>
    public int GetCurrentTileId () {
        return _anim == null ? TileIndex : _anim.GetCurrentFrame();
    }
}

public class TileAnimation {
    public required List<int> FrameTileIds { get; init; }
    public required List<int> FrameDurations { get; init; }
    public int TotalDuration { get; init; } // in milliseconds.
    /// <summary>
    /// Whether this tile's animation should be looped.
    /// </summary>
    public bool LoopAnim { get; init; } = true;

    public int GetCurrentFrame () {
        int time = (int)(Time.TotalTime * 1000);
        if (LoopAnim == false && time >= TotalDuration) return FrameTileIds[^1];

        time %= TotalDuration;
        int acc = 0;

        for (int i = 0; i < FrameTileIds.Count; i++) {
            acc += FrameDurations[i];
            if (time < acc) return FrameTileIds[i];
        }

        return FrameTileIds[0];
    }
}