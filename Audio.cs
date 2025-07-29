using NLog;
using SDL;

namespace battlesdk;
public static class Audio {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static readonly Dictionary<string, nint> _sounds = [];

    public static void PlaySound (string name) {
        if (_sounds.TryGetValue(name, out var chunk) == false) return;

        unsafe {
            SDL3_mixer.Mix_PlayChannel(-1, (Mix_Chunk*)chunk, 0);
        }
    }

    public static unsafe void Init () {
        SDL_AudioSpec spec = new();

        SDL3_mixer.Mix_Init(SDL3_mixer.MIX_INIT_WAVPACK);
        SDL3_mixer.Mix_OpenAudio(0, &spec);
    }

    public static unsafe void RegisterSounds () {
        //SDL3_mixer.Mix_OpenAudio(44100, SDL.)

        foreach (var s in Registry.Sounds) {
            Mix_Chunk* chunk = SDL3_mixer.Mix_LoadWAV(s.Path);

            if (chunk is not null) {
                _sounds[s.Name] = (nint)chunk;
            }
            else {
                var error = SDL3.SDL_GetError();
                _logger.Error($"Failed to load sound '{s.Path}': {error}.");
            }
        }
    }
}
