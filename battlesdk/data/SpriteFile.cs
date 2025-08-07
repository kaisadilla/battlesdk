using battlesdk.data.definitions;
using battlesdk.json;
using SDL;

namespace battlesdk.data;

public class SpriteFile : AssetFile {
    /// <summary>
    /// The sprite's width.
    /// </summary>
    public int Width { get; private init; }
    /// <summary>
    /// The sprite's height.
    /// </summary>
    public int Height { get; private init; }

    protected SpriteFile (string name, string path) : base(name, path) {
        unsafe {
            var surface = SDL3_image.IMG_Load(path);

            Width = surface->w;
            Height = surface->h;

            SDL3.SDL_DestroySurface(surface);
        }
    }

    public static SpriteFile New (
        string name, string path, SpriteMetadataDefinition? baseMetadata = null
    ) {
        if (name == "characters/dawn") {
            int k = 3;
        }
        var metadataFile = System.IO.Path.ChangeExtension(path, ".json");
        SpriteMetadataDefinition? def = null;

        if (File.Exists(metadataFile)) {
            var txt = File.ReadAllText(metadataFile);
            def = Json.Parse<SpriteMetadataDefinition>(txt)
                ?? throw new Exception("Failed to parse metadata file.");
        }

        if (baseMetadata is not null) def = baseMetadata.With(def);

        return def?.Type switch {
            null => new SpriteFile(name, path),
            SpriteType.Normal => new SpriteFile(name, path),
            SpriteType.Spritesheet => new SpritesheetFile(name, path, def),
            SpriteType.Frame => new FrameSpriteFile(name, path, def),
            SpriteType.Character => new CharacterSpriteFile(name, path, def),
            _ => throw new InvalidDataException("Unknown sprite type."),
        };
    }
}
