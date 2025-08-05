using battlesdk.data;
using SDL;

namespace battlesdk.graphics;
public unsafe class GraphicsCharacterSprite : GraphicsTexture {
    public override CharacterSpriteFile Asset { get; }

    private int _offsetX;
    private int _offsetY;

    public GraphicsCharacterSprite (Renderer renderer, CharacterSpriteFile asset)
        : base(renderer, asset)
    {
        Asset = asset;
        _offsetX = -((asset.SpriteSize.X - Constants.TILE_SIZE) / 2);
        _offsetY = -(asset.SpriteSize.Y - Constants.TILE_SIZE);
    }

    public SDL_FRect GetSprite (Direction dir, int set, int frame) {
        return new() {
            x = frame * Constants.DEFAULT_CHAR_SIZE + (set * 3 * Constants.DEFAULT_CHAR_SIZE),
            y = (int)dir * Constants.DEFAULT_CHAR_SIZE,
            w = Constants.DEFAULT_CHAR_SIZE,
            h = Constants.DEFAULT_CHAR_SIZE,
        };
    }

    public override void Draw (IVec2 position) {
        DrawSection(SdlFRect(0, 0, Asset.SpriteSize.X, Asset.SpriteSize.Y), position);
    }

    public unsafe void Draw (
        IVec2 position, Direction dir, int set, int frame
    ) {
        var src = GetSprite(dir, set, frame);

        DrawSection(src, position + new IVec2(_offsetX, _offsetY));
    }
}
