using battlesdk.data;
using NLog;
using SDL;
using System.Runtime.CompilerServices;

namespace battlesdk.graphics;
public class GraphicsTexture : IGraphicsSprite {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public virtual SpriteFile Asset { get; }

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

    public unsafe GraphicsTexture (Renderer renderer, SpriteFile asset) {
        _renderer = renderer.SdlRenderer;

        Asset = asset;

        var surface = SDL3_image.IMG_Load(asset.Path);

        Width = surface->w;
        Height = surface->h;

        _texture = SDL3.SDL_CreateTextureFromSurface(renderer.SdlRenderer, surface);
        if (_texture is null) {
            throw new Exception($"Failed to load texture: {SDL3.SDL_GetError()}.");
        }

        SDL3.SDL_DestroySurface(surface);
        SDL3.SDL_SetTextureBlendMode(_texture, SDL_BlendMode.SDL_BLENDMODE_BLEND);
    }

    public static GraphicsTexture New (Renderer renderer, SpriteFile asset) {
        return asset switch {
            SpritesheetFile f => new GraphicsTexture(renderer, f),
            CharacterSpriteFile f => new GraphicsCharacterSprite(renderer, f),
            FrameSpriteFile f => new GraphicsFrameSprite(renderer, f),
            _ => new GraphicsTexture(renderer, asset)
        };
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
