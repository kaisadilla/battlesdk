using battlesdk.world;
using SDL;

namespace battlesdk.graphics;
public class WorldScene {
    private World _world;

    public WorldScene (World world) {
        _world = world;
    }

    public unsafe void Draw (SDL_Renderer* renderer) {

    }
}
