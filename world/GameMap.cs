using battlesdk.data;

namespace battlesdk.world;

public class GameMap {
    public Map Map { get; private init; }

    public List<TileLayer> LayersAbovePlayer { get; } = [];
    public List<TileLayer> LayersBelowPlayer { get; } = [];

    public GameMap (Map map) {
        Map = map;

        foreach (var layer in map.LayersAbovePlayer) {
            LayersAbovePlayer.Add(layer.Clone());
        }

        foreach (var layer in map.LayersBelowPlayer) {
            LayersBelowPlayer.Add(layer.Clone());
        }
    }
}
