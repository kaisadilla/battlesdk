using NLog;
using SDL;

namespace battlesdk.graphics;
public class Window {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private unsafe SDL_Window* _window;
    private Renderer _renderer;

    public bool CloseRequested { get; private set; } = false;

    public unsafe Window (int width, int height, float scale) {
        _window = SDL3.SDL_CreateWindow(
            "BattleSDK",
            (int)(width * scale),
            (int)(height * scale),
            0
        );

        _renderer = new(_window, width, height, scale);
    }

    public unsafe void ProcessEvents () {
        Time.Update();
        Controls.Update();

        SDL_Event evt;
        while (SDL3.SDL_PollEvent(&evt)) {
            Controls.RegisterEvent(evt);
            if (evt.Type == SDL_EventType.SDL_EVENT_QUIT) {
                CloseRequested = true;
            }
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
