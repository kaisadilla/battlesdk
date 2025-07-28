using battlesdk.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        foreach (var l in ActiveMaps[0].Layers) {
            tiles.Add(l[pos].Properties);
        }

        return tiles;
    }
}
