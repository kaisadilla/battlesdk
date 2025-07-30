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
    /// <summary>
    /// The amount of time, in seconds, that it takes for a normal character
    /// (including the player) to complete a ledge jump.
    /// </summary>
    public const float LEDGE_JUMP_SPEED = 32f / 60f;

    /// <summary>
    /// The distance from the player (in the horizontal axis) at which a map
    /// is loaded and included in the world.
    /// </summary>
    public const int LOAD_DISTANCE_X = 32;
    /// <summary>
    /// The distance from the player (in the vertical axis) at which a map is
    /// loaded and included in the world.
    /// </summary>
    public const int LOAD_DISTANCE_Y = 24;
}
