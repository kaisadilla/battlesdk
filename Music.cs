using battlesdk.data;
using NLog;
using SDL;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace battlesdk;

// TODO: This class's async code is poorly implemented.
public static class Music {
    /// <summary>
    /// The amount of time, in ms, that it takes to fade in a soundtrack.
    /// </summary>
    private const int FADE_IN_MS = 500;
    /// <summary>
    /// The amount of time, in ms, that it takes to fade out a soundtrack.
    /// </summary>
    private const int FADE_OUT_MS = 5000;

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The track that is currently playing.
    /// </summary>
    private static MusicFile? _currentTrack = null;
    /// <summary>
    /// The SDL music object current playing.
    /// </summary>
    private static unsafe Mix_Music* _currentObj = null;
    /// <summary>
    /// A resolve source for <see cref="FadeOutMusic"/>
    /// </summary>
    private static TaskCompletionSource<bool>? _fadeOutResolve = null;
    /// <summary>
    /// A lock that MUST be used every time <see cref="_currentObj"/> or
    /// <see cref="_currentTrack"/> are accessed.
    /// </summary>
    private static readonly object _musicLock = new();

    public static bool IsFadingOut { get; private set; } = false;

    public static unsafe void Update () {
        lock (_musicLock) {
            // Music does not loop during fade out.
            if (IsFadingOut) return;

            // If no music is playing, do nothing.
            if (_currentObj is null || _currentTrack is null) return;

            var ms = SDL3_mixer.Mix_GetMusicPosition(_currentObj) * 1000;
            double loopEnd = _currentTrack.LoopEnd ?? double.PositiveInfinity;

            // When the position in the track is beyond the end of the loop.
            if (ms >= loopEnd) {
                _logger.Debug("Music looped.");

                // Move the position back to the start of the loop.
                double loopStart = _currentTrack.LoopStart ?? 0.0f;
                SDL3_mixer.Mix_SetMusicPosition(loopStart / 1000d);
            }
        }
    }

    /// <summary>
    /// Returns the id of the track this is currently playing, or -1 if no
    /// track is playing.
    /// </summary>
    public static int GetTrackId () {
        lock (_musicLock) {
            return _currentTrack?.Id ?? -1;
        }
    }

    /// <summary>
    /// Plays the music file given by fading it normally. If there's music
    /// already playing, that music will be stopped abruptly.
    /// </summary>
    /// <param name="file"></param>
    public static void FadeIn (MusicFile file) {
        lock (_musicLock) {
            unsafe {
                if (_currentObj is not null) {
                    SDL3_mixer.Mix_HaltMusic();
                    SDL3_mixer.Mix_HookMusicFinished(null);
                    SDL3_mixer.Mix_FreeMusic(_currentObj);
                    _currentObj = null;
                    _currentTrack = null;
                }

                var track = SDL3_mixer.Mix_LoadMUS(file.Path);
                if (track is null) {
                    _logger.Error(
                        $"Failed to load music file '{file.Path}'. " +
                        $"Error: {SDL3.SDL_GetError()}"
                    );
                    return;
                }

                if (SDL3_mixer.Mix_FadeInMusic(track, 1, FADE_IN_MS) == false) {
                    _logger.Error(
                        $"Failed to play music '{file.Name}'. Error: {SDL3.SDL_GetError()}"
                    );
                    SDL3_mixer.Mix_FreeMusic(track);
                    return;
                }

                // We only set these if nothing failed, since the way we check
                // when music is currently playing is by checking whether these
                // are null or not.
                _currentObj = track;
                _currentTrack = file;
            }
        }
    }

    /// <summary>
    /// Stops the music currently playing by fading it out normally. Returns a
    /// task that completes when the music has completely faded out.
    /// </summary>
    public static async Task FadeOutMusic () {
        lock (_musicLock) {
            if (IsFadingOut) return;
            if (_currentTrack is null) return;

            IsFadingOut = true;

            unsafe {
                // This will resolve _fadeOutResolve when the fade out ends.
                SDL3_mixer.Mix_HookMusicFinished(&_OnMusicFinished);

                // Try to fade out the music. If the action fails, clean up
                // immediately and return.
                if (SDL3_mixer.Mix_FadeOutMusic(FADE_OUT_MS) == false) {
                    _logger.Warn("Failed to fade out music.");
                    SDL3_mixer.Mix_HaltMusic();
                    SDL3_mixer.Mix_HookMusicFinished(null);
                    SDL3_mixer.Mix_FreeMusic(_currentObj);
                    _currentObj = null;
                    _currentTrack = null;
                    return;
                }

                _fadeOutResolve = new();
            }
        }

        await _fadeOutResolve.Task;

        lock (_musicLock) {
            _fadeOutResolve = null;
            unsafe {
                SDL3_mixer.Mix_FreeMusic(_currentObj);
                _currentTrack = null;
                _currentObj = null;
            }
            IsFadingOut = false;
        }
    }

    /// <summary>
    /// Stops the music currently playing instantly.
    /// </summary>
    public static void StopMusic () {
        lock (_musicLock) {
            unsafe {
                SDL3_mixer.Mix_HaltMusic();
                SDL3_mixer.Mix_FreeMusic(_currentObj);
                _currentObj = null;
                _currentTrack = null;
            }
        }
    }

    /// <summary>
    /// Sets the fade out task resolver to true. This is intended to be used
    /// as a callback in SDL.
    /// </summary>
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe void _OnMusicFinished () {
        _fadeOutResolve?.SetResult(true);
    }
}
