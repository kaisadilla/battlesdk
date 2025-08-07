using battlesdk.data.definitions;
using battlesdk.json;
using NLog;
using TiledCS;

namespace battlesdk.data;

/// <summary>
/// Contains the data used to build a given map.
/// </summary>
public class MapAsset : IIdentifiable {
    private const string GROUP_NAME_METADATA = "Metadata";
    private const string LAYER_NAME_Z_WARPS = "ZWarps";
    private const string PROP_NAME_BACKGROUND_MUSIC = "BackgroundMusic";

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public string Name { get; private init; }
    public int Id { get; private set; } = -1;
    public string Path { get; private init; }

    /// <summary>
    /// The width of this map, in tiles.
    /// </summary>
    public int Width { get; private set; }
    /// <summary>
    /// The height of this map, in tiles.
    /// </summary>
    public int Height { get; private set; }
    /// <summary>
    /// Each layer of tiles that make up this map's terrain.
    /// </summary>
    public List<TileLayer> Terrain { get; } = [];
    /// <summary>
    /// A map of the z warps present in this map.
    /// </summary>
    public ZWarpMap ZWarps { get; private set; } = new(0, 0);

    /// <summary>
    /// The id of this map's background music.
    /// </summary>
    public int BackgroundMusic { get; private set; } = -1;

    public List<NpcData> Npcs { get; } = [];

    public MapAsset (string name, string path) {
        Name = name;
        Path = path;

        ReadMapData();
        ReadEntityData();
    }

    public void SetId (int id) {
        Id = id;
    }

    protected void ReadMapData () {
        var map = new TiledMap(Path);

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
            var name = Registry.GetAssetName(
                Registry.FOLDER_TILESETS,
                System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path) ?? "", ts.source)
            );

            // We should be able to locate a tileset with the name given in
            // the registry. Otherwise, this map cannot be used by BattleSDK.
            if (Registry.Tilesets.TryGetId(name, out var id) == false) {
                throw new($"Unknown tileset: '{name}' (path: '{ts.source}').");
            }

            tilesetIds.Add(id);
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
                    if (layer is not null) Terrain.Add(layer);
                }
            }
            // A group named "Metadata" contains the metadata for the tiles.
            else if (g.name == GROUP_NAME_METADATA) {
                // If the layer is named "ZWarps", the data contained are the
                // z warps present in the map.
                foreach (var l in g.layers) {
                    if (l.name == LAYER_NAME_Z_WARPS) {
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

        foreach (var prop in map.Properties) {
            if (prop.name == PROP_NAME_BACKGROUND_MUSIC) {
                if (Registry.Music.TryGetId(prop.value, out int id)) {
                    BackgroundMusic = id;
                }
                else {
                    _logger.Warn(
                        $"Music track '{prop.value}' in map '{Name}' does not exist. " +
                        "The map will be loaded, but no music will play."
                    );
                }
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
    /// Reads the entities file associated with this map, if it exists, and
    /// populates the entity lists.
    /// </summary>
    protected void ReadEntityData () {
        string path = System.IO.Path.ChangeExtension(Path, ".entities.json");
        if (File.Exists(path) == false) return;

        try {
            var json = File.ReadAllText(path);
            var defs = Json.Parse<List<EntityDefinition>>(json);
            if (defs is null) return;

            for (int i = 0; i < defs.Count; i++) {
                var def = defs[i];
                try {
                    if (def.Type == EntityType.Npc) {
                        Npcs.Add(new NpcData(def));
                    }
                }
                catch (Exception ex) {
                    _logger.Error(
                        ex,
                        $"Failed to read entity #{i}. Entity will be ignored."
                    );
                    ex.PrintFancy();
                }
            }
        }
        catch (Exception ex) {
            _logger.Error(
                ex,
                $"Failed to read entities file for {Path}. The map will work, " +
                "but it won't have any entities."
            );
            ex.PrintFancy();
        }
    }

    /// <summary>
    /// Returns true if the position given is within the bounds of this map.
    /// </summary>
    /// <param name="pos">The position to check.</param>
    public bool IsWithinBounds (IVec2 pos) {
        return (pos.X >= 0 && pos.X < Width) && pos.Y >= 0 && pos.Y < Height;
    }
}
