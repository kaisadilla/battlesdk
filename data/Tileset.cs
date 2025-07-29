using TiledCS;

namespace battlesdk.data;
public class Tileset : INameable {
    private const string CLASS_FLAGS = "TileFlags";
    private const string CLASS_Z_INDICES = "TileZIndices";
    /// <summary>
    /// The name of the tileset.
    /// </summary>
    public string Name { get; private init; }
    /// <summary>
    /// The absolute path to the image contained by this tileset.
    /// </summary>
    public string TexturePath { get; private init; }
    public TilesetKind Kind { get; private set; } = TilesetKind.Normal;
    /// <summary>
    /// The width of the tileset, in tiles.
    /// </summary>
    public int Width { get; private init; }
    /// <summary>
    /// The height of the tileset, in tiles.
    /// </summary>
    public int Height { get; private init; }
    /// <summary>
    /// The amount of tiles in this tileset.
    /// </summary>
    public int TileCount { get; private init; }

    public List<TileProperties> Tiles { get; } = [];

    public Tileset (string name, string path) {
        Name = name;

        var tiled = new TiledTileset(path);

        var dir = Path.GetDirectoryName(path);
        if (dir is null) throw new Exception("Invalid directory.");

        string texPath = Path.Combine(dir, tiled.Image.source);
        texPath = Path.GetFullPath(texPath);

        TexturePath = texPath;
        Width = tiled.Image.width / tiled.TileWidth;
        Height = tiled.Image.height / tiled.TileHeight;
        TileCount = tiled.TileCount;

        if (tiled.Class == CLASS_FLAGS) {
            Kind = TilesetKind.Flags;
        }
        else if (tiled.Class == CLASS_Z_INDICES) {
            Kind = TilesetKind.ZIndices;
        }
        else {
            for (int i = 0; i < TileCount; i++) {
                var tiledTile = Array.Find(tiled.Tiles, t => t.id == i);
                Tiles.Add(new(tiledTile));
            }
        }
    }
}

public enum TilesetKind {
    Normal,
    Flags,
    ZIndices,
}