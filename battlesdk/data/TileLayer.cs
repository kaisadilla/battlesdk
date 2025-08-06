namespace battlesdk.data;

public class TileLayer {
    private MapTile?[,] _tiles;

    public int Width { get; private init; }
    public int Height { get; private init; }
    public int ZIndex { get; private init; }

    public TileLayer (int width, int height, int zIndex) {
        _tiles = new MapTile[width, height];
        Width = width;
        Height = height;
        ZIndex = zIndex;
    }

    public MapTile? this[int x, int y] {
        get => _tiles[x, y];
        set => _tiles[x, y] = value;
    }

    public MapTile? this[IVec2 pos] {
        get => this[pos.X, pos.Y];
        set => this[pos.X, pos.Y] = value;
    }

    /// <summary>
    /// Returns an exact (deep) copy of this layer.
    /// </summary>
    public TileLayer Clone () {
        TileLayer clone = new(Width, Height, ZIndex);

        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                clone[x, y] = this[x, y];
            }
        }

        return clone;
    }
}
