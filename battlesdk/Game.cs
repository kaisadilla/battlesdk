using battlesdk.graphics;
using battlesdk.screen;
using battlesdk.scripts;
using NLog;
using SDL;

namespace battlesdk;
public class Game {
    const int TARGET_FPS = 60;

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private Window _window;

    public bool CloseRequested => _window.CloseRequested;

    public Game () {
        InitSdl();
        Data.Init();
        Registry.Init();
        PlayerSettings.Init();

        Debug.Init();
        Time.Init();

        Audio.Init();

        _window = new Window(
            Settings.ViewportWidth,
            Settings.ViewportHeight,
            Settings.DefaultRendererScale
        );
        Lua.Init();
        Screen.Init(_window.Renderer);

        G.LoadGame();
        Screen.Push(new OverworldScreenLayer(_window.Renderer));
        InputManager.Push(new OverworldScreenLayer(_window.Renderer));
    }

    public void NextFrame () {
        var frameStart = SDL3.SDL_GetTicks();

        _window.ProcessEvents();
        Debug.OnFrameStart();
        G.World.FrameStart();

        ProcessInput();

        Music.Update();
        G.Update();
        InputManager.Update();
        Hud.Update();
        Screen.Update();

        CoroutineRuntime.Update();

        _window.Render();

        var frameTime = (int)(SDL3.SDL_GetTicks() - frameStart);
        var delay = 1000 / TARGET_FPS;

        if (frameTime < delay) {
            SDL3.SDL_Delay((uint)(delay - frameTime));
        }
    }

    public void Close () {
        _window.Destroy();
    }

    private static unsafe void InitSdl () {
        if (SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO)) {
            _logger.Info("SDL initialized.");
        }
        else {
            throw new InitializationException(
                $"Failed to initialize SDL: {SDL3.SDL_GetError()}."
            );
        }

        if (SDL3_ttf.TTF_Init()) {
            _logger.Info("SDL_ttf initialized.");
        }
        else {
            throw new InitializationException(
                $"Failed to initialize SDL_ttf: {SDL3.SDL_GetError()}."
            );
        }

        SDL_AudioSpec spec = new();

        SDL3_mixer.Mix_Init(
            SDL3_mixer.MIX_INIT_WAVPACK
            | SDL3_mixer.MIX_INIT_OGG
            | SDL3_mixer.MIX_INIT_MP3
        );

        if (SDL3_mixer.Mix_OpenAudio(0, &spec)) {
            _logger.Info("Audio device opened.");
        }
        else {
            // TODO: Allow playing without music, even though it sucks.
            throw new InitializationException(
                $"Failed to open audio device: {SDL3.SDL_GetError()}."
            );
        }

        SDL3_mixer.Mix_AllocateChannels(32);
        SDL3_mixer.Mix_VolumeMusic(32); // volume 0 to 128.
        //SDL3_mixer.Mix_VolumeMusic(6); // volume 0 to 128.

        CustomBlendModes.Init();
    }

    private static unsafe void ProcessInput () {
        if (Controls.GetKeyDown(ActionKey.ToggleDebugInfo)) {
            Debug.PrintToScreen = !Debug.PrintToScreen;
        }
    }
}
