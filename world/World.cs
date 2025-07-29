using battlesdk.data;

namespace battlesdk.world;

public class World {
    public List<GameMap> ActiveMaps { get; } = [];

    public Player Player { get; }

    public World (Map startingMap, IVec2 pos) {
        ActiveMaps.Add(new GameMap(startingMap));

        Player = new(pos);
    }

    public void OnFrameStart () {
        Player.OnFrameStart();
    }

    public void Update () {
        Player.Update();
    }

    public List<TileProperties> GetTilesAt (IVec2 pos) {
        if (ActiveMaps[0].Map.IsWithinBounds(pos) == false) return [];

        List<TileProperties> tiles = [];

        foreach (var l in ActiveMaps[0].Layers) {
            var props = l[pos];
            if (props is not null) {
                tiles.Add(props.Properties);
            }
        }

        return tiles;
    }

    /// <summary>
    /// Returns all the tiles at the given position that are interactable from
    /// the given z position.
    /// </summary>
    /// <param name="pos">The position to check.</param>
    /// <param name="zIndex">The z position to check.</param>
    /// <returns></returns>
    public List<TileProperties> GetTilesAt (IVec2 pos, int zIndex) {
        /*
         * A tile is interactable from a given z position if one of three
         * conditions apply:
         * - The tile is in the z position given.
         * - The tile warps to the z position given.
         * - The tile warps to the z position it is in. This one is important
         *   because, even when you are not in the z position of the tile,
         *   entering the tile will transfer you to that z position.
         */

        if (ActiveMaps[0].Map.IsWithinBounds(pos) == false) return [];

        List<TileProperties> tiles = [];

        var zWarp = GetZWarpAt(pos);

        foreach (var l in ActiveMaps[0].Layers) {
            // Check that any of the conditions apply, or else skip this tile.
            if (l.ZIndex != zIndex && l.ZIndex != zWarp && zWarp != zIndex) continue;

            var props = l[pos];
            if (props is not null) {
                tiles.Add(props.Properties);
            }
        }

        return tiles;
    }

    /// <summary>
    /// Returns the z warp present at the given tile, or null if the tile
    /// doesn't have any.
    /// </summary>
    /// <param name="pos">The position to check.</param>
    public int? GetZWarpAt (IVec2 pos) {
        if (ActiveMaps[0].Map.IsWithinBounds(pos) == false) return null;

        if (ActiveMaps[0].ZWarps.IsWarp(pos.X, pos.Y) == false) return null;

        return ActiveMaps[0].ZWarps[pos];
    }
}
