using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledCS;

namespace battlesdk.data;

public readonly record struct Tile (int TilesetId, int TileId) {
    public static Tile Empty = new(-1, -1);
}

public class Map {
    private string _path;

    public string Name { get; private init; }
    public int Width { get; private set; } 
    public int Height { get; private set; }
    public List<MapLayer> Layers { get; private init; } = [];

    public Map (string name, string path) {
        Name = name;
        _path = path;

        Init();
    }

    protected void Init () {
        var map = new TiledMap(_path);

        Width = map.Width;
        Height = map.Height;

        List<int> tilesetIds = [];
        List<int> firstGids = [];
        foreach (var ts in map.Tilesets) {
            var name = GetTilesetName(ts.source);
            
            if (Registry.TilesetIndices.TryGetValue(name, out var index) == false) {
                throw new($"Unknown tileset: '{name}' (path: '{ts.source}').");
            }

            tilesetIds.Add(index);
            firstGids.Add(ts.firstgid);
        }

        foreach (var layer in map.Layers) {
            if (layer.type != TiledLayerType.TileLayer) continue;

            Layers.Add(new(Width, Height));

            for (int y = 0; y < map.Height; y++) {
                for (int x = 0; x < map.Width; x++) {
                    int index = y * map.Width + x;
                    int rawGid = layer.data[index];
                    int gid = rawGid & 0x1fffffff;

                    if (gid == 0) {
                        Layers[^1][x, y] = Tile.Empty;
                        continue;
                    }

                    for (int i = 0; i < firstGids.Count; i++) {
                        if (firstGids[i] <= gid) {
                            Layers[^1][x, y] = new(i, gid - firstGids[i]);
                        }
                    }
                }
            }
        }
    }

    private string GetTilesetName (string path) {
        string basePath = Path.GetFullPath("res/tilesets");
        string targetPath = Path.GetFullPath(Path.Combine("res/maps", path));

        return Path.GetRelativePath(basePath, targetPath);
    }
}

public class MapLayer {
    private Tile[,] _tiles;

    public MapLayer (int width, int height) {
        _tiles = new Tile[width, height];
    }

    public Tile this[int x, int y] {
        get => _tiles[x, y];
        set => _tiles[x, y] = value;
    }
}