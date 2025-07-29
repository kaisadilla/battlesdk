using battlesdk.data;

namespace battlesdk.world;

public class GameMap {
    public Map Map { get; private init; }

    public List<TileLayer> Layers { get; } = [];
    public ZWarpMap ZWarps { get; }

    public GameMap (Map map) {
        Map = map;

        foreach (var layer in map.Layers) {
            Layers.Add(layer.Clone());
        }

        ZWarps = map.ZWarps.Clone();
    }
}
