using battlesdk.data;

namespace battlesdk.world.entities;

public class Warp : Entity {
    public Warp (int mapId, int entityId, GameMap map, WarpData data)
        : base(mapId, entityId, map, data)
    {

    }
}
