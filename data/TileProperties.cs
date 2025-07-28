using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledCS;

namespace battlesdk.data;
public class TileProperties {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public const string PROP_IMPASSABLE = "Impassable";
    public const string PROP_IMPASSABLE_DIRECTIONS = "ImpassableDirections";

    public int Impassable { get; init; } = 0;

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
            if (prop.name == "Impassable") {
                impassable = (int)(DirectionMask.Down
                    | DirectionMask.Right
                    | DirectionMask.Up
                    | DirectionMask.Left
                );
            }
        }

        Impassable = impassable;
    }
}
