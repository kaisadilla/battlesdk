using NLog;
using SDL;
using System.Runtime.CompilerServices;

namespace battlesdk.graphics.resources;
public class GraphicsTexture : IGraphicsSprite {
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
    public int Width { get; protected set; }
    /// <summary>
    /// The height of the loaded texture, in pixels.
    /// </summary>
    public int Height { get; protected set; }

    public unsafe GraphicsTexture (Renderer renderer) {
        _renderer = renderer.SdlRenderer;
    }

    public unsafe GraphicsTexture (Renderer renderer, SDL_Texture* texture) {
        _renderer = renderer.SdlRenderer;
        _texture = texture;

        Width = _texture->w;
        Height = _texture->h;

        SDL3.SDL_SetTextureBlendMode(_texture, SDL_BlendMode.SDL_BLENDMODE_BLEND);
    }

    /// <summary>
    /// Destroys this texture, freeing its memory.
    /// </summary>
    public virtual unsafe void Destroy () {
        SDL3.SDL_DestroyTexture(_texture);
    }

    public virtual unsafe void Draw (IVec2 position) {
        Draw(position, new(Width, Height), ResizeMode.Stretch);
    }

    public virtual unsafe void Draw (
        IVec2 position, IVec2 size, ResizeMode resizeMode = ResizeMode.Stretch
    ) {
        SDL_FRect dst = new() {
            x = position.X,
            y = position.Y,
            w = size.X,
            h = size.Y,
        };

        SDL3.SDL_RenderTexture(_renderer, _texture, null, &dst);
    }

    public virtual void DrawSubsprite (IVec2 position, int subsprite) {
        Draw(position);
    }

    /// <summary>
    /// Draws the given section of the texture to the position given, at its
    /// original resolution.
    /// </summary>
    /// <param name="section">The section of the texture to draw.</param>
    /// <param name="pos">The position in the screen (in pixels) at which to
    /// draw the texture.</param>
    public virtual unsafe void DrawSection (SDL_FRect section, IVec2 pos) {
        SDL_FRect dst = new() {
            x = pos.X,
            y = pos.Y,
            w = section.w,
            h = section.h,
        };

        SDL3.SDL_RenderTexture(_renderer, _texture, &section, &dst);
    }

    public virtual unsafe void DrawSection (
        SDL_FRect section,
        IVec2 pos,
        IVec2 size,
        ResizeMode resizeMode = ResizeMode.Stretch
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

    public virtual unsafe void SetTint (byte r, byte g, byte b, byte a = 255) {
        SDL3.SDL_SetTextureColorMod(_texture, r, g, b);
        SDL3.SDL_SetTextureAlphaMod(_texture, a);
    }

    public virtual unsafe void SetTint (ColorRGBA col) {
        SDL3.SDL_SetTextureColorMod(_texture, (byte)col.R, (byte)col.G, (byte)col.B);
        SDL3.SDL_SetTextureAlphaMod(_texture, (byte)col.A);
    }

    public virtual unsafe void ResetTint () {
        SDL3.SDL_SetTextureColorMod(_texture, 0, 0, 0);
        SDL3.SDL_SetTextureAlphaMod(_texture, 255);
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
