using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battlesdk;

public class Constants {
    /// <summary>
    /// The default size of each sprite in a overworld character spritesheet.
    /// </summary>
    public const int DEFAULT_CHAR_SIZE = 32;
    /// <summary>
    /// The size, in pixels, of a tile in a tileset.
    /// </summary>
    public const int TILE_SIZE = 16;

    /// <summary>
    /// The amount of time, in seconds, that it takes for a normal character
    /// (including the player) to move one tile when walking.
    /// </summary>
    public const float WALK_SPEED = 18f / 60f;
    /// <summary>
    /// The amount of time, in seconds, that it takes for a normal character
    /// (including the player) to move one tile when running.
    /// </summary>
    public const float RUN_SPEED = 9f / 60f;
}
