using battlesdk.data;
using NLog;
using SDL;

namespace battlesdk.graphics;
public class GraphicsSprite : GraphicsTexture {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public virtual SpriteFile Asset { get; }

    public unsafe GraphicsSprite (Renderer renderer, SpriteFile asset) : base(renderer) {
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

    public static GraphicsSprite New (Renderer renderer, SpriteFile asset) {
        return asset switch {
            CharacterSpriteFile f => new GraphicsCharacterSprite(renderer, f),
            SpritesheetFile f => new GraphicsSprite(renderer, f),
            FrameSpriteFile f => new GraphicsFrameSprite(renderer, f),
            _ => new GraphicsSprite(renderer, asset)
        };
    }

    public override void DrawSubsprite (IVec2 position, int subsprite) {
        if (Asset is SpritesheetFile spritesheet) {
            var origin = spritesheet.GetSubspriteOrigin(subsprite);

            DrawSection(
                SdlFRect(
                    origin.X,
                    origin.Y,
                    spritesheet.SpriteSize.X,
                    spritesheet.SpriteSize.Y
                ),
                position
            );
        }
    }
}
