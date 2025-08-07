using battlesdk.data.definitions;
using System.Collections.Immutable;

namespace battlesdk.data;

public class CharacterSpriteFile : SpritesheetFile {
    private ImmutableDictionary<string, int> _sheets { get; }

    private int _sheetsPerRow;
    private int _walkingSheet;
    private int _runningSheet;
    private int _cyclingSheet;

    public CharacterSpriteFile (string name, string path, SpriteMetadataDefinition def)
        : base(name, path, def)
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

        int spritesheetWidth = SpriteSize.X * 3;
        int spritesheetHeight = SpriteSize.Y * 4;

        _sheetsPerRow = Width / spritesheetWidth;

        Dictionary<string, int> sheets = [];
        for (int i = 0; i < def.Sheets.Count; i++) {
            sheets[def.Sheets[i]] = i;
        }
        _sheets = sheets.ToImmutableDictionary();

        if (_sheets.TryGetValue("walking", out _walkingSheet) == false) {
            _walkingSheet = 0;
        }
        if (_sheets.TryGetValue("running", out _runningSheet) == false) {
            _runningSheet = _walkingSheet;
        }
        if (_sheets.TryGetValue("cycling", out _cyclingSheet) == false) {
            _cyclingSheet = _walkingSheet;
        }
    }

    public int GetSprite (string sheet, int index) {
        if (_sheets.TryGetValue(sheet, out var sheetPos) == false) {
            sheetPos = _walkingSheet;
        }
        return GetSpriteAt(sheetPos, index);
    }

    public int GetWalkingSprite (Direction dir, int index) {
        return GetSpriteAt(_walkingSheet, ((int)dir * 3) + index);
    }

    public int GetRunningSprite (Direction dir, int index) {
        return GetSpriteAt(_runningSheet, ((int)dir * 3) + index);
    }

    public int GetCyclingSprite (Direction dir, int index) {
        return GetSpriteAt(_cyclingSheet, ((int)dir * 3) + index);
    }

    private int GetSpriteAt (int sheetIndex, int index) {
        int sheetX = sheetIndex % _sheetsPerRow;
        int sheetY = sheetIndex / _sheetsPerRow;

        int dx = index % 3;
        int dy = index / 3;

        int spriteX = sheetX * 3 + dx;
        int spriteY = sheetY * 4 + dy;

        return spriteY * _spritesPerRow + spriteX;
    }
}
