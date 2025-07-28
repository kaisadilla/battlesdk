using battlesdk.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battlesdk.world;

public class GameMap {
    public Map Map { get; private init; }

    public List<TileLayer> Layers { get; } = [];

    public GameMap (Map map) {
        Map = map;

        foreach (var layer in map.Layers) {
            Layers.Add(layer.Clone());
        }
    }
}
