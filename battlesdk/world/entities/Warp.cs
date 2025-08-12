using battlesdk.data;
using battlesdk.world.entities.interaction;

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

    private int _transitionScript = -1;

    public Warp (int mapId, int entityId, GameMap map, WarpData data)
        : base(mapId, entityId, map, data)
    {
        WarpType = data.WarpType;
        EntrySound = data.EntrySound;

        _interaction = new DoorEntityInteraction(this, data);

        if (Registry.Scripts.TryGetId("transitions/fade", out int transId)) {
            _transitionScript = transId;
        }
    }

    /// <summary>
    /// This method is called when the player is transferred to a map at the
    /// position this warp is in.
    /// </summary>
    public void OnPlayerArrive () {
        InputManager.PushBlock();

        if (WarpType == WarpType.Door) {
            Coroutine.Start(ExitFromDoor());
        }
        else if (WarpType == WarpType.Hall) {
            Coroutine.Start(ExitFromHall());
        }
    }

    private CoroutineTask ExitFromDoor () {
        yield return new WaitForSeconds(0.75f);
        Screen.PlayScriptTransition(_transitionScript, 0.5f, true);

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
        yield return new WaitForSeconds(0.75f);

        Screen.PlayScriptTransition(_transitionScript, 0.5f, true);
        yield return new WaitForSeconds(0.5f);

        G.World.Player.IsInvisible = false;

        G.World.Player.TryMove(G.World.Player.Direction, true);
        yield return new WaitForSeconds(0.4f);


        InputManager.Pop();
    }
}
