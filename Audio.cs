using SDL_Sharp.Mixer;

namespace battlesdk;
public static class Audio {
    public static void PlayCollision () {
        MIX.OpenAudio(44100, MIX.DEFAULT_FORMAT, 2, 2048);

        unsafe {
            Chunk* sfx = MIX.LoadWAV("res/sounds/collision.wav");

            if (sfx is not null) {
                MIX.PlayChannel(-1, sfx, 0);
            }
        }
    }
}
