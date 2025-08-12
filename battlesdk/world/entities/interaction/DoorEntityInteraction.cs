using battlesdk.data;
using NLog;

namespace battlesdk.world.entities.interaction;

public class DoorEntityInteraction : EntityInteraction {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private int _worldId;
    private int _mapId;
    private IVec2 _targetPos;

    private int _transitionScript = -1;

    public DoorEntityInteraction (Entity entity, WarpData data) : base(entity) {
        Trigger = InteractionTrigger.PlayerTouchesEntity; // TODO: Not hardcoded.
        _worldId = data.WorldId;
        _mapId = data.MapId;
        _targetPos = data.TargetPosition;

        if (Registry.Scripts.TryGetId("transitions/fade", out int transId)) {
            _transitionScript = transId;
        }

        if (data.TargetEntity is not null) {
            MapAsset map;
            if (_worldId != -1) {
                map = Registry.Maps[Registry.Worlds[_worldId].Maps[_mapId].Id];
            }
            else {
                map = Registry.Maps[_mapId];
            }

            bool found = false;
            foreach (var e in map.EnumerateEntities()) {
                if (e.Name is not null && e.Name == data.TargetEntity) {
                    _targetPos = e.Position;
                    found = true;
                    break;
                }
            }

            if (found == false) {
                _logger.Error(
                    $"Couldn't find entity named '{data.TargetEntity}' in map {map.Name}."
                );
            }
        }
    }

    public override void Interact (Direction from) {
        if (IsInteracting == true) return;
        IsInteracting = true;

        InputManager.PushBlock();

        if (_target is Warp warp) {
            if (warp.WarpType == WarpType.Door) {
                Coroutine.Start(DoorAnimation(from));
            }
            else if (warp.WarpType == WarpType.Hall) {
                Coroutine.Start(HallAnimation(from));
            }
        }
    }

    private CoroutineTask HallAnimation (Direction from) {
        G.World.Player.SetDirection(from.Opposite());

        PlayEntrySound();
        G.World.Player.SetRunning(false);
        G.World.Player.Move(G.World.Player.Position.OffsetAt(from.Opposite()));
        Screen.PlayScriptTransition(_transitionScript, 0.5f, false);
        yield return new WaitForSeconds(0.2f);

        G.World.Player.IsInvisible = true;
        yield return new WaitForSeconds(0.5f);

        if (_worldId == -1) {
            var map = Registry.Maps[_mapId];
            G.World.TransferTo(map, _targetPos);
        }
        else {
            var world = Registry.Worlds[_worldId];
            G.World.TransferTo(world, _targetPos);
        }

        End();
    }

    private CoroutineTask DoorAnimation (Direction from) {
        G.World.Player.SetDirection(from.Opposite());

        PlayEntrySound();

        if (_target.Sprite is SpritesheetFile) {
            for (int i = 1; i <= 3; i++) {
                _target.SetSpriteIndex(i);
                yield return new WaitForSeconds(0.12f);
            }
        }

        G.World.Player.SetRunning(false);
        G.World.Player.Move(G.World.Player.Position.OffsetAt(from.Opposite()));
        yield return new WaitForSeconds(0.4f);
        G.World.Player.IsInvisible = true;
        yield return new WaitForSeconds(0.2f);

        Screen.PlayScriptTransition(_transitionScript, 0.5f, false);
        if (_target.Sprite is SpritesheetFile) {
            for (int i = 2; i >= 0; i--) {
                _target.SetSpriteIndex(i);
                yield return new WaitForSeconds(0.12f);
            }
        }

        yield return new WaitForSeconds(0.25f);

        if (_worldId == -1) {
            var map = Registry.Maps[_mapId];
            G.World.TransferTo(map, _targetPos);
        }
        else {
            var world = Registry.Worlds[_worldId];
            G.World.TransferTo(world, _targetPos);
        }

        End();
    }

    private void PlayEntrySound () {
        if (_target is Warp warp && warp.EntrySound is not null) {
            Audio.Play(warp.EntrySound.Value);
        }
    }
}
