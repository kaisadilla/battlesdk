using battlesdk.data.definitions;
using battlesdk.world.entities;
using NLog;

namespace battlesdk.data;
public class WarpData : EntityData {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public WarpType WarpType { get; }
    /// <summary>
    /// The id of the world to warp to, or -1 if the map doesn't belong to any
    /// world.
    /// </summary>
    public int WorldId { get; } = -1;
    /// <summary>
    /// If this warps to a world, the index of the map in the world. Otherwise,
    /// the id of the map in the registry.
    /// </summary>
    public int MapId { get; } = -1;
    public int? EntrySound { get; }
    public string? TargetEntity { get; }
    public IVec2 TargetPosition { get; }

    public WarpData (EntityDefinition def) : base(def) {
        if (def.WarpType is null) {
            throw new InvalidDataException("Missing field: 'warp_type'.");
        }
        if (def.TargetMap is null) {
            throw new InvalidDataException("Missing field: 'target_map'.");
        }
        if (Registry.Maps.TryGetId(def.TargetMap, out var targetMapId) == false) {
            throw new InvalidDataException($"Invalid map: '{def.TargetMap}'.");
        }

        WarpType = def.WarpType.Value;

        if (def.EntrySound is not null) {
            if (Registry.Sounds.TryGetId(def.EntrySound, out int soundId)) {
                EntrySound = soundId;
            }
            else {
                _logger.Error($"Invalid sound: '{def.EntrySound}'.");
            }
        }

        if (def.TargetEntity is not null) {
            TargetEntity = def.TargetEntity;
        }
        else if (def.TargetPosition is not null) {
            TargetPosition = def.TargetPosition.Value;
        }
        else {
            _logger.Error(
                "Missing target position or entity. (0, 0) will be used instead."
            );

            TargetPosition = IVec2.Zero;
        }

        // For each existing world:
        for (int i = 0; i < Registry.Worlds.Count; i++) {
            // Find the index of this map in the world.
            MapId = Registry.Worlds[i].Maps.FindIndex(m => m.Id == targetMapId);
            // If the world contains the map:
            if (MapId != -1) {
                WorldId = i;

                // Convert the target position to world coordinates.
                var worldMap = Registry.Worlds[i].Maps[MapId];
                TargetPosition += worldMap.Position;

                break;
            }
        }

        // If the map is not part of any world.
        if (WorldId == -1) {
            MapId = targetMapId;
        }
    }
}
