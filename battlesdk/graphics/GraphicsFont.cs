using battlesdk.data;
using NLog;
using SDL;
using System.Text;

namespace battlesdk.graphics;

public class GraphicsFont {
    private static readonly SDL_Color WHITE = SdlColor(255, 255, 255, 255);

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private Renderer _renderer;

    public unsafe TTF_Font* Font { get; private set; }
    public FontAsset Asset { get; private set; }

    /// <summary>
    /// The atlas that contains the characters.
    /// </summary>
    public unsafe SDL_Texture* CharAtlas { get; private set; }
    /// <summary>
    /// The atlas that contains the drop shadows.
    /// </summary>
    public unsafe SDL_Texture* ShadowAtlas { get; private set; }
    /// <summary>
    /// Maps each character to its position in the atlases.
    /// </summary>
    private Dictionary<uint, SDL_FRect> _charMap = new();
    /// <summary>
    /// The current width of the atlas.
    /// </summary>
    private int _atlasWidth = 1024;
    /// <summary>
    /// The current height of the atlas.
    /// </summary>
    private int _atlasHeight = 1024;
    /// <summary>
    /// The x position of the next character in the atlas.
    /// </summary>
    private int _cursorX = 0;
    /// <summary>
    /// The y position of the next character in the atlas.
    /// </summary>
    private int _cursorY = 0;

    public unsafe GraphicsFont (Renderer renderer, FontAsset asset) {
        _renderer = renderer;
        Asset = asset;

        Font = SDL3_ttf.TTF_OpenFont(asset.Path, asset.Size);
        if (Font is null) {
            throw new Exception($"Failed to read font at '{asset.Path}'.");
        }

        SDL3_ttf.TTF_SetFontLineSkip(Font, asset.LineHeight);

        CharAtlas = SDL3.SDL_CreateTexture(
            _renderer.SdlRenderer,
            SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888, // TODO: Maybe do not hardcode this.
            SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET,
            _atlasWidth,
            _atlasHeight
        );

        if (CharAtlas is null) {
            throw new Exception("Failed to create character atlas.");
        }

        SDL3.SDL_SetTextureBlendMode(CharAtlas, SDL_BlendMode.SDL_BLENDMODE_BLEND);

        ShadowAtlas = SDL3.SDL_CreateTexture(
            _renderer.SdlRenderer,
            SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888, // TODO: Maybe do not hardcode this.
            SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET,
            _atlasWidth,
            _atlasHeight
        );

        if (ShadowAtlas is null) {
            throw new Exception("Failed to create shadow atlas.");
        }

        SDL3.SDL_SetTextureBlendMode(ShadowAtlas, SDL_BlendMode.SDL_BLENDMODE_BLEND);
    }

    /// <summary>
    /// Returns the position of the char given in the atlas. If the character
    /// is not yet in the atlas, adds it.
    /// </summary>
    /// <param name="ch">The character to find in the atlas.</param>
    /// <exception cref="Exception"></exception>
    public unsafe SDL_FRect GetChar (uint ch) {
        // If the character is already registered, simply return its position.
        if (_charMap.TryGetValue(ch, out var rect)) {
            return rect;
        }

        // Note: This logic is designed so that the shadow texture is always
        // as big, or bigger, as the character texture in both dimensions.
        // Both the character and the shadow texture are drawn in the exact same
        // position in their atlases, so the rect of the shadow texture can
        // safely be used to locate both character and shadow.

        SDL_Surface* surface = CreateSurface(ch, WHITE);
        if (surface is null) {
            throw new Exception($"Failed to render glyph for '{ch}'.");
        }
        SDL_Texture* charTex = CreateCharTexture(surface);
        SDL_Texture* shadowTex = CreateShadowTexture(surface);
        SDL3.SDL_DestroySurface(surface);

        // If the char doesn't fit in the current row, move to the next one.
        if (_cursorX + charTex->w > _atlasWidth) {
            _cursorX = 0;
            _cursorY += Asset.LineHeight;
        }

        // If the atlas is already full (and thus, the next line can't fit into it).
        if (_cursorY + charTex-> h > _atlasHeight) {
            // TODO: Test.
            int newHeight = _atlasHeight * 2;
            CharAtlas = GrowAtlas(CharAtlas, newHeight);
            ShadowAtlas = GrowAtlas(ShadowAtlas, newHeight);
            _atlasHeight = newHeight;
        }

        // Draw the character texture to the char atlas.
        SDL3.SDL_SetRenderTarget(_renderer.SdlRenderer, CharAtlas);
        SDL_FRect charDst = new() {
            x = _cursorX,
            y = _cursorY,
            w = charTex->w,
            h = charTex->h,
        };
        SDL3.SDL_RenderTexture(_renderer.SdlRenderer, charTex, null, &charDst);
        SDL3.SDL_SetRenderTarget(_renderer.SdlRenderer, null);

        // Draw the shadow texture to the shadow atlas.
        SDL3.SDL_SetRenderTarget(_renderer.SdlRenderer, ShadowAtlas);
        SDL_FRect shadowDst = new() {
            x = _cursorX,
            y = _cursorY,
            w = shadowTex->w,
            h = shadowTex->h,
        };
        SDL3.SDL_RenderTexture(_renderer.SdlRenderer, shadowTex, null, &shadowDst);
        SDL3.SDL_SetRenderTarget(_renderer.SdlRenderer, null);

        SDL3.SDL_DestroyTexture(shadowTex);

        // Register it in the map and move the cursor.
        _charMap[ch] = shadowDst;
        _cursorX += charTex->w;

        return shadowDst;
    }

