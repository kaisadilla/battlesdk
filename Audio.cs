using NLog;
using SDL;

namespace battlesdk;
public static class Audio {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static readonly Dictionary<int, Ptr<Mix_Chunk>> _sounds = [];
    private static unsafe Mix_Chunk* _beepShortChunk;
    private static unsafe Mix_Chunk* _collisionChunk;
    private static unsafe Mix_Chunk* _jumpChunk;

    public static unsafe void Init () {
        if (Registry.Sounds.TryGetId("beep_short", out int id)) {
            _beepShortChunk = GetSound(id);
        }
        if (Registry.Sounds.TryGetId("collision", out id)) {
            _collisionChunk = GetSound(id);
        }
        if (Registry.Sounds.TryGetId("jump", out id)) {
            _jumpChunk = GetSound(id);
        }

    }

    public static void PlayBeepShort () {
        unsafe {
            Play(_beepShortChunk);
        }
    }

    public static void PlayCollision () {
        unsafe {
            Play(_collisionChunk);
        }
    }
    
    public static void PlayJump () {
        unsafe {
            Play(_jumpChunk);
        }
    }

    public static unsafe void Play (Mix_Chunk* sound) {
        if (sound is null) return;

        _ = SDL3_mixer.Mix_PlayChannel(0, sound, 0);
    }

    public static unsafe void Play (int soundId) {
        var sound = GetSound(soundId);
        if (sound is null) return;

        _ = SDL3_mixer.Mix_PlayChannel(0, sound, 0);
    }

    public static unsafe Mix_Chunk* GetSound (int id) {
        if (_sounds.TryGetValue(id, out var chunk)) return chunk.Raw;

        if (Registry.Sounds.TryGetElement(id, out var asset) == false) {
            _logger.Error($"Failed to find sound asset # {id}.");
            return null;
        }

        unsafe {
            chunk = new(SDL3_mixer.Mix_LoadWAV(asset.Path));
        }

        if (chunk.Raw is null) {
            _logger.Error($"Failed to load file '{asset.Path}'.");
            return null;
        }

        _sounds[asset.Id] = chunk;

        return chunk.Raw;
    }
}
