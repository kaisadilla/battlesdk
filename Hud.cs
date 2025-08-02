using NLog;
using SDL;

namespace battlesdk;

public static class Hud {

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static unsafe TTF_Font* _txtFont = null;

    public static unsafe void _Init () {
        _txtFont = SDL3_ttf.TTF_OpenFont("res/fonts/power_clear.ttf", 12);
        if (_txtFont is null) {
            _logger.Error(
                $"Failed to open test font 'power_clear.ttf': {SDL3.SDL_GetError()}."
            );
        }


    }

    public static unsafe void Draw (SDL_Renderer* renderer) {
        SDL_Color black = new() { r = 0, g = 0, b = 0, a = 255 };
        SDL_Color white = new() { r = 255, g = 255, b = 255, a = 255 };

        var surface = SDL3_ttf.TTF_RenderText_Solid(_txtFont, "This is a Pokémon.", (nuint)"This is a Pokémon.".Length, black);

        //SDL_FRect rect = new() { x = 40, y = 190, w = 300 - 40, h = 240 - 190 };
        //SDL3.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
        //SDL3.SDL_RenderFillRect(renderer, &rect);

        var tex = SDL3.SDL_CreateTextureFromSurface(renderer, surface);
        SDL3.SDL_SetTextureScaleMode(tex, SDL_ScaleMode.SDL_SCALEMODE_NEAREST);

        float w, h;
        SDL3.SDL_GetTextureSize(tex, &w, &h);

        SDL_FRect dst = new() { x = 50, y = 200, w = w, h = h };
        SDL3.SDL_RenderTexture(renderer, tex, null, &dst);
    }
}
