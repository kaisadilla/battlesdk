using battlesdk.data.definitions;
using battlesdk.world;

namespace battlesdk.data;

public class CharacterData : EntityData {
    public CharacterMovementData? Movement { get; }

    public CharacterData (EntityDefinition def) : base(def) {
        if (def.Movement is not null) {
            Movement = CharacterMovementData.New(def.Movement);
        }
    }
}

public abstract class CharacterMovementData {
    public static CharacterMovementData New (CharacterMovementDefinition def) {
        return def.Type switch {
            CharacterMovementType.Route => new RouteCharacterMovementData(def),
            CharacterMovementType.Random => new RandomCharacterMovementData(def),
            CharacterMovementType.LookAround => new LookAroundCharacterMovementData(def),
            _ => throw new InvalidDataException(
                $"Unknown character movement type: '{def.Type}'"
            ),
        };
    }
}

public class RouteCharacterMovementData : CharacterMovementData {
    public List<MoveKind> Route { get; }

    public RouteCharacterMovementData (CharacterMovementDefinition def) {
        if (def.Type != CharacterMovementType.Route) {
            throw new ArgumentException("Invalid definition type.");
        }

        if (def.Route is null || def.Route.Count == 0) {
            throw new InvalidDataException(
                "Field 'route' must exist and contain, at least, one element."
            );
        }

        Route = [.. def.Route];
    }
}

public class RandomCharacterMovementData : CharacterMovementData {
    public RandomCharacterMovementData (CharacterMovementDefinition def) {
        if (def.Type != CharacterMovementType.Random) {
            throw new ArgumentException("Invalid definition type.");
        }
    }
}

public class LookAroundCharacterMovementData : CharacterMovementData {
    public LookAroundCharacterMovementData (CharacterMovementDefinition def) {
        if (def.Type != CharacterMovementType.LookAround) {
            throw new ArgumentException("Invalid definition type.");
        }
    }
}