    public void DrawChar (IVec2 pos, uint ch, SDL_Color color, bool shadow) {
        DrawChar(pos, GetChar(ch), color, shadow);
    }

    /// <summary>
    /// Draws the character given to the screen. The character is described by
    /// its rect in the atlas.
    /// </summary>
    /// <param name="pos">The position in the screen to draw at.</param>
    /// <param name="ch">The rect containing the character in the atlas.</param>
    /// <param name="color">The character's color.</param>
    /// <param name="shadow">True to draw the character's shadow.</param>
    public unsafe void DrawChar (
        IVec2 pos, SDL_FRect ch, SDL_Color color, bool shadow
    ) {
        SDL_FRect dst = new() {
            x = pos.X,
            y = pos.Y,
            w = ch.w,
            h = ch.h,
        };

        if (shadow) {
            SDL3.SDL_SetTextureColorMod(ShadowAtlas, 0, 0, 0);
            SDL3.SDL_SetTextureAlphaMod(ShadowAtlas, Constants.TEXT_SHADOW_ALPHA);
            SDL3.SDL_RenderTexture(_renderer.SdlRenderer, ShadowAtlas, &ch, &dst);
        }

        SDL3.SDL_SetTextureColorMod(CharAtlas, color.r, color.g, color.b);
        SDL3.SDL_SetTextureAlphaMod(ShadowAtlas, color.a);
        SDL3.SDL_RenderTexture(_renderer.SdlRenderer, CharAtlas, &ch, &dst);
    }

    public unsafe void DrawCharInViewport (
        IVec2 pos, IRect viewport, SDL_FRect ch, SDL_Color color, bool shadow
    ) {
        float drawLeft = Math.Max(pos.X, viewport.Left);
        float drawRight = Math.Min(pos.X + ch.w, viewport.Right);
        float drawTop = Math.Max(pos.Y, viewport.Top);
        float drawBottom = Math.Min(pos.Y + ch.h, viewport.Bottom);

        // The width and height of the part of the texture that is visible.
        float drawWidth = drawRight - drawLeft;
        float drawHeight = drawBottom - drawTop;

        // This character is not visible in the viewport given.
        if (drawWidth <= 0 || drawHeight <= 0) return;

        // How far the viewport is from the texture.
        float srcOffsetX = drawLeft - pos.X;
        float srcOffsetY = drawTop - pos.Y;

        SDL_FRect src = new() {
            x = ch.x + srcOffsetX,
            y = ch.y + srcOffsetY,
            w = drawWidth,
            h = drawHeight,
        };

        SDL_FRect dst = new() {
            x = drawLeft,
            y = drawTop,
            w = drawWidth,
            h = drawHeight,
        };

        if (shadow) {
            SDL3.SDL_SetTextureColorMod(ShadowAtlas, 0, 0, 0);
            SDL3.SDL_SetTextureAlphaMod(ShadowAtlas, Constants.TEXT_SHADOW_ALPHA);
            SDL3.SDL_RenderTexture(_renderer.SdlRenderer, ShadowAtlas, &src, &dst);
        }

        SDL3.SDL_SetTextureColorMod(CharAtlas, color.r, color.g, color.b);
        SDL3.SDL_SetTextureAlphaMod(ShadowAtlas, color.a);
        SDL3.SDL_RenderTexture(_renderer.SdlRenderer, CharAtlas, &src, &dst);

    }

