using NLog;
using System.Diagnostics.CodeAnalysis;
using Tomlyn;

namespace battlesdk;

public static class Settings {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The width of the viewport, in pixels. This is NOT the size of the window,
    /// do NOT apply the screen's scale here. 
    /// </summary>
    public static int ViewportWidth { get; private set; } = 352;
    /// <summary>
    /// The height of the viewport, in pixels. This is NOT the size of the window,
    /// do NOT apply the screen's scale here. 
    /// </summary>
    public static int ViewportHeight { get; private set; } = 240;
    /// <summary>
    /// The scale of the viewport. This is the actual size of each pixel in the
    /// screen. The size of the window will be equal to the viewport's
    /// dimensions multiplied by this value.
    /// </summary>
    public static float DefaultRendererScale { get; private set; } = 3f;

    /// <summary>
    /// The amount of visible tiles, from the center, to each horizontal side.
    /// </summary>
    public static float VisibleTilesX => (ViewportWidth / 2f) / TileSize;
    /// <summary>
    /// The amount of visible tiles, from the center, to each vertical side.
    /// </summary>
    public static float VisibleTilesY => (ViewportHeight / 2f) / TileSize;

    /// <summary>
    /// The distance from the player (in the horizontal axis) at which a map
    /// is loaded and included in the world.
    /// </summary>
    public static int LoadDistanceX => ((ViewportWidth / TileSize) / 2) + 6;
    /// <summary>
    /// The distance from the player (in the vertical axis) at which a map is
    /// loaded and included in the world.
    /// </summary>
    public static int LoadDistanceY => ((ViewportHeight / TileSize) / 2) + 6;

    /// <summary>
    /// The size, in pixels, of a tile in a tileset.
    /// </summary>
    public static int TileSize { get; private set; } = 16;

    /// <summary>
    /// The amount of time, in seconds, that it takes for a normal character
    /// (including the player) to move one tile when walking.
    /// </summary>
    public static float WalkSpeed { get; private set; } = 60f / 18f; // TODO: Rename these as "speed" is not what this is.
    /// <summary>
    /// The amount of time, in seconds, that it takes for a normal character
    /// (including the player) to move one tile when running.
    /// </summary>
    public static float RunSpeed { get; private set; } = 60f / 9f;
    /// <summary>
    /// The amount of time, in seconds, that it takes for a normal character
    /// (including the player) to complete a ledge jump.
    /// </summary>
    public static float LedgeJumpSpeed { get; private set; } = 60f / 32f;

    /// <summary>
    /// The minimum amount of money the player may have. This value can be negative.
    /// </summary>
    public static int MinMoney { get; private set; } = 0;

    /// <summary>
    /// The maximum amount of money the player may have.
    /// </summary>
    public static int MaxMoney { get; private set; } = 999_999_999;

    /// <summary>
    /// The normal color of text.
    /// </summary>
    public static ColorRGBA DefaultTextColor { get; private set; } = new(85, 85, 93, 255);
    /// <summary>
    /// The normal color of text's shadow.
    /// </summary>
    public static ColorRGBA DefaultTextShadowColor { get; private set; } = new(0, 0, 0, 48);

    public static void Load (string path) {
        if (File.Exists(path) == false) {
            _logger.Warn(
                $"No settings file found at '{path}'. Default settings will be used."
            );
            return;
        }

        var txt = File.ReadAllText(path);
        var toml = Toml.ToModel(txt).Flatten();

        // [window]
        if (toml.TryGetFloat("window.renderer_scale", out float rendererScale)) {
            DefaultRendererScale = rendererScale;
        }
        if (toml.TryGetInt("window.viewport.width", out int viewportWidth)) {
            ViewportWidth = viewportWidth;
        }
        if (toml.TryGetInt("window.viewport.height", out int viewportHeight)) {
            ViewportHeight = viewportHeight;
        }

        // [characters]
        if (toml.TryGetFloat("characters.speed.walk", out float walkSpeed)) {
            WalkSpeed = walkSpeed;
        }
        if (toml.TryGetFloat("characters.speed.run", out float runSpeed)) {
            RunSpeed = runSpeed;
        }
        if (toml.TryGetFloat("characters.speed.ledge_jump", out float ledgeJumpSpeed)) {
            LedgeJumpSpeed = ledgeJumpSpeed;
        }

        // [player]
        if (toml.TryGetInt("player.money.min", out int minMoney)) {
            MinMoney = minMoney;
        }
        if (toml.TryGetInt("player.money.max", out int maxMoney)) {
            MaxMoney = maxMoney;
        }

        // [map]
        if (toml.TryGetInt("map.tile_size", out int tileSize)) {
            TileSize = tileSize;
        }
    }

    private static bool TryGetAs<T> (
        this Dictionary<string, object> toml, string key, [NotNullWhen(true)] out T? val
    ) {
        if (toml.TryGetValue(key, out object? valObj) == false) {
            _logger.Warn($"Missing key: '{key}'");
            val = default;
            return false;
        }
        if (valObj is not T t) {
            _logger.Warn(
                $"Key '{key}' has an invalid type: {valObj.GetType().Name}. " +
                $"Expected type is {typeof(T).Name}."
            );
            val = default;
            return false;
        }
    
        val = t;
        return true;
    }
    
    private static bool TryGetInt (
        this Dictionary<string, object> toml, string key, [NotNullWhen(true)] out int val
    ) {
        if (toml.TryGetValue(key, out object? valObj) == false) {
            _logger.Warn($"Missing key: '{key}'");
            val = default;
            return false;
        }

        if (valObj is long l) {
            val = (int)l;
            return true;
        }

        _logger.Warn(
            $"Key '{key}' has an invalid type: {valObj.GetType().Name}. " +
            $"Expected type is int."
        );
        val = default;
        return false;
    }
    
    private static bool TryGetFloat (
        this Dictionary<string, object> toml, string key, [NotNullWhen(true)] out float val
    ) {
        if (toml.TryGetValue(key, out object? valObj) == false) {
            _logger.Warn($"Missing key: '{key}'");
            val = default;
            return false;
        }

        if (valObj is double dbl) {
            val = (float)dbl;
            return true;
        }
        if (valObj is long l) {
            val = l;
            return true;
        }

        _logger.Warn(
            $"Key '{key}' has an invalid type: {valObj.GetType().Name}. " +
            $"Expected type is float."
        );
        val = default;
        return false;
    }

}
