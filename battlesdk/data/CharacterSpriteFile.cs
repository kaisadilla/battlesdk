using battlesdk.data.definitions;
using System.Collections.Immutable;

namespace battlesdk.data;

public class CharacterSpriteFile : SpriteFile {
    public IVec2 SpriteSize { get; }
    public ImmutableDictionary<string, int> Sheets { get; }

    public CharacterSpriteFile (string name, string path, SpriteMetadataDefinition def)
        : base(name, path) 
    {
        if (def.Type != SpriteType.Character) {
            throw new ArgumentException("Invalid type.");
        }
        if (def.SpriteSize is null) {
            throw new InvalidDataException(
                $"Spritesheet metadata must contain a field " +
                $"'{nameof(def.SpriteSize)}' of type [int, int]."
            );
        }
        if (def.Sheets is null) {
            throw new InvalidDataException(
                $"Character sprite metadata must contain a field " +
                $"'{nameof(def.SpriteSize)}' of type [int, int]."
            );
        }

        SpriteSize = def.SpriteSize.Value;

        Dictionary<string, int> sheets = [];
        for (int i = 0; i < def.Sheets.Count; i++) {
            sheets[def.Sheets[i]] = i;
        }
        Sheets = sheets.ToImmutableDictionary();
    }
}
