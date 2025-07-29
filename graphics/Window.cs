using SDL;

namespace battlesdk.graphics;
public class Window {
    private unsafe SDL_Window* _window;
    private Renderer _renderer;

    public bool CloseRequested { get; private set; } = false;

    public int TargetFps { get; set; } = 60;

    public unsafe Window (int width, int height, float scale) {
        SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO);

        _window = SDL3.SDL_CreateWindow(
            "BattleSDK",
            (int)(width * scale),
            (int)(height * scale),
            0
        );

        _renderer = new(_window, width, height);
    }

    public unsafe void ProcessEvents () {
        Time.Update();
        Controls.Update();

        var frameStart = SDL3.SDL_GetTicks();

        SDL_Event evt;
        while (SDL3.SDL_PollEvent(&evt)) {
            Controls.RegisterEvent(evt);
            if (evt.Type == SDL_EventType.SDL_EVENT_QUIT) {
                CloseRequested = true;
            }
        }

        var frameTime = (int)(SDL3.SDL_GetTicks() - frameStart);
        var delay = 1000 / TargetFps;

        if (frameTime < delay) {
            SDL3.SDL_Delay((uint)(delay - frameTime));
        }
    }

    public unsafe void Render () {
        _renderer.Render();
    }

    public unsafe void Destroy () {
        _renderer.Destroy();
        SDL3.SDL_DestroyWindow(_window);
        SDL3.SDL_Quit();
    }
}
