using battlesdk.data;
using SDL;

namespace battlesdk.graphics;

public unsafe class Renderer {
    private unsafe SDL_Renderer* _renderer;

    private Dictionary<int, TilesetTexture> _tilesetTexes = [];
    private Dictionary<int, CharacterTexture> _charTexes = [];

    private int _width;
    private int _height;
    private Camera _camera;

    public Renderer (SDL_Window* window, int width, int height) {
        _renderer = SDL3.SDL_CreateRenderer(window, (string?)null);
        _width = width;
        _height = height;
        _camera = new(width, height, IVec2.Zero);

        SDL3.SDL_SetRenderLogicalPresentation(_renderer, width, height, SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_INTEGER_SCALE);

        foreach (var t in Registry.Tilesets) {
            LoadTileset(t);
        }

        foreach (var ch in Registry.CharSprites) {
            LoadCharacterSprite(ch);
        }
    }

    public unsafe void Render () {
        SDL3.SDL_SetRenderDrawColor(_renderer, 0, 0, 0, 255);
        SDL3.SDL_RenderClear(_renderer);

        if (G.World is not null) {
            _camera.Center = G.World.Player.Subposition;
        }

        if (G.World is not null) {
            // TODO: Dynamically adapt to arbitrary amount of z indices.
            for (int i = 0; i < 6; i++) {
                foreach (var l in G.World.ActiveMaps[0].Layers) {
                    if (l.ZIndex == i) {
                        DrawLayer(l);
                    }
                }
                if (G.World.Player.Z == i) {
                    DrawPlayer();
                }
            }
        }

        SDL3.SDL_RenderPresent(_renderer);
    }

    public unsafe void Destroy () {
        SDL3.SDL_DestroyRenderer(_renderer);
    }

    private unsafe void LoadTileset (Tileset tileset) {
        TilesetTexture tex = new(_renderer, tileset.TexturePath);
        _tilesetTexes[Registry.Tilesets.Indices[tileset.Name]] = tex;
    }

    private unsafe void LoadCharacterSprite (AssetFile sprite) {
        CharacterTexture tex = new(_renderer, sprite);
        _charTexes[Registry.CharSprites.Indices[sprite.Name]] = tex;
    }

    private void DrawLayer (TileLayer layer) {
        for (int y = 0; y < layer.Height; y++) {
            int yPos = _camera.GetScreenY(y);

            if (yPos + Constants.TILE_SIZE < 0) continue;
            if (yPos >= _height) continue;

            for (int x = 0; x < layer.Width; x++) {
                int xPos = _camera.GetScreenX(x);

                if (xPos + Constants.TILE_SIZE < 0) continue;
                if (xPos >= _width) continue;

                MapTile tile = layer[x, y];

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

        var player = G.World.Player;

        int frame = 0;
        if (player.IsMoving && player.MoveProgress >= 0.25f && player.MoveProgress < 0.75f) {
            frame = (player.Position.X + player.Position.Y) % 2 == 0 ? 1 : 2;
        }

        unsafe {
            _charTexes[0].Draw(
                _renderer,
                _camera.GetScreenPos(player.Subposition),
                player.Direction,
                (player.IsMoving && player.IsRunning) ? 1 : 0,
                frame
            );
        }
    }
}
