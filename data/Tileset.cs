using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledCS;

namespace battlesdk.data;
public class Tileset {
    /// <summary>
    /// The name of the tileset.
    /// </summary>
    public string Name { get; private init; }
    /// <summary>
    /// The absolute path to the image contained by this tileset.
    /// </summary>
    public string TexturePath { get; private init; }
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
    }
}
