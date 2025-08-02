using NLog;
using SDL;

namespace battlesdk;

public static class Hud {

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static unsafe TTF_Font* _txtFont = null;
    private static unsafe TTF_Font* _shadowFont = null;

    private static unsafe SDL_Texture* _stringTex = null;
    private static unsafe SDL_Texture* _shadowTex = null;

    public static unsafe void _Init () {
        _txtFont = SDL3_ttf.TTF_OpenFont("res/fonts/power_clear.ttf", 12);
        _shadowFont = SDL3_ttf.TTF_OpenFont("res/fonts/power_clear_bold.ttf", 12);

        if (_txtFont is null || _shadowFont is null) {
            _logger.Error(
                $"Failed to open test font: {SDL3.SDL_GetError()}."
            );
        }

        SDL3_ttf.TTF_SetFontLineSkip(_txtFont, 16);
        SDL3_ttf.TTF_SetFontLineSkip(_shadowFont, 16);
    }

    public static unsafe void Draw (SDL_Renderer* renderer) {
        if (_txtFont is null || _shadowFont is null) return;

        if (_stringTex is null || _shadowTex is null) {
            _Test(renderer, "Hi! I've seen you have 6 Mewtwos! Would you trade all of them for my Rattata and $50?");
        }

        SDL_FRect dst = new() { x = 17, y = Constants.VIEWPORT_HEIGHT - 38, w = _shadowTex->w, h = _shadowTex->h };
        SDL3.SDL_RenderTexture(renderer, _shadowTex, null, &dst);

        dst = new() { x = 17, y = Constants.VIEWPORT_HEIGHT - 38, w = _stringTex->w, h = _stringTex->h };
        SDL3.SDL_RenderTexture(renderer, _stringTex, null, &dst);
    }

    private static unsafe void _Test (SDL_Renderer* renderer, string str) {
        SDL_Color black = new() { r = 0, g = 0, b = 0, a = 255 };
        SDL_Color almostBlack = new() { r = 0, g = 0, b = 0, a = 64 };

        var stringSurface = GenerateSurface(str, black);
        var shadowSurface = GenerateSurface(str, almostBlack);

        var stringTex = GenerateStringTexture(renderer, stringSurface);
        var shadowTex = GenerateShadowTexture(renderer, shadowSurface);

        _stringTex = stringTex;
        _shadowTex = shadowTex;
    }

    /// <summary>
    /// Given a string and a color, renders said string to a surface.
    /// </summary>
    /// <param name="str">The string to render.</param>
    /// <param name="color">The color to use.</param>
    private static unsafe SDL_Surface* GenerateSurface (string str, SDL_Color color) {
        var surface = SDL3_ttf.TTF_RenderText_Solid_Wrapped(
            _txtFont,
            str,
            (nuint)str.Length,
            color,
            Constants.VIEWPORT_WIDTH - 6 - 14 - 25
        );

        return surface;
    }

    /// <summary>
    /// Given a surface with rendered text, returns a texture that contains it.
    /// </summary>
    /// <param name="renderer">The renderer that will own the texture.</param>
    /// <param name="surface">The surface to use.</param>
    /// <returns></returns>
    private static unsafe SDL_Texture* GenerateStringTexture (SDL_Renderer* renderer, SDL_Surface* surface) {
        var tex = SDL3.SDL_CreateTextureFromSurface(renderer, surface);
        SDL3.SDL_SetTextureScaleMode(tex, SDL_ScaleMode.SDL_SCALEMODE_NEAREST);
        SDL3.SDL_DestroySurface(surface);

        return tex;
    }

