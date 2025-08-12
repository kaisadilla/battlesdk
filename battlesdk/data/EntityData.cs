using battlesdk.data.definitions;
using battlesdk.world.entities.interaction;

namespace battlesdk.data;

public abstract class EntityData {
    public int? Sprite { get; } = null;
    public string? Name { get; } = null;
    public IVec2 Position { get; }
    public EntityInteractionData? Interaction { get; } = null;

    public EntityData (EntityDefinition def) {
        if (string.IsNullOrEmpty(def.Sprite) == false) {
            if (Registry.Sprites.TryGetId(def.Sprite, out int spriteId) == false) {
                throw new InvalidDataException(
                    $"Couldn't find character sprite '{def.Sprite}'."
                );
            }

            Sprite = spriteId;
        }

        Position = def.Position;

        if (string.IsNullOrEmpty(def.Name) == false) {
            Name = def.Name;
        }

        if (def.Interaction is not null) {
            Interaction = EntityInteractionData.New(def.Interaction);
        }
    }
}

public abstract class EntityInteractionData {
    public InteractionTrigger Trigger { get; }

    protected EntityInteractionData (EntityInteractionDefinition def) {
        Trigger = def.Trigger ?? InteractionTrigger.ActionButton;
    }

    public static EntityInteractionData New (EntityInteractionDefinition def) {
        return def.Type switch {
            EntityInteractionType.Script => new ScriptEntityInteractionData(def),
            EntityInteractionType.Message => new MessageEntityInteractionData(def),
            _ => throw new InvalidDataException(
                $"Unknown entity interaction type: '{def.Type}'"
            ),
        };
    }
}

public class ScriptEntityInteractionData : EntityInteractionData {
    public int ScriptId { get; }

    public ScriptEntityInteractionData (EntityInteractionDefinition def) : base(def) {
        if (def.Type != EntityInteractionType.Script) {
            throw new ArgumentException("Invalid definition type.");
        }
        if (string.IsNullOrEmpty(def.Script)) {
            throw new InvalidDataException(
                "Missing field: 'script'."
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

public class MessageEntityInteractionData : EntityInteractionData {
    public List<string> TextKeys { get; }

    public MessageEntityInteractionData (EntityInteractionDefinition def) : base(def) {
        if (def.Type != EntityInteractionType.Message) {
            throw new ArgumentException("Invalid definition type.");
        }

        if (def.TextKeys is not null && def.TextKeys.Count > 0) {
            TextKeys = [.. def.TextKeys];
            return;
        }

        if (string.IsNullOrEmpty(def.TextKey)) {
            throw new InvalidDataException(
                "Missing field: 'text_key' or 'text_keys'."
            );
        }

        TextKeys = [def.TextKey];
    }
}