    /// <summary>
    /// Frees all resources used by this font, nulling its fields. After this
    /// is called, using this object will result in undefined behavior.
    /// </summary>
    public unsafe void Destroy () {
        SDL3_ttf.TTF_CloseFont(Font);
        SDL3.SDL_DestroyTexture(CharAtlas);
        Font = null;
        CharAtlas = null;
    }

    /// <summary>
    /// Creates a sprite containing a string of plain text. This sprite does
    /// not use this font's sprite atlas and can't include advanced features
    /// like multiple colors or fonts.
    /// </summary>
    /// <param name="str">A string of plain text to render.</param>
    public unsafe IGraphicsSprite RenderPlainText (string str) {
        int maxBytes = str.Length * 4;

        Span<byte> strBuffer = str.Length <= 256
            ? stackalloc byte[maxBytes]
            : new byte[maxBytes];
        int len = Encoding.UTF8.GetBytes(str, strBuffer);

        SDL_Surface* surface;

        fixed (byte* ptr = strBuffer) {
            surface = SDL3_ttf.TTF_RenderText_Solid(Font, ptr, (nuint)len, WHITE);
        }

        SDL_Texture* tex = SDL3.SDL_CreateTextureFromSurface(_renderer.SdlRenderer, surface);
        if (tex is null) {
            throw new Exception($"Couldn't create texture: {SDL3.SDL_GetError()}.");
        }

        SDL3.SDL_DestroySurface(surface);

        return new GraphicsPlainTextSprite(_renderer, tex, null);
    }

    public unsafe IGraphicsSprite RenderShadowedPlainText (string str) {
        int maxBytes = str.Length * 4;

        Span<byte> strBuffer = str.Length <= 256
            ? stackalloc byte[maxBytes]
            : new byte[maxBytes];
        int len = Encoding.UTF8.GetBytes(str, strBuffer);

        SDL_Surface* surface;

        fixed (byte* ptr = strBuffer) {
            surface = SDL3_ttf.TTF_RenderText_Solid(Font, ptr, (nuint)len, WHITE);
        }

        SDL_Texture* tex = CreateCharTexture(surface);
        SDL_Texture* shadowTex = CreateShadowTexture(surface);
        if (tex is null) {
            throw new Exception($"Couldn't create texture: {SDL3.SDL_GetError()}.");
        }

        SDL3.SDL_DestroySurface(surface);

        return new GraphicsPlainTextSprite(_renderer, tex, shadowTex);
    }

    /// <summary>
    /// Given a character and a color, renders said character to a surface.
    /// </summary>
    /// <param name="color">The color to use.</param>
    private unsafe SDL_Surface* CreateSurface (uint ch, SDL_Color color) {
        var surface = SDL3_ttf.TTF_RenderGlyph_Solid(Font, ch, color);
        return surface;
    }

    /// <summary>
    /// Given a surface with rendered text, returns a texture that contains it.
    /// </summary>
    /// <param name="surface">The surface to use.</param>
    private unsafe SDL_Texture* CreateCharTexture (SDL_Surface* surface) {
        var tex = SDL3.SDL_CreateTextureFromSurface(_renderer.SdlRenderer, surface);
        return tex;
    }

