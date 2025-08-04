using battlesdk.data.definitions;

namespace battlesdk.data;

public abstract class EntityData {
    public int Sprite { get; }
    public IVec2 Position { get; }
    public EntityInteractionData? Interaction { get; } = null;

    public EntityData (EntityDefinition def) {
        if (string.IsNullOrEmpty(def.Sprite)) {
            throw new InvalidDataException("Entity is missing field 'sprite'.");
        }
        if (Registry.CharSprites.TryGetId(def.Sprite, out int spriteId) == false) {
            throw new InvalidDataException(
                $"Couldn't find character sprite '{def.Sprite}'."
            );
        }

        Sprite = spriteId;
        Position = def.Position;

        if (def.Interaction is not null) {
            Interaction = EntityInteractionData.New(def.Interaction);
        }
    }
}

public abstract class EntityInteractionData {
    public static EntityInteractionData New (EntityInteractionDefinition def) {
        return def.Type switch {
            EntityInteractionType.Script => new ScriptEntityInteractionData(def),
            EntityInteractionType.Message => throw new NotImplementedException(),
            _ => throw new InvalidDataException(
                $"Unknown entity interaction type: '{def.Type}'"
            ),
        };
    }
}

public class ScriptEntityInteractionData : EntityInteractionData {
    public int ScriptId { get; }
    public ScriptEntityInteractionData (EntityInteractionDefinition def) {
        if (def.Type != EntityInteractionType.Script) {
            throw new ArgumentException("Invalid definition type.");
        }
        if (string.IsNullOrEmpty(def.Script)) {
            throw new InvalidDataException(
                "Script interaction is missing field 'script'."
            );
        }
        if (Registry.Scripts.TryGetId(def.Script, out int scriptId) == false) {
            throw new InvalidDataException(
                $"Couldn't find script '{def.Script}'."
            );
        }

        ScriptId = scriptId;
    }
}
