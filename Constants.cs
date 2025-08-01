namespace battlesdk;

public class Constants {
    /// <summary>
    /// The width of the viewport, in pixels. This is NOT the size of the window,
    /// do NOT apply the screen's scale here. 
    /// </summary>
    public const int VIEWPORT_WIDTH = 352;
    /// <summary>
    /// The height of the viewport, in pixels. This is NOT the size of the window,
    /// do NOT apply the screen's scale here. 
    /// </summary>
    public const int VIEWPORT_HEIGHT = 240;
    /// <summary>
    /// The scale of the viewport. This is the actual size of each pixel in the
    /// screen. The size of the window will be equal to the viewport's
    /// dimensions multiplied by this value.
    /// </summary>
    public const float DEFAULT_SCREEN_SCALE = 3f;
    /// <summary>
    /// The amount of visible tiles, from the center, to each horizontal side.
    /// </summary>
    public const float VISIBLE_TILES_X = (VIEWPORT_WIDTH / 2f) / TILE_SIZE;
    /// <summary>
    /// The amount of visible tiles, from the center, to each vertical side.
    /// </summary>
    public const float VISIBLE_TILES_Y = (VIEWPORT_HEIGHT / 2f) / TILE_SIZE;

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
    public const int LOAD_DISTANCE_X = ((VIEWPORT_WIDTH / TILE_SIZE) / 2) + 6;
    /// <summary>
    /// The distance from the player (in the vertical axis) at which a map is
    /// loaded and included in the world.
    /// </summary>
    public const int LOAD_DISTANCE_Y = ((VIEWPORT_HEIGHT / TILE_SIZE) / 2) + 6;
}
