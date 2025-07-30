using battlesdk.data;
using battlesdk.world;
using NLog;
using SDL;

namespace battlesdk.graphics;

public unsafe class Renderer {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private unsafe SDL_Renderer* _renderer;

    private Dictionary<int, TilesetTexture> _tilesetTexes = [];
    private Dictionary<int, CharacterTexture> _charTexes = [];
    private Dictionary<int, StdTexture> _miscTexes = [];

    private int _width;
    private int _height;
    private float _scale;
    private Camera _camera;

    public Renderer (SDL_Window* window, int width, int height, float scale) {
        _renderer = SDL3.SDL_CreateRenderer(window, (string?)null);
        _width = width;
        _height = height;
        _scale = scale;
        _camera = new(width, height, IVec2.Zero);

        EnableScale();

        foreach (var t in Registry.Tilesets) {
            LoadTileset(t);
        }

        foreach (var ch in Registry.CharSprites) {
            LoadCharacterSprite(ch);
        }

        foreach (var ch in Registry.MiscSprites) {
            LoadMiscSprite(ch);
        }
    }

    public void EnableScale () {
        SDL3.SDL_SetRenderLogicalPresentation(
            _renderer,
            _width,
            _height,
            SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_INTEGER_SCALE
        );
    }

    public void DisableScale () {
        SDL3.SDL_SetRenderLogicalPresentation(
            _renderer,
            (int)(_width * _scale),
            (int)(_height * _scale),
            SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_INTEGER_SCALE
        );
    }

    public unsafe void Render () {
        SDL3.SDL_SetRenderDrawColor(_renderer, 0, 0, 0, 255);
        SDL3.SDL_RenderClear(_renderer);

        _camera.Center = G.World.Player.Subposition;
        // TODO: Dynamically adapt to arbitrary amount of z indices.
        for (int i = 0; i < 8; i++) {
            foreach (var map in G.World.Maps) {
                foreach (var l in map.Terrain) {
                    if (l.ZIndex == i) {
                        DrawLayer(l, map.WorldPos.Left, map.WorldPos.Top);
                    }
                }
            }

            List<Character> chars = [];

            foreach (var npc in G.World.Npcs) {
                if (npc.Z == i) {
                    chars.Add(npc);
                }
            }
            if (G.World.Player.Z == i) {
                chars.Add(G.World.Player);
            }

            chars.Sort(((a, b) => a.Subposition.Y.CompareTo(b.Subposition.Y)));

            foreach (var ch in chars) {
                DrawCharacter(ch);
            }
        }

        DisableScale();
        Debug.Draw(_renderer);
        EnableScale();

        SDL3.SDL_RenderPresent(_renderer);
    }

    public unsafe void Destroy () {
        SDL3.SDL_DestroyRenderer(_renderer);
    }

    private unsafe void LoadTileset (Tileset tileset) {
        if (Registry.Tilesets.TryGetId(tileset.Name, out int id)) {
            TilesetTexture tex = new(_renderer, tileset.TexturePath);
            _tilesetTexes[id] = tex;
        }
    }

    private unsafe void LoadCharacterSprite (AssetFile sprite) {
        if (Registry.CharSprites.TryGetId(sprite.Name, out int id)) {
            CharacterTexture tex = new(_renderer, sprite);
            _charTexes[id] = tex;
        }
    }

    private unsafe void LoadMiscSprite (AssetFile sprite) {
        if (Registry.MiscSprites.TryGetId(sprite.Name, out int id)) {
            StdTexture tex = new(_renderer, sprite);
            _miscTexes[id] = tex;
        }
    }

    private void DrawLayer (TileLayer layer, int xOffset, int yOffset) {
        for (int y = 0; y < layer.Height; y++) {
            int yPos = _camera.GetScreenY(y + yOffset);

            if (yPos + Constants.TILE_SIZE < 0) continue;
            if (yPos >= _height) continue;

            for (int x = 0; x < layer.Width; x++) {
                int xPos = _camera.GetScreenX(x + xOffset);

                if (xPos + Constants.TILE_SIZE < 0) continue;
                if (xPos >= _width) continue;

                MapTile? tile = layer[x, y];

                if (tile is null) continue;

                var ts = Registry.Tilesets[tile.TilesetId];
                var tex = _tilesetTexes[tile.TilesetId];

                SDL_FRect src = new() {
                    x = (tile.TileId % ts.Width) * Constants.TILE_SIZE,
                    y = (tile.TileId / ts.Width) * Constants.TILE_SIZE,
                    h = Constants.TILE_SIZE,
                    w = Constants.TILE_SIZE,
                };

                SDL_FRect dst = new() {
                    x = xPos,
                    y = yPos,
                    h = Constants.TILE_SIZE,
                    w = Constants.TILE_SIZE,
                };

                unsafe {
                    SDL3.SDL_RenderTexture(_renderer, tex.Texture, &src, &dst);
                }
            }
        }
    }

    private void DrawPlayer () {
        if (G.World is null) return;
        DrawCharacter(G.World.Player);
    }

    private void DrawCharacter (Character character) {
        int frame = 0;
        if (
            character.IsMoving
            && character.MoveProgress >= 0.25f
            && character.MoveProgress < 0.75f
        ) {
            frame = (character.Position.X + character.Position.Y) % 2 == 0 ? 1 : 2;
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

        unsafe {
            if (Registry.CharSpriteShadow != -1) {
                _miscTexes[Registry.CharSpriteShadow].Draw(
                    _renderer,
                    _camera.GetScreenPos(character.Subposition) + new IVec2(0, 8)
                );
            }

            _charTexes[character.Sprite].Draw(
                _renderer,
                _camera.GetScreenPos(subpos),
                character.Direction,
                (character.IsMoving && !character.IsJumping && character.IsRunning) ? 1 : 0,
                frame
            );
        }
    }
}
