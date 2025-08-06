using battlesdk.data;

namespace battlesdk.world.entities;
public class Npc : Character {
    public Npc (int mapId, int entityId, IVec2 position, string sprite)
        : base(mapId, entityId, position, sprite)
    {

    }

    public Npc (int mapId, int entityId, GameMap map, NpcData data)
        : base(mapId, entityId, map, data)
    {

    }
}
