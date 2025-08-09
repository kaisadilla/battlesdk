global using battlesdk.types;
global using static battlesdk.types.TypesUtils;

using battlesdk;
using battlesdk.graphics;
using battlesdk.screen;
using battlesdk.scripts;
using NLog;
using SDL;

const int TARGET_FPS = 60;

Window win;

Logger _logger = LogManager.GetCurrentClassLogger();

_logger.Info("Launching BattleSDK.");

try {
    InitSdl();
    Data.Init();
    Registry.Init();
    Settings.Init();

    Debug.Init();
    Time.Init();

    Audio.Init();

    win = new Window(
        Constants.VIEWPORT_WIDTH,
        Constants.VIEWPORT_HEIGHT,
        Constants.DEFAULT_SCREEN_SCALE
    );
    Lua.Init();
    Screen.Init(win.Renderer);

    G.LoadGame();
    Screen.Push(new OverworldScreenLayer(win.Renderer));
    InputManager.Push(new OverworldScreenLayer(win.Renderer));
}
catch (Exception ex) {
    _logger.FatalEx(ex, $"A fatal error occurred during initialization.");

    Environment.Exit(1);
    return;
}

while (win.CloseRequested == false) {
    try {
        var frameStart = SDL3.SDL_GetTicks();

        win.ProcessEvents();
        Debug.OnFrameStart();
        G.World.FrameStart();

        ProcessInput();

        Music.Update();
        G.World.Update();
        InputManager.Update();
        Hud.Update();
        Screen.Update();

        CoroutineRuntime.Update();

        win.Render();

        var frameTime = (int)(SDL3.SDL_GetTicks() - frameStart);
        var delay = 1000 / TARGET_FPS;

        if (frameTime < delay) {
            SDL3.SDL_Delay((uint)(delay - frameTime));
        }
    }
    catch (Exception ex) {
        _logger.FatalEx(ex, $"An unrecoverable error ocurred in the program loop.");

        win.Destroy();

        Environment.Exit(1);
        return;

    }
}

win.Destroy();

unsafe void InitSdl () {
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

    CustomBlendModes.Init();
}

unsafe void ProcessInput () {
    if (Controls.GetKeyDown(ActionKey.ToggleDebugInfo)) {
        Debug.PrintToScreen = !Debug.PrintToScreen;
    }
}
