using NLog;
using TiledCS;

namespace battlesdk.data;

public class Map : INameable {
    private const string Z_INDICES_LAYER_NAME = "ZIndices";

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private string _path;

    public string Name { get; private init; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public List<TileLayer> Layers { get; } = [];
    public ZWarpMap ZWarps { get; private set; } = new(0, 0);

    public Map (string name, string path) {
        Name = name;
        _path = path;

        Init();
    }

    protected void Init () {
        var map = new TiledMap(_path);

        Width = map.Width;
        Height = map.Height;
        ZWarps = new(Width, Height);

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

        foreach (var g in map.Groups) {
            if (g.name.StartsWith("Z=")) {
                if (int.TryParse(g.name.Substring(2), out int zIndex) == false) {
                    _logger.Error(
                        "Invalid z-index layer value. Layer name must be 'Z=' " +
                        "followed by an integer and nothing else."
                    );
                    continue;
                }

                foreach (var l in g.layers) {
                    var layer = _BuildLayer(l, zIndex);
                    if (layer is not null) Layers.Add(layer);
                }
            }
            else if (g.name == "Flags") {
                foreach (var l in g.layers) {
                    if (l.name == Z_INDICES_LAYER_NAME) {
                        _ReadZIndices(l);
                    }
                }
            }
            else {
                _logger.Warn($"Unrecognized map group label: '{g.name}'.");
            }
        }

        TileLayer? _BuildLayer (TiledLayer tiledLayer, int zIndex) {
            if (tiledLayer.type != TiledLayerType.TileLayer) return null;

            TileLayer layer = new(Width, Height, zIndex);

            for (int y = 0; y < map.Height; y++) {
                for (int x = 0; x < map.Width; x++) {
                    int index = y * map.Width + x;
                    int rawGid = tiledLayer.data[index];
                    int gid = rawGid & 0x1fffffff;

                    if (gid == 0) {
                        layer[x, y] = null;
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
                        $"Invalid tile at layer '{tiledLayer.name}'[{x}, {y}]."
                    );

                    int tileId = gid - firstGids[tsIndex];

                    var tileset = Registry.Tilesets[tilesetIds[tsIndex]];

                    if (tileset.Kind != TilesetKind.Normal) continue;

                    layer[x, y] = new() {
                        TilesetId = tilesetIds[tsIndex],
                        TileId = tileId,
                        Properties = tileset.Tiles[tileId],
                        ZIndex = 0,
                        Flags = [],
                    };
                }
            }

            return layer;
        }

        void _ReadZIndices (TiledLayer tiledLayer) {
            if (tiledLayer.type != TiledLayerType.TileLayer) return;

            for (int y = 0; y < map.Height; y++) {
                for (int x = 0; x < map.Width; x++) {
                    int index = y * map.Width + x;
                    int rawGid = tiledLayer.data[index];
                    int gid = rawGid & 0x1fffffff;

                    if (gid == 0) continue;

                    int tsIndex = -1;
                    for (int i = firstGids.Count - 1; i >= 0; i--) {
                        if (firstGids[i] <= gid) {
                            tsIndex = i;
                            break;
                        }
                    }

                    if (tsIndex == -1) throw new Exception(
                        $"Invalid tile at layer '{tiledLayer.name}'[{x}, {y}]."
                    );

                    int tileId = gid - firstGids[tsIndex];

                    var tileset = Registry.Tilesets[tilesetIds[tsIndex]];

                    if (tileset.Kind != TilesetKind.ZIndices) continue;

                    ZWarps.SetWarp(x, y, tileId);
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
