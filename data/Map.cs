using NLog;
using TiledCS;

namespace battlesdk.data;

public class Map : INameable {
    private const string ABOVE_PLAYER_LAYER_NAME = "AbovePlayer";
    private const string BELOW_PLAYER_LAYER_NAME = "BelowPlayer";

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private string _path;

    public string Name { get; private init; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public List<TileLayer> LayersBelowPlayer { get; } = [];
    public List<TileLayer> LayersAbovePlayer { get; } = [];

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

        foreach (var g in map.Groups) {
            if (g.name == ABOVE_PLAYER_LAYER_NAME) {
                foreach (var l in g.layers) {
                    var layer = _BuildLayer(l);
                    if (layer is not null) LayersAbovePlayer.Add(layer);
                }
            }
            else if (g.name == BELOW_PLAYER_LAYER_NAME) {
                foreach (var l in g.layers) {
                    var layer = _BuildLayer(l);
                    if (layer is not null) LayersBelowPlayer.Add(layer);
                }
            }
            else {
                _logger.Warn($"Unrecognized map group label: '{g.name}'.");
            }
        }

        TileLayer? _BuildLayer (TiledLayer tiledLayer) {
            if (tiledLayer.type != TiledLayerType.TileLayer) return null;

            TileLayer layer = new(Width, Height);

            for (int y = 0; y < map.Height; y++) {
                for (int x = 0; x < map.Width; x++) {
                    int index = y * map.Width + x;
                    int rawGid = tiledLayer.data[index];
                    int gid = rawGid & 0x1fffffff;

                    if (gid == 0) {
                        layer[x, y] = MapTile.Empty;
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

                    layer[x, y] = new(
                        tsIndex,
                        tileId,
                        Registry.Tilesets.Elements[tilesetIds[tsIndex]].Tiles[tileId]
                    );
                }
            }

            return layer;
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
