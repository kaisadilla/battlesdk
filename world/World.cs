using battlesdk.data;

namespace battlesdk.world;

public class World {
    public List<GameMap> ActiveMaps { get; } = [];

    public Player Player { get; }

    public World (Map startingMap, IVec2 pos) {
        ActiveMaps.Add(new GameMap(startingMap));

        Player = new(pos);
    }

    public void OnFrameStart () {
        Player.OnFrameStart();
    }

    public void Update () {
        Player.Update();
    }

    public List<TileProperties> GetTilesAt (IVec2 pos) {
        if (ActiveMaps[0].Map.IsWithinBounds(pos) == false) return [];

        List<TileProperties> tiles = [];

        foreach (var l in ActiveMaps[0].LayersAbovePlayer) {
            tiles.Add(l[pos].Properties);
        }

        foreach (var l in ActiveMaps[0].LayersBelowPlayer) {
            tiles.Add(l[pos].Properties);
        }

        return tiles;
    }
}