    /// <summary>
    /// Given a surface with rendered text, returns a texture that contains an
    /// appropriate shadow for it.
    /// </summary>
    /// <param name="surface">The surface to use as reference.</param>
    private unsafe SDL_Texture* CreateShadowTexture (SDL_Surface* surface) {
        // Create a raw texture. This is only done to get the format, really.
        var rawTex = SDL3.SDL_CreateTextureFromSurface(_renderer.SdlRenderer, surface); // TODO: Maybe remove this step.

        // Convert the surface to the format we'll use in the streaming texture.
        var compatSurface = SDL3.SDL_ConvertSurface(surface, rawTex->format);

        // Create a texture to draw to.
        var streamingTex = SDL3.SDL_CreateTexture(
            _renderer.SdlRenderer,
            compatSurface->format,
            SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
            rawTex->w,
            Asset.LineHeight
        );

        nint pixels = 0; // The address of the pixels in the streaming texture.
        int pitch = 0; // The pitch in the texture.

        if (SDL3.SDL_LockTexture(streamingTex, null, &pixels, &pitch)) {
            byte* srcPixels = (byte*)compatSurface->pixels;
            byte* dstPixels = (byte*)pixels;

            int srcPitch = compatSurface->pitch;
            int copyWidth = Math.Min(pitch, srcPitch);

            // To compose the shadow, we iterate each pixel in the new texture,
            // and compare it to the surface that contains the rendered text.
            // If said pixel in the surface, or any of its top / left neighbors
            // is not empty, then the pixel in the shadow is not empty.
            // This creates a drop shadow in the bottom-right direction.
            for (int y = 0; y < streamingTex->h; y++) {
                for (int x = 0; x < streamingTex->w; x++) {
                    // The address of this pixel in the texture.
                    byte* dstPix = dstPixels + (y * pitch) + (x * 4);

                    // The address of the pixel in the surface that we'll copy.
                    byte* reference = null;

                    // If this pixel's y is inside the surface, check this
                    // pixel's position in the surface.
                    if (y < compatSurface->h) {
                        byte* srcPix = srcPixels + (y * srcPitch) + (x * 4);

                        if (srcPix[3] != 0) reference = srcPix;
                    }

                    // If reference is still empty, check the pixel in the
                    // surface to the left.
                    if (reference is null && x > 0 && y < compatSurface->h) {
                        byte* left = srcPixels + (y * srcPitch) + ((x - 1) * 4);
                        if (left[3] != 0) reference = left;
                    }
                    // If still empty, check the pixel above.
                    if (reference is null && y > 0 && y <= compatSurface->h) {
                        byte* top = srcPixels + ((y - 1) * srcPitch) + (x * 4);
                        if (top[3] != 0) reference = top;
                    }
                    // If still empty, check the pixel above to the left.
                    if (reference is null && x > 0 && y > 0 && y <= compatSurface->h) {
                        byte* topLeft = srcPixels + ((y - 1) * srcPitch) + ((x - 1) * 4);
                        if (topLeft[3] != 0) reference = topLeft;
                    }

                    // If we have a reference pixel, copy it.
                    if (reference is not null) {
                        dstPix[0] = 255;
                        dstPix[1] = 255;
                        dstPix[2] = 255;
                        dstPix[3] = 255;
                    }
                    // Else, this pixel will be transparent.
                    else {
                        dstPix[0] = 0;
                        dstPix[1] = 0;
                        dstPix[2] = 0;
                        dstPix[3] = 0;
                    }
                }
            }

            SDL3.SDL_UnlockTexture(streamingTex);
        }
        else {
            _logger.Error("Failed to copy texture to shadow's streaming texture.");

            // If something failed, we return the raw texture, which won't work
            // as a shadow but will not produce any errors later.
            SDL3.SDL_DestroySurface(compatSurface);
            return rawTex;
        }

        SDL3.SDL_DestroyTexture(rawTex);
        SDL3.SDL_DestroySurface(compatSurface);

        return streamingTex;
    }

    private unsafe SDL_Texture* GrowAtlas (SDL_Texture* atlas, int newHeight) {
        var newAtlas = SDL3.SDL_CreateTexture(
            _renderer.SdlRenderer,
            SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888, // TODO: Maybe do not hardcode this.
            SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET,
            _atlasWidth,
            newHeight
        );

        if (newAtlas is null) {
            throw new Exception("Failed to grow atlas texture.");
        }

        SDL3.SDL_SetRenderTarget(_renderer.SdlRenderer, newAtlas);
        SDL3.SDL_RenderTexture(_renderer.SdlRenderer, atlas, null, null);
        SDL3.SDL_SetRenderTarget(_renderer.SdlRenderer, null);

        SDL3.SDL_DestroyTexture(atlas);

        return newAtlas;
    }
}
