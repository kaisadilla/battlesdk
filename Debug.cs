using NLog;
using SDL;

namespace battlesdk;
public static class Debug {
    /// <summary>
    /// The size of the font used in debug panels, in pixels.
    /// </summary>
    private const int FONT_SIZE = 12;
    /// <summary>
    /// The size of margins, in pixels.
    /// </summary>
    private const int MARGIN = 10;
    /// <summary>
    /// The gap between lines of text, in pixels.
    /// </summary>
    private const int GAP = 18;

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static unsafe TTF_Font* _font = null;

    // To minimize the amount of textures created by text, the texture for each
    // piece of information is cached and rebuilt when that information changes.

    /// <summary>
    /// Maps each debug datum to the last string it produced.
    /// </summary>
    private static readonly Dictionary<string, string> _texStrings = [];
    /// <summary>
    /// Maps each debug datum to the texture containing the string it produced.
    /// </summary>
    private static readonly Dictionary<string, nint> _texes = [];

    public static bool PrintToScreen { get; set; } = false;
    public static FpsCounter? FpsCounter { get; private set; } = null;

    public static unsafe void Init () {
        _font = SDL3_ttf.TTF_OpenFont("res/fonts/cascadia_mono.ttf", FONT_SIZE);
        if (_font is null) {
            _logger.Error(
                $"Failed to open debug font 'cascadia_mono.ttf': {SDL3.SDL_GetError()}."
            );
        }
        else {
            _logger.Info("Loaded debug panel font.");
        }

        FpsCounter = new();
        FpsCounter.SetUpdateTime(0.1f);
    }

    public static void OnFrameStart () {
        FpsCounter?.Count();
    }

    public static unsafe void Draw (SDL_Renderer* renderer) {
        if (PrintToScreen == false) return;
        if (_font is null) return;

        SDL_FRect squarePos = new() { x = MARGIN, y = MARGIN, w = 180, h = 120, };

        // Draw the square that contains the data.
        SDL3.SDL_SetRenderDrawBlendMode(renderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);
        SDL3.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0x80);
        SDL3.SDL_RenderFillRect(renderer, &squarePos);

        int leftMargin = MARGIN * 2;
        int topMargin = MARGIN * 2;

        DrawText(
            renderer,
            "fps",
            $"FPS: {FpsCounter?.Fps}",
            leftMargin,
            topMargin
        );
        DrawText(
            renderer,
            "player_position",
            $"Pos: {G.World.Player.Position}",
            leftMargin,
            topMargin + (GAP * 1)
        );
        DrawText(
            renderer,
            "player_z",
            $"Z: {G.World.Player.Z}",
            leftMargin,
            topMargin + (GAP * 2)
        );
        if (G.World.TryGetMapAt(G.World.Player.Position, out var map)) {
                DrawText(
                renderer,
                "map",
                $"Map: {map.Data.Name}",
                leftMargin,
                topMargin + (GAP * 3)
            );
        }
        string loadedMapsStr = $"Loaded maps: {G.World.Maps.Count}";
        DrawText(
            renderer,
            "loaded_maps",
            $"Loaded maps: {G.World.Maps.Count}",
            leftMargin,
            topMargin + (GAP * 4)
        );
        string loadedNpcsStr = $"Loaded NPCs: {G.World.Npcs.Count()}";
        DrawText(
            renderer,
            "loaded_npcs",
            loadedNpcsStr,
            leftMargin,
            topMargin + (GAP * 5)
        );
    }

    /// <summary>
    /// Draws the string given to the screen, at the position given.
    /// </summary>
    /// <param name="renderer">The renderer to draw to.</param>
    /// <param name="key">A unique key for this text, used for caching.</param>
    /// <param name="str">The string to draw.</param>
    /// <param name="x">The x position in the screen.</param>
    /// <param name="y">The y position in the screen.</param>
    private static unsafe void DrawText (
        SDL_Renderer* renderer, string key, string str, int x, int y
    ) {
        if (PrintToScreen == false) return;
        if (_font is null) return;

        SDL_Texture* tex = null;

        // Check if a texture for the key given is cached.
        if (_texStrings.TryGetValue(key, out var cachedStr)) {
            // If it is, check that the cached string is the same as the current one.
            if (cachedStr == str) {
                // If it's the same value, we can reuse the texture.
                tex = (SDL_Texture*)_texes[key];
            }
            else {
                // Else, we delete the texture.
                SDL3.SDL_DestroyTexture((SDL_Texture*)_texes[key]);
            }
        }

        // If no fitting cached texture exists, create a new one:
        if (tex is null) {
            SDL_Color white = new() { r = 255, g = 255, b = 255, a = 255, };

            // Render the text.
            var surface = SDL3_ttf.TTF_RenderText_Blended(
                _font, str, (nuint)str.Length, white
            );
            if (surface is null) {
                _logger.Error("Failed to create surface for text: " + SDL3.SDL_GetError());
                return;
            }

            // Create a texture in the renderer with the text.
            tex = SDL3.SDL_CreateTextureFromSurface(renderer, surface);
            if (tex is null) {
                _logger.Error("Failed to create texture for text: " + SDL3.SDL_GetError());
                return;
            }

            // Cache the string and its texture.
            _texStrings[key] = str;
            _texes[key] = (nint)tex;

            // Free the surface.
            SDL3.SDL_DestroySurface(surface);
        }

        // Get the texture's dimensions.
        float w, h;
        SDL3.SDL_GetTextureSize(tex, &w, &h);

        // Draw the texture to the screen.
        SDL_FRect dst = new() { x = x, y = y, w = w, h = h };
        SDL3.SDL_RenderTexture(renderer, tex, null, &dst);
    }
}
