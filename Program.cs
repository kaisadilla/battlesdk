global using battlesdk.types;
global using static battlesdk.types.TypesUtils;

using battlesdk;
using NLog;
using SDL;

const int TARGET_FPS = 60;

Logger _logger = LogManager.GetCurrentClassLogger();

_logger.Info("Launching BattleSDK.");

InitSdl();
Registry.BuildRegistry();
G.LoadGame();
Debug.Init();

Audio.RegisterSounds();

var win = new battlesdk.graphics.Window(
    Constants.VIEWPORT_WIDTH,
    Constants.VIEWPORT_HEIGHT,
    Constants.DEFAULT_SCREEN_SCALE
);

while (win.CloseRequested == false) {
    var frameStart = SDL3.SDL_GetTicks();

    win.ProcessEvents();
    Debug.OnFrameStart();
    G.World.OnFrameStart();

    ProcessInput();

    G.World.Update();

    win.Render();

    var frameTime = (int)(SDL3.SDL_GetTicks() - frameStart);
    var delay = 1000 / TARGET_FPS;

    if (frameTime < delay) {
        SDL3.SDL_Delay((uint)(delay - frameTime));
    }
}

win.Destroy();

unsafe void InitSdl () {
    if (SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO)) {
        _logger.Info("SDL initialized.");
    }
    else {
        _logger.Fatal($"Failed to initialize SDL: {SDL3.SDL_GetError()}.");
        SDL3.SDL_Quit();
    }

    if (SDL3_ttf.TTF_Init()) {
        _logger.Info("SDL_ttf initialized.");
    }
    else {
        _logger.Fatal($"Failed to initialize SDL_ttf: {SDL3.SDL_GetError()}.");
        SDL3.SDL_Quit();
    }

    SDL_AudioSpec spec = new();

    SDL3_mixer.Mix_Init(SDL3_mixer.MIX_INIT_WAVPACK);

    if (SDL3_mixer.Mix_OpenAudio(0, &spec)) {
        _logger.Info("Audio device opened.");
    }
    else {
        _logger.Fatal($"Failed to open audio device: {SDL3.SDL_GetError()}.");
        SDL3.SDL_Quit();
    }
}

unsafe void ProcessInput () {
    if (Controls.GetKeyDown(ActionKey.ToggleDebugInfo)) {
        Debug.PrintToScreen = !Debug.PrintToScreen;
    }
}