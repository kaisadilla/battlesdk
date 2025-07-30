using battlesdk.data;

namespace battlesdk.world;

public class GameMap {
    public MapData Data { get; }

    public int Width { get; }
    public int Height { get; }

    /// <summary>
    /// The position of this map in the world.
    /// </summary>
    public IRect WorldPos { get; }

    public List<TileLayer> Terrain { get; } = [];
    public ZWarpMap ZWarps { get; }

    public GameMap (MapData map, int x, int y) {
        Data = map;

        Width = map.Width;
        Height = map.Height;

        WorldPos = new(y, x, y + map.Height, x + map.Width);

        foreach (var layer in map.Terrain) {
            Terrain.Add(layer.Clone());
        }

        ZWarps = map.ZWarps.Clone();
    }

    /// <summary>
    /// Given a world position, returns its local position relative to this map.
    /// </summary>
    /// <param name="worldPos">A position in the world.</param>
    public IVec2 GetLocalPos (IVec2 worldPos) {
        return worldPos - new IVec2(WorldPos.Left, WorldPos.Top);
    }

    /// <summary>
    /// Returns true if the world position given is inside this map.
    /// </summary>
    /// <param name="worldPos">The position in the world this map belongs to.</param>
    public bool IsInsideBounds (IVec2 worldPos) {
        return worldPos.X >= WorldPos.Left
            && worldPos.X < WorldPos.Right
            && worldPos.Y >= WorldPos.Top
            && worldPos.Y < WorldPos.Bottom;
    }
}
