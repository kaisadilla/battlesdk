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
    private Renderer _renderer;

    private WorldScene _worldScene;

    public bool CloseRequested { get; private set; } = false;

    public int TargetFps { get; set; } = 60;

    public unsafe Window (int width, int height, float scale) {
        SDL.Init(SDL.SDL_INIT_VIDEO);

        _window = SDL.CreateWindow(
            "BattleSDK",
            (int)SDL.SDL_WINDOWPOS_CENTERED_MASK,
            (int)SDL.SDL_WINDOWPOS_CENTERED_MASK,
            (int)(width * scale),
            (int)(height * scale),
            0
        );

        _renderer = new(_window, width, height);
    }

    public unsafe void ProcessEvents () {
        Time.Update();
        Controls.Update();

        var frameStart = SDL.GetTicks();

        SDLEvent evt;
        while (SDL.PollEvent(&evt) != 0) {
            Controls.RegisterEvent(evt);
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
        _renderer.Render();
    }

    public unsafe void Destroy () {
        _renderer.Destroy();
        SDL.DestroyWindow(_window);
        SDL.Quit();
    }
}
