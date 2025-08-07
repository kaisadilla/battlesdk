using battlesdk.data;
using battlesdk.world.entities;
using NLog;
using System.Diagnostics.CodeAnalysis;

namespace battlesdk.world;

public class World {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private IVec2 _focus = IVec2.Zero;

    private Dictionary<string, Warp> _warps { get; } = [];
    /// <summary>
    /// The NPCs in the world, indexed by a unique id that encodes the map
    /// they belong to as well as their individual id.
    /// </summary>
    private Dictionary<string, Npc> _npcs { get; } = [];

    /// <summary>
    /// The world the player is currently in, if any. This value will be null
    /// when the player is in a map that doesn't belong to any world.
    /// </summary>
    public WorldAsset? CurrentWorld { get; private set; } = null;
    /// <summary>
    /// A list that contains all the maps that are currently loaded in the world.
    /// This includes the map the player is currently in, as well as any map
    /// nearby enough to the player to be loaded.
    /// </summary>
    public List<GameMap> Maps { get; } = [];
    /// <summary>
    /// The character the player controls.
    /// </summary>
    public Player Player { get; }

    public IEnumerable<Warp> Warps => _warps.Values;
    /// <summary>
    /// Contains all the NPCs that are currently loaded in the world.
    /// </summary>
    public IEnumerable<Npc> Npcs => _npcs.Values;

    public World () {
        Player = new(new(0, 0));
    }

    public void LoadNearbyEntities () {
        foreach (var gm in Maps) {
            LoadMapEntities(gm);
        }
    }

    private void LoadMapEntities (GameMap map) {
        for (int i = 0; i < map.Data.Warps.Count; i++) {
            var warpData = map.Data.Warps[i];
            string id = $"{map.Data.Id}_{i}";

            if (_npcs.ContainsKey(id)) continue;

            _warps[id] = new(map.Data.Id, i, map, warpData);
        }
        for (int i = 0; i < map.Data.Npcs.Count; i++) {
            var npcData = map.Data.Npcs[i];
            string id = $"{map.Data.Id}_{i}";

            if (_npcs.ContainsKey(id)) continue;

            _npcs[id] = new(map.Data.Id, i, map, npcData);
        }
    }

    public void CullEntities () {
        CullEntities(_warps);
        CullEntities(_npcs);
    }

    protected void CullEntities<T> (Dictionary<string, T> dict) where T : Entity {
        foreach (var kv in dict) {
            bool remove = true;
            foreach (var map in Maps) {
                // If the map this npc belongs to is loaded, this npc stays loaded.
                if (kv.Value.MapId == map.Data.Id) {
                    remove = false;
                    break;
                }
            }
            if (remove == false) continue;

            var distX = Math.Abs(kv.Value.Position.X - Player.Position.X);
            var distY = Math.Abs(kv.Value.Position.Y - Player.Position.Y);
            if (distX < Constants.LOAD_DISTANCE_X && distY < Constants.LOAD_DISTANCE_Y) {
                continue;
            }

            // TODO: Clean up method for the npc.
            dict.Remove(kv.Key);
        }
    }

    public void FrameStart () {
        foreach (var npc in Npcs) {
            npc.FrameStart();
        }

        Player.FrameStart();
    }

    public void Update () {
        foreach (var npc in Npcs) {
            npc.Update();
        }

        Player.Update();

        if (
            Music.IsFadingOut == false
            && TryGetMapAt(_focus, out var currentMap)
            && Registry.Music.TryGetElement(currentMap.Data.BackgroundMusic, out var track)
        ) {
            int trackId = Music.GetTrackId();
            if (trackId != track.Id) {
                _ = Music.FadeOutMusic();
            }
            if (trackId == -1) {
                Music.FadeIn(track);
            }
        }
    }

    /// <summary>
    /// Loads the world given, placing the player at the position given.
    /// </summary>
    /// <param name="world"></param>
    /// <param name="position"></param>
    public void TransferTo (WorldAsset world, IVec2 position) {
        _logger.Info($"Player transferred to world '{world.Name}' at pos {position}.");

        CurrentWorld = world;
        Player.SetPosition(position);
        SetFocus(Player.Position);
    }

    public void TransferTo (MapAsset map, IVec2 position) {
        throw new NotImplementedException();
    }

