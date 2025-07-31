using battlesdk.data;
using NLog;
using SDL;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace battlesdk;
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
    /// The music memory object current playing.
    /// </summary>
    private static unsafe Mix_Music* _current = null;
    /// <summary>
    /// The music file that is currently playing.
    /// </summary>
    private static MusicFile? _currentFile = null;
    /// <summary>
    /// A lock that MUST be used every time <see cref="_current"/> or
    /// <see cref="_currentFile"/> are accessed.
    /// </summary>
    private static readonly object _musicLock = new();
    /// <summary>
    /// A resolve source for <see cref="FadeOutMusic"/>
    /// </summary>
    private static TaskCompletionSource<bool>? _fadeOutResolve = null;

    private static bool _isTransitioning = false;
    private static MusicFile? _transitionTarget = null;

    public static unsafe void Update () {
        lock (_musicLock) {
            // If no music is playing, do nothing.
            if (_current is null || _currentFile is null) return;

            var ms = SDL3_mixer.Mix_GetMusicPosition(_current) * 1000;
            double loopEnd = _currentFile.LoopEnd ?? double.PositiveInfinity;

            // When the position in the track is beyond the end of the loop.
            if (ms >= loopEnd) {
                _logger.Debug("Music looped.");

                // Move the position back to the start of the loop.
                double loopStart = _currentFile.LoopStart ?? 0.0f;
                SDL3_mixer.Mix_SetMusicPosition(loopStart / 1000d);
            }
        }
    }

    /// <summary>
    /// Plays the music file given by fading it normally. If there's music
    /// already playing, that music will be stopped.
    /// </summary>
    /// <param name="file"></param>
    public static async Task FadeInMusic (MusicFile file, bool fadeOutCurrent) {
        // TODO: This still may mix up tracks.
        lock (_musicLock) {
            // If the music currently playing is the one provided, do nothing.
            if (_transitionTarget is not null && _currentFile?.Id == _transitionTarget?.Id) {
                return;
            }

            _transitionTarget = file;

            if (_isTransitioning) return;
            _isTransitioning = true;
        }

        Task? fadeOutTask = null;

        lock (_musicLock) {
            if (_currentFile is not null) {
                // These methods clean up the music.
                if (fadeOutCurrent) {
                    fadeOutTask = FadeOutMusic();
                }
                else {
                    StopMusic();
                }
            }
        }

        if (fadeOutTask is not null) await fadeOutTask;

        lock (_musicLock) {
            var target = _transitionTarget;
            _EndTransition();

            if (target is null) return;

            unsafe {
                var track = SDL3_mixer.Mix_LoadMUS(target.Path);
                if (track is null) {
                    _logger.Error(
                        $"Failed to load music file '{target.Path}'. " +
                        $"Error: {SDL3.SDL_GetError()}"
                    );
                    return;
                }

                if (SDL3_mixer.Mix_FadeInMusic(track, 1, FADE_IN_MS) == false) {
                    _logger.Error(
                        $"Failed to play music '{target.Name}'. Error: {SDL3.SDL_GetError()}"
                    );
                    SDL3_mixer.Mix_FreeMusic(track);
                    return;
                }

                // We only set these if nothing failed, since the way we check
                // when music is currently playing is by checking whether these
                // are null or not.
                _current = track;
                _currentFile = file;
            }
        }

        // NOTE: Only call when music is locked.
        void _EndTransition () {
            _isTransitioning = false;
            _transitionTarget = null;
        }
    }

    /// <summary>
    /// Stops the music currently playing by fading it out normally. Returns a
    /// task that completes when the music has completely faded out.
    /// </summary>
    public static async Task FadeOutMusic () {
        lock (_musicLock) {
            if (_currentFile is null) return;

            unsafe {
                // This will resolve _fadeOutResolve when the fade out ends.
                SDL3_mixer.Mix_HookMusicFinished(&_OnMusicFinished);

                // Try to fade out the music. If the action fails, clean up
                // immediately and return.
                if (SDL3_mixer.Mix_FadeOutMusic(FADE_OUT_MS) == false) {
                    _logger.Warn("Failed to fade out music.");
                    _CleanUp();
                    return;
                }
            }

            _fadeOutResolve = new();
        }

        await _fadeOutResolve.Task;
        _fadeOutResolve = null;

        lock (_musicLock) {
            _CleanUp();
        }

        unsafe void _CleanUp () {
            SDL3_mixer.Mix_HookMusicFinished(null);
            SDL3_mixer.Mix_FreeMusic(_current);
            _current = null;
            _currentFile = null;
        }
    }

    /// <summary>
    /// Stops the music currently playing instantly.
    /// </summary>
    public static void StopMusic () {
        lock (_musicLock) {
            unsafe {
                SDL3_mixer.Mix_HaltMusic();
                SDL3_mixer.Mix_FreeMusic(_current);
                _current = null;
                _currentFile = null;
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
