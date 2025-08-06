using battlesdk.data.definitions;

namespace battlesdk.data;

public class NpcData : CharacterData {
    public NpcData (EntityDefinition def) : base(def) {
        if (def.Type != EntityType.Npc) {
            throw new ArgumentException("Invalid definition type.");
        }
    }
}