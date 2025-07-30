using NLog;
using TiledCS;

namespace battlesdk.data;

public class Map : INameable {
    private const string METADATA_GROUP_NAME = "Metadata";
    private const string Z_WARPS_LAYER_NAME = "ZWarps";

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

        // A tiled map contains a list with all the tilesets used in it. However,
        // these tilesets are identified by the path to their file, which means
        // that we have to map each tileset in the map to a tileset in BattleSDK's
        // registry.

        // A list that maps each tileset in the map to the index of said tileset
        // in the registry.
        List<int> tilesetIds = [];
        // A list that contains the first gid that belongs to each tileset in
        // the map.
        List<int> firstGids = [];

        foreach (var ts in map.Tilesets) {
            var name = Registry.GetAssetName(Registry.FOLDER_TILESETS, ts.source);

            // We should be able to locate a tileset with the name given in
            // the registry. Otherwise, this map cannot be used by BattleSDK.
            if (Registry.Tilesets.Indices.TryGetValue(name, out var index) == false) {
                throw new($"Unknown tileset: '{name}' (path: '{ts.source}').");
            }

            tilesetIds.Add(index);
            firstGids.Add(ts.firstgid);
        }

        // BattleSDK expects every layer in the map to be placed inside a group.
        // As such, layers outside groups are ignored. Each group may contain
        // an arbitrary number of layers.
        foreach (var g in map.Groups) {
            // Groups that start with "Z=" contain the terrain. The text at the
            // right of the equals sign must be a number that can be parsed,
            // representing the z index of the layer.
            if (g.name.StartsWith("Z=")) {
                // If the Z index in the name is invalid, ignore this layer.
                if (int.TryParse(g.name.AsSpan(2), out int zIndex) == false) {
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
            // A group named "Metadata" contains the metadata for the tiles.
            else if (g.name == METADATA_GROUP_NAME) {
                // If the layer is named "ZWarps", the data contained are the
                // z warps present in the map.
                foreach (var l in g.layers) {
                    if (l.name == Z_WARPS_LAYER_NAME) {
                        _ReadZIndices(l);
                    }
                }
            }
            // Groups with different names don't serve any purpose. As such,
            // they are ignored.
            else {
                _logger.Warn($"Unrecognized map group label: '{g.name}'.");
            }
        }

        TileLayer? _BuildLayer (TiledLayer tiledLayer, int zIndex) {
            if (tiledLayer.type != TiledLayerType.TileLayer) return null;

            TileLayer layer = new(Width, Height, zIndex);

            for (int y = 0; y < map.Height; y++) {
                for (int x = 0; x < map.Width; x++) {
                    // Tiled assigns indices to each tile in a map horizontally.
                    int index = (y * map.Width) + x;
                    // The raw gid of a tile in Tiled encodes not only the tile
                    // itself, but also transformations applied to them.
                    int rawGid = tiledLayer.data[index];
                    // The actual gid is obtained by filtering out these
                    // transformations.
                    int gid = rawGid & 0x1fffffff;

                    // If the gid is 0, then this position is empty.
                    if (gid == 0) {
                        layer[x, y] = null;
                        continue;
                    }

                    // Get the registry index of the tileset this tile belongs to.
                    int tilesetIndex = _GetTsIndex(tiledLayer.name, x, y, gid);
                    var tileset = Registry.Tilesets[tilesetIds[tilesetIndex]];
                    // The id of the tile inside the tileset.
                    int tileId = gid - firstGids[tilesetIndex];

                    // Only normal tilesets can be used to define terrain.
                    if (tileset.Kind != TilesetKind.Normal) continue;

                    layer[x, y] = new() {
                        TilesetId = tilesetIds[tilesetIndex],
                        TileId = tileId,
                        Properties = tileset.Tiles[tileId],
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

                    int tilesetIndex = _GetTsIndex(tiledLayer.name, x, y, gid);
                    var tileset = Registry.Tilesets[tilesetIds[tilesetIndex]];
                    int tileId = gid - firstGids[tilesetIndex];

                    // Only z-index tilesets
                    if (tileset.Kind != TilesetKind.ZWarps) continue;

                    // BattleSDK supports an arbitrary amount of z warps, the
                    // textures in the tileset are purely informative. As such,
                    // the index of a tile in the tileset represents the z index
                    // of the warp.
                    ZWarps.SetWarp(x, y, tileId);
                }
            }
        }

        // Returns the index of the tileset this tile belongs to.
        int _GetTsIndex (string layerName, int x, int y, int gid) {
            for (int i = firstGids.Count - 1; i >= 0; i--) {
                if (firstGids[i] <= gid) {
                    return i;
                }
            }

            throw new Exception(
                $"Invalid tile at layer '{layerName}'[{x}, {y}]."
            );
        }
    }

    /// <summary>
    /// Returns true if the position given is within the bounds of this map.
    /// </summary>
    /// <param name="pos">The position to check.</param>
    public bool IsWithinBounds (IVec2 pos) {
        return pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;
    }
}
