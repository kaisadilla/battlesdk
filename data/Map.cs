using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledCS;

namespace battlesdk.data;

public class Map : INameable {
    private string _path;

    public string Name { get; private init; }
    public int Width { get; private set; } 
    public int Height { get; private set; }
    public List<TileLayer> Layers { get; } = [];

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
            
            if (Registry.Tilesets.Indices.TryGetValue(name, out var index) == false) {
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
                        Layers[^1][x, y] = MapTile.Empty;
                        continue;
                    }

                    int tsIndex = -1;
                    for (int i = firstGids.Count - 1; i >= 0; i--) {
                        if (firstGids[i] <= gid) {
                            tsIndex = i;
                            break;
                        }
                    }

                    if (tsIndex == -1) throw new Exception(
                        $"Invalid tile at layer '{layer.name}'[{x}, {y}]."
                    );

                    int tileId = gid - firstGids[tsIndex];

                    Layers[^1][x, y] = new(
                        tsIndex,
                        tileId,
                        Registry.Tilesets.Elements[tilesetIds[tsIndex]].Tiles[tileId]
                    );
                }
            }
        }
    }

    /// <summary>
    /// Returns true if the position given is within the bounds of this map.
    /// </summary>
    /// <param name="pos">The position to check.</param>
    public bool IsWithinBounds (IVec2 pos) {
        return pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;
    }

    private string GetTilesetName (string path) {
        string basePath = Path.GetFullPath("res/tilesets");
        string targetPath = Path.GetFullPath(Path.Combine("res/maps", path));

        return Path.GetRelativePath(basePath, targetPath);
    }
}
