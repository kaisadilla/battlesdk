namespace battlesdk.world.entities;
public class Player : Character {
    public Player (IVec2 position) : base(-1, -1, position, "characters/dawn") {
    }

    public override void Update () {
        base.Update();
    }

    public void HandlePrimaryInput () { // TODO: Better name
        var ch = G.World.GetCharacterAt(GetPositionInFront());
        if (ch is not null) {
            ch.Interact(Direction.Opposite());
            return;
        }
    }
}