    public void SetFocus (IVec2 worldPos) {
        if (CurrentWorld is null) return;

        _focus = worldPos;

        foreach (var worldMap in CurrentWorld.Maps) {
            if (Registry.Maps.TryGetElement(worldMap.Id, out var mapData) == false) {
                _logger.Error($"Couldn't find map with id '{worldMap.Id}'.");
                continue;
            }

            bool isInside = worldPos.X >= (worldMap.Position.X - Constants.LOAD_DISTANCE_X)
                && worldPos.X < (worldMap.Position.X + mapData.Width + Constants.LOAD_DISTANCE_X)
                && worldPos.Y >= (worldMap.Position.Y - Constants.LOAD_DISTANCE_Y)
                && worldPos.Y < (worldMap.Position.Y + mapData.Height + Constants.LOAD_DISTANCE_Y);

            if (isInside == false) {
                RemoveMap(worldMap.Id);
                continue;
            }
            else {
                AddMap(mapData, worldMap.Position.X, worldMap.Position.Y);
            }
        }

        CullEntities();
        LoadNearbyEntities();
    }

    private void AddMap (MapAsset mapData, int x, int y) {
        foreach (var m in Maps) {
            if (m.Data.Id == mapData.Id) return;
        }

        Maps.Add(new(mapData, x, y));
        _logger.Info($"Added map '{mapData.Name}'.");
    }

    private void RemoveMap (int id) {
        for (int i = 0; i < Maps.Count; i++) {
            if (Maps[i].Data.Id == id) {
                _logger.Info($"Removed map '{Maps[i].Data.Name}'.");
                Maps.RemoveAt(i);
                break;
            }
        }
    }

    public List<TileProperties> GetTilesAt (IVec2 worldPos) {
        if (TryGetMapAt(worldPos, out var map) == false) return [];

        List<TileProperties> tiles = [];

        foreach (var l in map.Terrain) {
            var props = l[worldPos];
            if (props is not null) {
                tiles.Add(props.Properties);
            }
        }

        return tiles;
    }

    /// <summary>
    /// Returns the character that is at the given position, if any. If multiple
    /// characters exist in the same position, only one of them will be returned.
    /// This method will return the player if the player is at the given position.
    /// </summary>
    /// <param name="worldPos">A position in the world.</param>
    public Entity? GetEntityAt (IVec2 worldPos) {
        if (Player.Position == worldPos) return Player;

        foreach (var ch in Npcs) {
            if (ch.Position == worldPos) return ch;
        }

        foreach (var warp in Warps) {
            if (warp.Position == worldPos) return warp;
        }

        return null;
    }

    /// <summary>
    /// Gets the game map at the position given, if there's any.
    /// </summary>
    /// <param name="worldPos">The position in the world</param>
    /// <param name="map">The map at that position.</param>
    public bool TryGetMapAt (IVec2 worldPos, [NotNullWhen(true)] out GameMap? map) {
        foreach (var m in Maps) {
            if (m.IsInsideBounds(worldPos)) {
                map = m;
                return true;
            }
        }

        map = null;
        return false;
    }

    /// <summary>
    /// Returns all the tiles at the given position that are interactable from
    /// the given z position.
    /// </summary>
    /// <param name="worldPos">The position to check.</param>
    /// <param name="zIndex">The z position to check.</param>
    /// <returns></returns>
    public List<TileProperties> GetTilesAt (IVec2 worldPos, int zIndex) {
        /*
         * A tile is interactable from a given z position if one of three
         * conditions apply:
         * - The tile is in the z position given.
         * - The tile warps to the z position it is in.
         * - The tile warps to the z position directly above it.
         */

        if (TryGetMapAt(worldPos, out var map) == false) return [];
        var localPos = map.GetLocalPos(worldPos);

        List<TileProperties> tiles = [];

        var zWarp = GetZWarpAt(worldPos);

        foreach (var l in map.Terrain) {
            // Check that any of the conditions apply, or else skip this tile.
            if (
                l.ZIndex == zIndex
                || (zWarp is not null && l.ZIndex == zWarp)
                || (zWarp is not null && l.ZIndex == zWarp - 1)
            ) {
                var props = l[localPos];
                if (props is not null) {
                    tiles.Add(props.Properties);
                }
            }
        }

        return tiles;
    }

    /// <summary>
    /// Returns the z warp present at the given tile, or null if the tile
    /// doesn't have any.
    /// </summary>
    /// <param name="worldPos">The position to check.</param>
    public int? GetZWarpAt (IVec2 worldPos) {
        if (TryGetMapAt(worldPos, out var map) == false) return null;

        var localPos = map.GetLocalPos(worldPos);

        if (map.ZWarps.IsWarp(localPos.X, localPos.Y) == false) return null;

        return map.ZWarps[localPos];
    }
}
