using battlesdk.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battlesdk;

public static class Registry {
    private static List<Tileset> _tilesets = [];
    private static Dictionary<string, int> _tilesetIndices = [];

    private static List<Map> _maps = [];
    private static Dictionary<string, int> _mapIndices = new Dictionary<string, int>();

    public static IReadOnlyList<Tileset> Tilesets => _tilesets;
    public static IReadOnlyDictionary<string, int> TilesetIndices => _tilesetIndices;
    public static IReadOnlyList<Map> Maps => _maps;
    public static IReadOnlyDictionary<string, int> MapIndices => _mapIndices;

    public static void RegisterTileset (Tileset ts) {
        _tilesetIndices[ts.Name] = _tilesets.Count;
        _tilesets.Add(ts);
    }

    public static void RegisterMap (Map map) {
        _mapIndices[map.Name] = _maps.Count;
        _maps.Add(map);
    }
}
