using battlesdk.data;
using Hexa.NET.SDL2;

namespace battlesdk.graphics;

public unsafe class Renderer {
    private unsafe SDLRenderer* _renderer;

    private Dictionary<int, TilesetTexture> _tilesetTexes = [];
    private Dictionary<int, CharacterTexture> _charTexes = [];

    private int _width;
    private int _height;
    private Camera _camera;

    public Renderer (SDLWindow* window, int width, int height) {
        _renderer = SDL.CreateRenderer(window, -1, 0);
        _width = width;
        _height = height;
        _camera = new(width, height, IVec2.Zero);

        SDL.RenderSetLogicalSize(_renderer, width, height);

        foreach (var t in Registry.Tilesets) {
            LoadTileset(t);
        }

        foreach (var ch in Registry.CharSprites) {
            LoadCharacterSprite(ch);
        }
    }

    public unsafe void Render () {
        SDL.SetRenderDrawColor(_renderer, 0, 0, 0, 255);
        SDL.RenderClear(_renderer);

        if (G.World is not null) {
            _camera.Center = G.World.Player.Subposition;
        }

        DrawWorld();
        DrawPlayer();

        SDL.RenderPresent(_renderer);
    }

    public unsafe void Destroy () {
        SDL.DestroyRenderer(_renderer);
    }

    private unsafe void LoadTileset (Tileset tileset) {
        TilesetTexture tex = new(_renderer, tileset.TexturePath);
        _tilesetTexes[Registry.Tilesets.Indices[tileset.Name]] = tex;
    }

    private unsafe void LoadCharacterSprite (AssetFile sprite) {
        CharacterTexture tex = new(_renderer, sprite);
        _charTexes[Registry.CharSprites.Indices[sprite.Name]] = tex;
    }

    private void DrawWorld () {
        if (G.World is null) return;
        if (G.World.ActiveMaps.Count == 0) return;

        foreach (var layer in G.World.ActiveMaps[0].Layers) {
            for (int y = 0; y < G.World.ActiveMaps[0].Map.Height; y++) {
                int yPos = _camera.GetScreenY(y);

                if (yPos + Constants.TILE_SIZE < 0) continue;
                if (yPos >= _height) continue;

                for (int x = 0; x < G.World.ActiveMaps[0].Map.Width; x++) {
                    int xPos = _camera.GetScreenX(x);

                    if (xPos + Constants.TILE_SIZE < 0) continue;
                    if (xPos >= _width) continue;

                    MapTile tile = layer[x, y];

                    if (tile == MapTile.Empty) continue;

                    var ts = Registry.Tilesets[tile.TilesetId];
                    var tex = _tilesetTexes[tile.TilesetId];

                    SDLRect src = new() {
                        X = (tile.TileId % ts.Width) * Constants.TILE_SIZE,
                        Y = (tile.TileId / ts.Width) * Constants.TILE_SIZE,
                        H = Constants.TILE_SIZE,
                        W = Constants.TILE_SIZE,
                    };

                    SDLRect dst = new() {
                        X = xPos,
                        Y = yPos,
                        H = Constants.TILE_SIZE,
                        W = Constants.TILE_SIZE,
                    };

                    unsafe {
                        SDL.RenderCopy(_renderer, tex.Texture, ref src, ref dst);
                    }
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
