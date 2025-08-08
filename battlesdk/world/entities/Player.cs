namespace battlesdk.world.entities;
public class Player : Character {
    public Player (IVec2 position) : base(-1, -1, position, "characters/dawn") {
    }

    public override void Update () {
        base.Update();
    }

    public void HandlePrimaryInput () { // TODO: Better name
        var entities = G.World.GetEntitiesAt(GetPositionInFront());
        if (entities.Count > 0) {
            entities[0].OnPrimaryAction(Direction.Opposite());
            return;
        }
    }
}
