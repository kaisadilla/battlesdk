using battlesdk.data.definitions;

namespace battlesdk.data;

public class SpritesheetFile : SpriteFile {
    public IVec2 SpriteSize { get; }
    public List<string> Names { get; }

    public SpritesheetFile (string name, string path, SpriteMetadataDefinition def)
        : base(name, path) 
    {
        if (def.Type != SpriteType.Spritesheet) {
            throw new ArgumentException("Invalid type.");
        }
        if (def.SpriteSize is null) {
            throw new InvalidDataException(
                $"Spritesheet metadata must contain a field " +
                $"'{nameof(def.SpriteSize)}' of type [int, int]."
            );
        }

        SpriteSize = def.SpriteSize.Value;
        Names = [.. def.Names ?? []];
    }
}
