using battlesdk.data.definitions;
using NLog;

namespace battlesdk.data;
public class WarpData : EntityData {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The id of the world to warp to, or -1 if the map doesn't belong to any
    /// world.
    /// </summary>
    private readonly int _worldId = -1;
    /// <summary>
    /// If this warps to a world, the index of the map in the world. Otherwise,
    /// the id of the map in the registry.
    /// </summary>
    private readonly int _mapId = -1;
    private readonly IVec2 _targetPosition;

    public WarpData (EntityDefinition def) : base(def) {
        if (def.Type != EntityType.Warp) {
            throw new ArgumentException("Invalid type.");
        }

        if (def.WarpType is null) {
            throw new InvalidDataException("Missing field: 'warp_type'.");
        }
        if (def.TargetMap is null) {
            throw new InvalidDataException("Missing field: 'target_map'.");
        }
        if (Registry.Maps.TryGetId(def.TargetMap, out var targetMapId) == false) {
            throw new InvalidDataException($"Invalid map: '{def.TargetMap}'.");
        }
        if (def.TargetPosition is null) {
            _logger.Error(
                "Missing field: 'target_position'. (0, 0) will be used instead."
            );
        }

        _targetPosition = def.TargetPosition ?? IVec2.Zero;

        // For each existing world:
        for (int i = 0; i < Registry.Worlds.Count; i++) {
            // Find the index of this map in the world.
            _mapId = Registry.Worlds[i].Maps.FindIndex(m => m.Id == targetMapId);
            // If the world contains the map:
            if (_mapId != -1) {
                _worldId = i;

                // Convert the target position to world coordinates.
                var worldMap = Registry.Worlds[i].Maps[_mapId];
                _targetPosition += worldMap.Position;

                break;
            }
        }

        // If the map is not part of any world.
        if (_worldId == -1) {
            _mapId = targetMapId;
        }
    }
}
