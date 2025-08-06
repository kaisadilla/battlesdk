using NLog;
using TiledCS;

namespace battlesdk.data;
public class TileProperties {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public const string PROP_IMPASSABLE = "Impassable";
    public const string PROP_IMPASSABLE_DIRECTIONS = "ImpassableDirections";

    public int Impassable { get; init; } = 0;
    public bool Jump { get; init; } = false;
    public Direction JumpDirection { get; init; } = Direction.None;

    public bool ImpassableDown => (Impassable & (int)DirectionMask.Down) != 0;
    public bool ImpassableRight => (Impassable & (int)DirectionMask.Right) != 0;
    public bool ImpassableUp => (Impassable & (int)DirectionMask.Up) != 0;
    public bool ImpassableLeft => (Impassable & (int)DirectionMask.Left) != 0;

    public bool ImpassableAt (Direction direction) {
        return (Impassable & (int)direction.ToMask()) != 0;
    }

    public TileProperties () { }

    public TileProperties (TiledTile? tiledTile) {
        if (tiledTile is null) return;

        int impassable = 0;

        foreach (var prop in tiledTile.properties) {
            if (prop.name == "ImpassableDirections") {
                if (int.TryParse(prop.value, out impassable) == false) {
                    _logger.Warn("Tile property 'ImpassableDirections' has an invalid value.");
                }
            }
            else if (prop.name == "Impassable") {
                if (bool.TryParse(prop.value, out var isImpassable) == false) {
                    _logger.Warn("Tile property 'Impassable' has an invalid value.");
                }

                impassable = isImpassable ? (int)(DirectionMask.Down
                    | DirectionMask.Right
                    | DirectionMask.Up
                    | DirectionMask.Left
                ) : 0;
            }
            else if (prop.name == "Jump") {
                if (bool.TryParse(prop.value, out var jump) == false) {
                    _logger.Warn("Tile property 'Jump' has an invalid value.");
                }

                Jump = jump;
            }
            else if (prop.name == "JumpDirection") {
                if (int.TryParse(prop.value, out var jumpDir) == false) {
                    _logger.Warn("Tile property 'JumpDirection' has an invalid value.");
                }
                else if (jumpDir < 0 || jumpDir > (int)Direction.None) {
                    _logger.Warn("Tile property 'JumpDirection' has an invalid value.");
                }
                else {
                    JumpDirection = (Direction)jumpDir;
                }
            }
        }

        Impassable = impassable;
    }
}
