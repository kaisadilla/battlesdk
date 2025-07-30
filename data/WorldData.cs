using NLog;
using System.Text.Json;

namespace battlesdk.data;

public class WorldData : INameable {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public string Name { get; private init; }
    public int Id { get; private set; }
    public string Path { get; private init; }

    /// <summary>
    /// A list of maps that belong to this world.
    /// </summary>
    public List<WorldMapData> Maps { get; } = [];

    public WorldData (string name, string path) {
        Name = name;
        Path = path;

        var txt = File.ReadAllText(path);
        using var json = JsonDocument.Parse(txt);
        var root = json.RootElement;

        if (
            root.TryGetProperty("type", out var typeJson) == false
            || typeJson.ValueKind != JsonValueKind.String
            || typeJson.GetString() != "world"
        ) throw new InvalidDataException(
            "World must contain a field 'type' with the value 'world'."
        );

        if (
            root.TryGetProperty("maps", out var mapsJson) == false
            || mapsJson.ValueKind != JsonValueKind.Array
            || mapsJson.GetArrayLength() == 0
        ) throw new InvalidDataException(
            "World must contain a field 'maps' with an array of maps. " +
            "This array must contain, at least, one map."
        );

        foreach (var m in mapsJson.EnumerateArray()) {
            if (
                m.TryGetProperty("fileName", out var fnJson) == false
                || fnJson.ValueKind != JsonValueKind.String
            ) {
                _logger.Error(
                    $"Map in world '{Name}' is missing property 'fileName' of type String."
                );
                continue;
            }

            var fileName = fnJson.GetString()!;
            var mapName = Registry.GetAssetName(Registry.FOLDER_MAPS, fileName);

            if (Registry.Maps.TryGetId(mapName, out int mapId) == false) {
                _logger.Error(
                    $"World '{Name}' contains an invalid reference to a map: '{fileName}'."
                );
                continue;
            }

            if (
                m.TryGetProperty("x", out var xJson) == false
                || xJson.TryGetInt32(out int x) == false
            ) {
                _logger.Error(
                    $"Map '{fileName}' in world {Name} is missing property 'x' of type Number."
                );
                continue;
            }
            if (
                m.TryGetProperty("y", out var yJson) == false
                || yJson.TryGetInt32(out int y) == false
            ) {
                _logger.Error(
                    $"Map '{fileName}' in world {Name} is missing property 'y' of type Number."
                );
                continue;
            }

            Maps.Add(new WorldMapData {
                Id = mapId,
                Position = new(x / Constants.TILE_SIZE, y / Constants.TILE_SIZE),
            });
        }
    }

    public void SetId (int id) {
        Id = id;
    }
}

public class WorldMapData {
    /// <summary>
    /// The id of the map in the registry.
    /// </summary>
    public required int Id { get; init; }
    /// <summary>
    /// The position of the map in this world.
    /// </summary>
    public required IVec2 Position { get; init; }
}