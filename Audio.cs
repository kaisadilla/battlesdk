//using SDL_Sharp.Mixer;

namespace battlesdk;
public static class Audio {
    private static readonly Dictionary<string, nint> _sounds = [];

    public static void PlaySound (string name) {
        //if (_sounds.TryGetValue(name, out var chunk) == false) return;
        //
        //unsafe {
        //    MIX.PlayChannel(-1, (Chunk*)chunk, 0);
        //}
    }

    public static unsafe void RegisterSounds () {
        //foreach (var s in Registry.Sounds) {
        //    Chunk* chunk = MIX.LoadWAV(s.Path);
        //
        //    if (chunk is not null) {
        //        _sounds[s.Name] = (nint)chunk;
        //    }
        //}
    }
}