    /// <summary>
    /// Given a surface with rendered text, returns a texture that contains an
    /// appropriate pixel shadow for it. The surface given will be destroyed.
    /// </summary>
    /// <param name="renderer">The renderer that will own the texture.</param>
    /// <param name="surface">The surface to use as reference.</param>
    /// <returns></returns>
    private static unsafe SDL_Texture* GenerateShadowTexture (
        SDL_Renderer* renderer, SDL_Surface* surface
    ) {
        // Create a raw texture. This is only done to get the format, really.
        var rawTex = SDL3.SDL_CreateTextureFromSurface(renderer, surface); // TODO: Maybe remove this step.

        // Convert the surface to the format we'll use in the streaming texture.
        var compatSurface = SDL3.SDL_ConvertSurface(surface, rawTex->format);

        // Create a texture to draw to.
        var streamingTex = SDL3.SDL_CreateTexture(
            renderer,
            compatSurface->format,
            SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
            rawTex->w,
            rawTex->h
        );

        nint pixels = 0; // The address of the pixels in the streaming texture.
        int pitch = 0; // The pitch in the texture.

        if (SDL3.SDL_LockTexture(streamingTex, null, &pixels, &pitch)) {
            byte* srcPixels = (byte*)compatSurface->pixels;
            byte* dstPixels = (byte*)pixels;

            int srcPitch = compatSurface->pitch;
            int copyWidth = Math.Min(pitch, srcPitch);

            // To compose the shadow, we iterate each pixel in the surface.
            // If said pixel, or any of its top / left neighbors is not empty,
            // then the pixel in the shadow is not empty. This creates a drop
            // shadow in the bottom-right direction.
            for (int y = 0; y < compatSurface->h; y++) {
                for (int x = 0; x < compatSurface->w; x++) {
                    byte* srcPix = srcPixels + (y * srcPitch) + (x * 4);
                    byte* dstPix = dstPixels + (y * pitch) + (x * 4);

                    byte a = srcPix[3]; // 3 should be the alpha channel.
                    // If the src pixel is not transparent, copy it directly.
                    if (a != 0) {
                        dstPix[0] = srcPix[0];
                        dstPix[1] = srcPix[1];
                        dstPix[2] = srcPix[2];
                        dstPix[3] = srcPix[3];
                    }
                    // Else, check if its top, left or top-left neighbor is not empty.
                    else {
                        byte* neighbor = null;

                        if (x > 0) {
                            byte* left = srcPixels + (y * srcPitch) + ((x - 1) * 4);
                            if (left[3] != 0) neighbor = left;
                        }
                        if (neighbor is null && y > 0) {
                            byte* top = srcPixels + ((y - 1) * srcPitch) + (x * 4);
                            if (top[3] != 0) neighbor = top;
                        }
                        if (neighbor is null && x > 0 && y > 0) {
                            byte* topLeft = srcPixels + ((y - 1) * srcPitch) + ((x - 1) * 4);
                            if (topLeft[3] != 0) neighbor = topLeft;
                        }

                        if (neighbor is not null) {
                            dstPix[0] = neighbor[0];
                            dstPix[1] = neighbor[1];
                            dstPix[2] = neighbor[2];
                            dstPix[3] = neighbor[3];
                        }
                        else {
                            dstPix[0] = 0;
                            dstPix[1] = 0;
                            dstPix[2] = 0;
                            dstPix[3] = 0;
                        }
                    }
                }
            }

            SDL3.SDL_UnlockTexture(streamingTex);
        }
        else {
            _logger.Error("Failed to copy texture to shadow's streaming texture.");

            // If something failed, we return the raw texture, which won't work
            // as a shadow but will not produce any errors later.
            SDL3.SDL_DestroySurface(surface);
            SDL3.SDL_DestroySurface(compatSurface);
            SDL3.SDL_SetTextureScaleMode(rawTex, SDL_ScaleMode.SDL_SCALEMODE_NEAREST);
            return rawTex;
        }

        SDL3.SDL_DestroyTexture(rawTex);
        SDL3.SDL_DestroySurface(surface);
        SDL3.SDL_DestroySurface(compatSurface);

        SDL3.SDL_SetTextureScaleMode(streamingTex, SDL_ScaleMode.SDL_SCALEMODE_NEAREST);
        return streamingTex;
    }
}
