using battlesdk.data;
using NLog;
using SDL;

namespace battlesdk;
public static class Music {
    /// <summary>
    /// The amount of time, in ms, that it takes to fade in or out a soundtrack.
    /// </summary>
    private const int FADE_MS = 300;

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static MusicFile? _currentFile = null;
    private static unsafe Mix_Music* _current = null;

    public static unsafe void PlayMusic (MusicFile file) {
        if (_currentFile?.Id == file.Id) return;

        if (_currentFile is not null) {
            SDL3_mixer.Mix_FadeOutMusic(FADE_MS);
            SDL3_mixer.Mix_FreeMusic(_current);
            _current = null;
            _currentFile = null;
        }

        var track = SDL3_mixer.Mix_LoadMUS(file.Path);
        if (track is null) {
            _logger.Error(
                $"Failed to load music file '{file.Path}'. " +
                $"Error: {SDL3.SDL_GetError()}"
            );
        }

        if (SDL3_mixer.Mix_FadeInMusic(track, 1, FADE_MS) == false) {
            _logger.Error(
                $"Failed to play music '{file.Name}'. Error: {SDL3.SDL_GetError()}"
            );
        }

        _currentFile = file;
        _current = track;
    }

    public static unsafe void Update () {
        // TODO: Rewrite this method properly.
        if (_current is null || _currentFile is null) return;
        var ms = SDL3_mixer.Mix_GetMusicPosition(_current) * 1000;
        if (ms < 0) return;

        double loopStart = _currentFile.LoopStart ?? 0.0f;
        double loopEnd = _currentFile.LoopEnd ?? double.PositiveInfinity;

        if (double.IsInfinity(loopEnd)) return;

        if (ms >= loopEnd) {
            int prevVolume = SDL3_mixer.Mix_VolumeMusic(-1);
            SDL3_mixer.Mix_VolumeMusic(0);

            if (SDL3_mixer.Mix_SetMusicPosition(loopStart) == false) {
                SDL3_mixer.Mix_HaltMusic();
                SDL3_mixer.Mix_PlayMusic(_current, 1);
                SDL3_mixer.Mix_SetMusicPosition(loopStart);
            }

            SDL3_mixer.Mix_VolumeMusic(prevVolume);
        }
    }
}
