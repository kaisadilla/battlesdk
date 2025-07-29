namespace battlesdk.data;
public class ZWarpMap {
    /// <summary>
    /// Maps every position in the map to a z warp. <see cref="int.MinValue"/>
    /// is used to indicate that the position does not have any z warp.
    /// </summary>
    private int[,] _map;

    public ZWarpMap (int width, int height) {
        _map = new int[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                _map[x, y] = int.MinValue;
            }
        }
    }

    /// <summary>
    /// Returns the z warp at the position given. Use <see cref="IsWarp(int, int)"/>
    /// to check if the position actually contains a z warp.
    /// </summary>
    public int this[int x, int y] => _map[x, y];

    /// <summary>
    /// Returns the z warp at the position given. Use <see cref="IsWarp(int, int)"/>
    /// to check if the position actually contains a z warp.
    /// </summary>
    public int this[IVec2 pos] => this[pos.X, pos.Y];

    /// <summary>
    /// Returns true if a z warp is present at the given position.
    /// </summary>
    public bool IsWarp (int x, int y) {
        return _map[x, y] != int.MinValue;
    }

    /// <summary>
    /// Sets the z warp at the position given to the value given. Use
    /// <see cref="int.MinValue"/> to remove z warps from this location.
    /// </summary>
    public void SetWarp (int x, int y, int zIndex) {
        _map[x, y] = zIndex;
    }

    /// <summary>
    /// Returns a deep clone of this z warp map.
    /// </summary>
    public ZWarpMap Clone () {
        ZWarpMap clone = new(_map.GetLength(0), _map.GetLength(1));

        for (int x = 0; x < _map.GetLength(0); x++) {
            for (int y = 0; y < _map.GetLength(1); y++) {
                clone._map[x, y] = _map[x, y];
            }
        }

        return clone;
    }
}
