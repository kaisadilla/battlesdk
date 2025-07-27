using battlesdk.data;
using Hexa.NET.SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace battlesdk.graphics;
public class Window {
    private unsafe SDLWindow* _window;
    private unsafe SDLRenderer* _renderer;

    private Dictionary<int, TilesetTexture> _tilesetTexes = [];

    public bool CloseRequested { get; private set; } = false;

    public int TargetFps { get; set; } = 60;

    public unsafe Window () {
        SDL.Init(SDL.SDL_INIT_VIDEO);

        const int WIDTH = 340;
        const int HEIGHT = 240;

        _window = SDL.CreateWindow(
            "BattleSDK",
            (int)SDL.SDL_WINDOWPOS_CENTERED_MASK,
            (int)SDL.SDL_WINDOWPOS_CENTERED_MASK,
            WIDTH * 3,
            HEIGHT * 3,
            0
        );
        _renderer = SDL.CreateRenderer(_window, -1, 0);

        SDL.RenderSetLogicalSize(_renderer, WIDTH, HEIGHT);
    }

    public unsafe void ProcessEvents () {
        var frameStart = SDL.GetTicks();
        SDLEvent evt;

        while (SDL.PollEvent(&evt) != 0) {
            if (evt.Type == (uint)SDLEventType.Quit) {
                CloseRequested = true;
            }
        }

        var frameTime = (int)(SDL.GetTicks() - frameStart);
        var delay = 1000 / TargetFps;

        if (frameTime < delay) {
            SDL.Delay((uint)(delay - frameTime));
        }
    }

    public unsafe void Render () {
        SDL.SetRenderDrawColor(_renderer, 0, 0, 0, 255);
        SDL.RenderClear(_renderer);

        var map = Registry.Maps[0];

        foreach (var layer in map.Layers) {
            for (int y = 0; y < map.Height; y++) {
                for (int x = 0; x < map.Width; x++) {
                    Tile tile = layer[x, y];

                    if (tile == Tile.Empty) continue;

                    var ts = Registry.Tilesets[tile.TilesetId];
                    var tex = _tilesetTexes[tile.TilesetId];

                    SDLRect src = new() {
                        X = (tile.TileId % ts.Width) * 16,
                        Y = (tile.TileId / ts.Width) * 16,
                        H = 16,
                        W = 16,
                    };

                    SDLRect dst = new() {
                        X = x * 16,
                        Y = y * 16,
                        H = 16,
                        W = 16,
                    };

                    SDL.RenderCopy(_renderer, tex.Texture, ref src, ref dst);
                }
            }
        }

        //SDLRect grass = new() {
        //    X = 16,
        //    Y = 0,
        //    H = 16,
        //    W = 16,
        //};
        //
        //SDLRect corner = new() {
        //    X = 0,
        //    Y = 16 * 12,
        //    H = 16,
        //    W = 16,
        //};
        //
        //for (int x = 0; x < 6; x++) {
        //    for (int y = 0; y < 6; y++) {
        //        SDLRect dst = new() {
        //            X = x * 16,
        //            Y = y * 16,
        //            H = 16,
        //            W = 16,
        //        };
        //
        //        SDL.RenderCopy(_renderer, _tilesetTexes[0].Texture, ref grass, ref dst);
        //    }
        //}
        //
        //SDLRect dst2 = new() {
        //    X = 0,
        //    Y = 0,
        //    H = 16,
        //    W = 16,
        //};
        //
        //SDL.RenderCopy(_renderer, _tilesetTexes[0].Texture, ref corner, ref dst2);

        SDL.RenderPresent(_renderer);
    }

    public unsafe void Destroy () {
        SDL.DestroyRenderer(_renderer);
        SDL.DestroyWindow(_window);
        SDL.Quit();
    }

    public unsafe void LoadTileset (Tileset tileset) {
        TilesetTexture tex = new(_renderer, tileset.TexturePath);
        _tilesetTexes[Registry.TilesetIndices[tileset.Name]] = tex;
    }
}
