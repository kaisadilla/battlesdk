using NLog;
using SDL;
using System.Runtime.CompilerServices;

namespace battlesdk.graphics;
public abstract class GraphicsTexture {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The renderer this texture was loaded to.
    /// </summary>
    protected unsafe SDL_Renderer* _renderer;
    /// <summary>
    /// The texture owned by the renderer.
    /// </summary>
    protected unsafe SDL_Texture* _texture;

    /// <summary>
    /// The width of the loaded texture, in pixels.
    /// </summary>
    protected int _width;
    /// <summary>
    /// The height of the loaded texture, in pixels.
    /// </summary>
    protected int _height;

    protected unsafe GraphicsTexture (SDL_Renderer* renderer, string path) {
        _renderer = renderer;

        var surface = SDL3_image.IMG_Load(path);

        _width = surface->w;
        _height = surface->h;

        _texture = SDL3.SDL_CreateTextureFromSurface(renderer, surface);
        if (_texture is null) {
            throw new Exception($"Failed to load texture: {SDL3.SDL_GetError()}.");
        }

        SDL3.SDL_DestroySurface(surface);
        SDL3.SDL_SetTextureBlendMode(_texture, SDL_BlendMode.SDL_BLENDMODE_BLEND);
    }

    /// <summary>
    /// Destroys this texture, freeing its memory.
    /// </summary>
    public virtual unsafe void Destroy () {
        SDL3.SDL_DestroyTexture(_texture);
    }

    /// <summary>
    /// Draws the given section of the texture to the position given, at its
    /// original resolution.
    /// </summary>
    /// <param name="section">The section of the texture to draw.</param>
    /// <param name="pos">The position in the screen (in pixels) at which to
    /// draw the texture.</param>
    protected unsafe void DrawSection (SDL_FRect section, IVec2 pos) {
        SDL_FRect dst = new() {
            x = pos.X,
            y = pos.Y,
            w = section.w,
            h = section.h,
        };

        SDL3.SDL_RenderTexture(_renderer, _texture, &section, &dst);
    }

    protected unsafe void DrawSectionResize (
        SDL_FRect section, IVec2 pos, IVec2 size, ResizeMode resizeMode
    ) {
        switch (resizeMode) {
            case ResizeMode.Stretch:
                DrawSectionResizeStretch(section, pos, size);
                break;
            case ResizeMode.Repeat:
                break;
            default:
                _logger.Error("Missing resize mode.");
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected unsafe void DrawSectionResizeStretch (
        SDL_FRect section, IVec2 pos, IVec2 size
    ) {
        SDL_FRect dst = new() {
            x = pos.X,
            y = pos.Y,
            w = size.X,
            h = size.Y,
        };

        SDL3.SDL_RenderTexture(_renderer, _texture, &section, &dst);
    }
}
