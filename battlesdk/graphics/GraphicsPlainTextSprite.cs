using battlesdk.data;
using SDL;

namespace battlesdk.graphics;
public class GraphicsPlainTextSprite : IGraphicsSprite {
    private unsafe SDL_Renderer* _renderer;
    private unsafe GraphicsTexture _tex;
    private unsafe GraphicsTexture? _shadowTex;

    public unsafe GraphicsPlainTextSprite (
        Renderer renderer, SDL_Texture* tex, SDL_Texture* shadowTex
    ) {
        _renderer = renderer.SdlRenderer;
        _tex = new(renderer, tex);
        _tex.SetTint(85, 85, 93);

        if (shadowTex is not null) {
            _shadowTex = new(renderer, shadowTex);
            _shadowTex.SetTint(0, 0, 0, Constants.TEXT_SHADOW_ALPHA);
        }
    }

    public SpriteFile Asset => throw new NotImplementedException();

    public unsafe void Destroy () {
        _tex.Destroy();
        _shadowTex?.Destroy();
    }

    public unsafe void Draw (IVec2 position) {
        if (_shadowTex is not null) {
            _shadowTex.Draw(position);
        }
        _tex.Draw(position);
    }

    public unsafe void Draw (IVec2 position, IVec2 size, ResizeMode resizeMode = ResizeMode.Stretch) {
        _shadowTex?.Draw(position, size, resizeMode);
        _tex.Draw(position, size, resizeMode);
    }

    public void DrawSubsprite (IVec2 position, int subsprite) {
        Draw(position);
    }

    public unsafe void DrawSection (SDL_FRect section, IVec2 position) {
        _shadowTex?.DrawSection(section, position);
        _tex.DrawSection(section, position);
    }

    public unsafe void DrawSection (SDL_FRect section, IVec2 position, IVec2 size, ResizeMode resizeMode = ResizeMode.Stretch) {
        _shadowTex?.DrawSection(section, position, size, resizeMode);
        _tex.DrawSection(section, position, size, resizeMode);
    }

    public void SetColor (ColorRGBA col) {
        _tex.SetTint((byte)col.R, (byte)col.G, (byte)col.B, (byte)col.A);
    }

    public void SetShadowColor (ColorRGBA col) {
        _shadowTex?.SetTint((byte)col.R, (byte)col.G, (byte)col.B, (byte)col.A);
    }
}
