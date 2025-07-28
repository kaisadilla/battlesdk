using battlesdk.world;
using Hexa.NET.SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battlesdk.graphics;
public class WorldScene {
    private World _world;

    public WorldScene (World world) {
        _world = world;
    }

    public unsafe void Draw (SDLRenderer* renderer) {

    }
}
