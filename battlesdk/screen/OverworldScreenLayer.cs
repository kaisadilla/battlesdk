using battlesdk.data;
using battlesdk.graphics;
using battlesdk.input;
using battlesdk.world;
using battlesdk.world.entities;
using SDL;

namespace battlesdk.screen;

public class OverworldScreenLayer : IScreenLayer, IInputListener {
    private const float MOVE_INPUT_DELAY = 0.1f;

    private Renderer _renderer;
    private Camera _camera;
    private Player _player;

    private float _leftKeyStart = float.MinValue;
    private float _rightKeyStart = float.MinValue;
    private float _upKeyStart = float.MinValue;
    private float _downKeyStart = float.MinValue;

    public string Name => "Overworld screen";
    public bool IsTransparent => false;
    public bool BlockOtherInput => true;

    public OverworldScreenLayer (Renderer renderer) {
        _renderer = renderer;
        _camera = new(renderer.Width, renderer.Height, IVec2.Zero);
        _player = G.World.Player;
    }

    public void HandleInput () {
        if (_player.IsMoving == false) {
            if (Controls.GetKeyDown(ActionKey.Left)) {
                _player.SetDirection(Direction.Left);
                _leftKeyStart = Time.TotalTime;
            }
            else if (Controls.GetKeyDown(ActionKey.Right)) {
                _player.SetDirection(Direction.Right);
                _rightKeyStart = Time.TotalTime;
            }
            else if (Controls.GetKeyDown(ActionKey.Up)) {
                _player.SetDirection(Direction.Up);
                _upKeyStart = Time.TotalTime;
            }
            else if (Controls.GetKeyDown(ActionKey.Down)) {
                _player.SetDirection(Direction.Down);
                _downKeyStart = Time.TotalTime;
            }

            if (Controls.GetKey(ActionKey.Left)) {
                if (Time.TotalTime - _leftKeyStart > MOVE_INPUT_DELAY) {
                    _player.TryMove(Direction.Left, false);
                }
            }
            else if (Controls.GetKey(ActionKey.Right)) {
                if (Time.TotalTime - _rightKeyStart > MOVE_INPUT_DELAY) {
                    _player.TryMove(Direction.Right, false);
                }
            }
            else if (Controls.GetKey(ActionKey.Up)) {
                if (Time.TotalTime - _upKeyStart > MOVE_INPUT_DELAY) {
                    _player.TryMove(Direction.Up, false);
                }
            }
            else if (Controls.GetKey(ActionKey.Down)) {
                if (Time.TotalTime - _downKeyStart > MOVE_INPUT_DELAY) {
                    _player.TryMove(Direction.Down, false);
                }
            }

            if (_player.Collided) {
                Audio.PlayCollision();
            }
            if (_player.IsJumping) {
                Audio.PlayJump();
            }
        }

        if (Controls.GetKeyUp(ActionKey.Left)) {
            _leftKeyStart = float.MinValue;
        }
        else if (Controls.GetKeyUp(ActionKey.Right)) {
            _rightKeyStart = float.MinValue;
        }
        else if (Controls.GetKeyUp(ActionKey.Up)) {
            _upKeyStart = float.MinValue;
        }
        else if (Controls.GetKeyUp(ActionKey.Down)) {
            _downKeyStart = float.MinValue;
        }

        if (_player.IsMoving == false) {
            if (Controls.GetKeyDown(ActionKey.Primary)) {
                _player.HandlePrimaryInput();
            }
            if (Controls.GetKeyDown(ActionKey.Menu)) {
                Screen.MainMenu.Open();
            }
        }

        _player.SetRunning(Controls.GetKey(ActionKey.Secondary));

        if (_player.IsMoving) {
            G.World.SetFocus(_player.Position);
        }
    }


    public void OnInputBlocked () {
        _player.SetRunning(false);
    }

    public void Draw () {
        _camera.SetPosition(TileToPixelSpace(G.World.Player.Subposition));

        List<GameMap> visibleMaps = [];
        foreach (var map in G.World.Maps) {
            if (_camera.IsRectVisible(TileToPixelSpace(map.WorldPos))) {
                visibleMaps.Add(map);
            }
        }

        for (int z = 0; z < 8; z++) { // TODO: Do not hardcode z-index like this.
            foreach (var map in visibleMaps) {
                foreach (var l in map.Terrain) {
                    if (l.ZIndex == z) {
                        DrawLayer(l, map.WorldPos.Left, map.WorldPos.Top);
                    }
                }
            }

            List<Character> chars = [];
            List<Entity> entities = [];

            foreach (var warp in G.World.Warps) {
                if (warp.VisualZ == z) {
                    entities.Add(warp);
                }
            }

            foreach (var npc in G.World.Npcs) {
                if (npc.VisualZ == z) {
                    chars.Add(npc);
                }
            }

            if (G.World.Player.VisualZ == z) {
                chars.Add(G.World.Player);
            }

            chars.Sort((a, b) => a.Subposition.Y.CompareTo(b.Subposition.Y));

            foreach (var ent in entities) {
                DrawEntity(ent);
            }

            foreach (var ch in chars) {
                var tiles = G.World.GetTilesAt(ch.Position);
                var onTop = ch.IsMoving && ch.PreviousPosition.Y > ch.Position.Y;

                foreach (var tile in tiles) {
                    if (tile.OnStepUnderTile is not null) {
                        DrawTile(
                            tile.OnStepUnderTile,
                            _camera.GetScreenX(ch.Position.X * Constants.TILE_SIZE),
                            _camera.GetScreenY(ch.Position.Y * Constants.TILE_SIZE)
                        );
                    }
                }
                if (onTop == false) DrawCharacter(ch);
                foreach (var tile in tiles) {
                    if (tile.OnStepOverTile is not null) {
                        DrawTile(
                            tile.OnStepOverTile,
                            _camera.GetScreenX(ch.Position.X * Constants.TILE_SIZE),
                            _camera.GetScreenY(ch.Position.Y * Constants.TILE_SIZE)
                        );
                    }
                }
                if (onTop) DrawCharacter(ch);
            }
        }

        ApplyTimeTint();
    }

