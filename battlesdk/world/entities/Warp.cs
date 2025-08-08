using battlesdk.data;

namespace battlesdk.world.entities;

public enum WarpType {
    /// <summary>
    /// This warp is a door seen from the front. Using this door will show an
    /// animation of the door opening.
    /// </summary>
    Door,
    /// <summary>
    /// This warp is the hall after a door or other entry that is behind and
    /// cannot be seen.
    /// </summary>
    Hall,
}

public class Warp : Entity {
    public WarpType WarpType { get; }
    public int? EntrySound { get; }

    public Warp (int mapId, int entityId, GameMap map, WarpData data)
        : base(mapId, entityId, map, data)
    {
        WarpType = data.WarpType;
        EntrySound = data.EntrySound;

        _interaction = new DoorEntityInteraction(this, data);
    }

    /// <summary>
    /// This method is called when the player is transferred to a map at the
    /// position this warp is in.
    /// </summary>
    public void OnPlayerArrive () {
        if (WarpType == WarpType.Door) {
            Coroutine.Start(ExitFromDoor());
        }
        else if (WarpType == WarpType.Hall) {
            Coroutine.Start(ExitFromHall());
        }
    }

    private CoroutineTask ExitFromDoor () {
        InputManager.PushBlock();

        yield return new WaitForSeconds(0.75f);
        Screen.FadeFromBlack(0.5f);

        if (EntrySound is not null) {
            Audio.Play(EntrySound.Value);
        }
        if (Sprite is SpritesheetFile) {
            for (int i = 1; i <= 3; i++) {
                SetSpriteIndex(i);
                yield return new WaitForSeconds(0.12f);
            }
        }

        G.World.Player.IsInvisible = false;
        yield return new WaitForSeconds(0.2f);

        G.World.Player.TryMove(G.World.Player.Direction, true);
        yield return new WaitForSeconds(0.4f);

        if (Sprite is SpritesheetFile) {
            for (int i = 2; i >= 0; i--) {
                SetSpriteIndex(i);
                yield return new WaitForSeconds(0.12f);
            }
        }

        InputManager.Pop();
    }

    private CoroutineTask ExitFromHall () {
        InputManager.PushBlock();

        yield return new WaitForSeconds(0.75f);

        Screen.FadeFromBlack(0.5f);
        yield return new WaitForSeconds(0.5f);

        G.World.Player.IsInvisible = false;

        G.World.Player.TryMove(G.World.Player.Direction, true);
        yield return new WaitForSeconds(0.4f);


        InputManager.Pop();
    }
}
