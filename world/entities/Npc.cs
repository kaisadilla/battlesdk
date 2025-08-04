using battlesdk.data;

namespace battlesdk.world.entities;
public class Npc : Character {
    public Npc (IVec2 position, string sprite) : base(position, sprite) {
    }

    public Npc (GameMap map, NpcData data) : base(map, data) {

    }
}
