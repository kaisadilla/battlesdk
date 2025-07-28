using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battlesdk.data;

public readonly record struct MapTile (
    int TilesetId, int TileId, TileProperties Properties
) {
    public static MapTile Empty = new(-1, -1, new());
}

public class TileLayer {
    private MapTile[,] _tiles;
    private int _width;
    private int _height;

    public TileLayer (int width, int height) {
        _tiles = new MapTile[width, height];
        _width = width;
        _height = height;
    }

    public MapTile this[int x, int y] {
        get => _tiles[x, y];
        set => _tiles[x, y] = value;
    }

    public MapTile this[IVec2 pos] {
        get => this[pos.X, pos.Y];
        set => this[pos.X, pos.Y] = value;
    }

    /// <summary>
    /// Returns an exact (deep) copy of this layer.
    /// </summary>
    public TileLayer Clone () {
        TileLayer clone = new(_width, _height);

        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _height; y++) {
                clone[x, y] = this[x, y];
            }
        }

        return clone;
    }
}