    private void DrawLayer (TileLayer layer, int xOffset, int yOffset) {
        for (int y = 0; y < layer.Height; y++) {
            int yPos = _camera.GetScreenY((y + yOffset) * Constants.TILE_SIZE);

            if (yPos + Constants.TILE_SIZE < 0) continue;
            if (yPos >= _renderer.Height) continue;

            for (int x = 0; x < layer.Width; x++) {
                int xPos = _camera.GetScreenX((x + xOffset) * Constants.TILE_SIZE);

                if (xPos + Constants.TILE_SIZE < 0) continue;
                if (xPos >= _renderer.Width) continue;

                MapTile? tile = layer[x, y];

                if (tile is null) continue;

                DrawTile(tile, xPos, yPos);
            }
        }
    }

    private void DrawTile (MapTile tile, int screenX, int screenY) {
        var ts = Registry.Tilesets[tile.TilesetId];
        var tex = _renderer.GetTileset(tile.TilesetId);

        var tileId = tile.Properties.GetCurrentTileId();

        SDL_FRect src = new() {
            x = (tileId % ts.Width) * Constants.TILE_SIZE,
            y = (tileId / ts.Width) * Constants.TILE_SIZE,
            h = Constants.TILE_SIZE,
            w = Constants.TILE_SIZE,
        };

        SDL_FRect dst = new() {
            x = screenX,
            y = screenY,
            h = Constants.TILE_SIZE,
            w = Constants.TILE_SIZE,
        };

        unsafe {
            SDL3.SDL_RenderTexture(_renderer.SdlRenderer, tex.Texture, &src, &dst);
        }
    }

    private void DrawEntity (Entity entity) {
        if (entity.Sprite is null) return;
        if (entity.IsInvisible) return;

        var sprite = _renderer.GetSprite(entity.Sprite.Id);
        if (sprite is null) return;

        sprite.DrawSubsprite(_camera.GetScreenPos(TileToPixelSpace(entity.Subposition)), entity.SpriteIndex);
    }

    private void DrawCharacter (Character character) {
        if (character.Sprite is null) return;
        if (character.IsInvisible) return;
        
        var subpos = character.Subposition;
        
        if (character.IsJumping) {
            if (character.MoveProgress < 0.5) {
                subpos += new Vec2(0, -2 * character.MoveProgress);
            }
            else {
                subpos += new Vec2(0, -2 * (1 - character.MoveProgress));
            }
        }
        else if (character.IsJumpingInPlace) {
            if (character.MoveProgress < 0.5) {
                subpos = (Vec2)character.Position + new Vec2(0, -1 * character.MoveProgress);
            }
            else {
                subpos = (Vec2)character.Position + new Vec2(0, -1 * (1 - character.MoveProgress));
            }
        }
        
        if (Registry.CharSpriteShadow != -1) {
            _renderer.GetSprite(Registry.CharSpriteShadow)?.Draw(
                _camera.GetScreenPos(TileToPixelSpace(character.Subposition)) + new IVec2(0, 8)
            );
        }
        
        var charSprite = _renderer.GetSprite(character.Sprite.Id);
        if (charSprite is null) return;

        charSprite.DrawSubsprite(
            _camera.GetScreenPos(TileToPixelSpace(subpos)),
            character.SpriteIndex
        );
    }

    /// <summary>
    /// Applies the day/night tint to everything that's drawn to the screen so
    /// far, if day/night tint is enabled and properly configured.
    /// </summary>
    private unsafe void ApplyTimeTint () {
        // If time tints are not defined, then nothing is applied.
        if (Data.Misc.TimeTints is null) return;

        int minutes = Time.RealMinutes();
        int hour = minutes / 60;
        float progress = (minutes % 60) / 60f;

        var thisTint = Data.Misc.TimeTints[hour % 24];
        var nextTint = Data.Misc.TimeTints[(hour + 1) % 24];

        var tint = new ColorRGB() {
            R = (int)thisTint.R.Lerp(nextTint.R, progress),
            G = (int)thisTint.G.Lerp(nextTint.G, progress),
            B = (int)thisTint.B.Lerp(nextTint.B, progress),
        };

        SDL3.SDL_SetRenderDrawBlendMode(_renderer.SdlRenderer, CustomBlendModes.Subtract);
        SDL3.SDL_SetRenderDrawColor(
            _renderer.SdlRenderer,
            (byte)Math.Max(0, -tint.R),
            (byte)Math.Max(0, -tint.G),
            (byte)Math.Max(0, -tint.B),
            255
        );
        SDL3.SDL_RenderFillRect(_renderer.SdlRenderer, null);

        SDL3.SDL_SetRenderDrawBlendMode(_renderer.SdlRenderer, CustomBlendModes.Add);
        SDL3.SDL_SetRenderDrawColor(
            _renderer.SdlRenderer,
            (byte)Math.Max(0, tint.R),
            (byte)Math.Max(0, tint.G),
            (byte)Math.Max(0, tint.B),
            255
        );
        SDL3.SDL_RenderFillRect(_renderer.SdlRenderer, null);

        SDL3.SDL_SetRenderDrawBlendMode(_renderer.SdlRenderer, SDL_BlendMode.SDL_BLENDMODE_NONE);
    }
}
