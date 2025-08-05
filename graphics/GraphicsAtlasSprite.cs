using battlesdk.data;
using NLog;
using SDL;

namespace battlesdk.graphics;
public class GraphicsAtlasSprite : IGraphicsSprite {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private unsafe SDL_Renderer* _renderer;
    private GraphicsTexture _atlas;
    /// <summary>
    /// The position of this specific sprite in the texture;
    /// </summary>
    private SDL_FRect _src;

    public SpriteFile Asset { get; }

    public unsafe GraphicsAtlasSprite (
        Renderer renderer, SpritesheetFile asset, string name
    ) {
        _renderer = renderer.SdlRenderer;
        Asset = asset;

        var atlas = renderer.GetSpriteAtlas(asset.Id)
            ?? throw new("Couldn't find spritesheet atlas.");
        _atlas = atlas;

        var index = asset.Names.IndexOf(name);

        var spritesPerRow = _atlas.Width / asset.SpriteSize.X;
        var x = (index % spritesPerRow) * asset.SpriteSize.X;
        var y = (index / spritesPerRow) * asset.SpriteSize.Y;

        _src = SdlFRect(x, y, asset.SpriteSize.X, asset.SpriteSize.Y);
    }

    public virtual unsafe void Destroy () {
    }

    public unsafe void Draw (IVec2 position) {
        _atlas.DrawSection(_src, position);
    }

    public unsafe void Draw (
        IVec2 position, IVec2 size, ResizeMode resizeMode = ResizeMode.Stretch
    ) {
        _atlas.DrawSection(_src, position, size, resizeMode);
    }

    public unsafe void DrawSection (SDL_FRect section, IVec2 position) {
        _atlas.DrawSection(section, position);
    }

    public unsafe void DrawSection (
        SDL_FRect section,
        IVec2 position,
        IVec2 size,
        ResizeMode resizeMode = ResizeMode.Stretch
    ) {
        _atlas.DrawSection(section, position, size, resizeMode);
    }
}
