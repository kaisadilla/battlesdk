using battlesdk.data;
using battlesdk.graphics;
using battlesdk.world;
using battlesdk.world.entities;
using SDL;

namespace battlesdk.screen;

public class OverworldScreenLayer : IScreenLayer {
    public bool IsTransparent => false;

    private Renderer _renderer;

    private Camera _camera;

    public OverworldScreenLayer (Renderer renderer) {
        _renderer = renderer;
        _camera = new(renderer.Width, renderer.Height, IVec2.Zero);
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

            foreach (var npc in G.World.Npcs) {
                if (npc.VisualZ == z) {
                    chars.Add(npc);
                }
            }

            if (G.World.Player.VisualZ == z) {
                chars.Add(G.World.Player);
            }

            chars.Sort((a, b) => a.Subposition.Y.CompareTo(b.Subposition.Y));

            foreach (var ch in chars) {
                DrawCharacter(ch);
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

        SDL_FRect src = new() {
            x = (tile.TileId % ts.Width) * Constants.TILE_SIZE,
            y = (tile.TileId / ts.Width) * Constants.TILE_SIZE,
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

    private void DrawCharacter (Character character) {
        int frame = 0;
        if (
            character.IsMoving
            && character.MoveProgress >= 0.25f
            && character.MoveProgress < 0.75f
        ) {
            frame = 1 + (character.MoveCount % 2);
        }

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

        unsafe {
            if (Registry.CharSpriteShadow != -1) {
                _renderer.GetSprite(Registry.CharSpriteShadow)?.Draw(
                    _camera.GetScreenPos(TileToPixelSpace(character.Subposition)) + new IVec2(0, 8)
                );
            }

            var charSprite = _renderer.GetSprite(character.Sprite);
            if (charSprite is null) return;

            if (charSprite is GraphicsCharacterSprite gcs) {
                gcs.Draw(
                    _camera.GetScreenPos(TileToPixelSpace(subpos)),
                    character.Direction,
                (character.IsMoving && !character.IsJumping && character.IsRunning) ? 1 : 0,
                frame
                );
            }
            else {
                charSprite.Draw(_camera.GetScreenPos(TileToPixelSpace(subpos)));
            }
        }
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
    }
}
