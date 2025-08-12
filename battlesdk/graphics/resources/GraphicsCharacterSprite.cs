using battlesdk.data;

namespace battlesdk.graphics.resources;
public unsafe class GraphicsCharacterSprite : GraphicsSprite {
    public override CharacterSpriteFile Asset { get; }

    private int _offsetX;
    private int _offsetY;

    public GraphicsCharacterSprite (Renderer renderer, CharacterSpriteFile asset)
        : base(renderer, asset)
    {
        Asset = asset;
        _offsetX = -((asset.SpriteSize.X - Settings.TileSize) / 2);
        _offsetY = -(asset.SpriteSize.Y - Settings.TileSize);
    }

    public override void Draw (IVec2 position) {
        DrawSection(
            SdlFRect(0, 0, Asset.SpriteSize.X, Asset.SpriteSize.Y),
            position + new IVec2(_offsetX, _offsetY)
        );
    }

    public override void DrawSubsprite (IVec2 position, int subsprite) {
        base.DrawSubsprite(position + new IVec2(_offsetX, _offsetY), subsprite);
    }
}
