using battlesdk.data;
using SDL;

namespace battlesdk.graphics.resources;
public class GraphicsPlainTextSprite : IGraphicsSprite {
    private unsafe SDL_Renderer* _renderer;
    private unsafe GraphicsFont _font;
    private unsafe GraphicsTexture _tex;
    private unsafe GraphicsTexture? _shadowTex;

    public int Width { get; }
    public int Height { get; }

    public unsafe GraphicsPlainTextSprite (
        Renderer renderer, GraphicsFont font, SDL_Texture* tex, SDL_Texture* shadowTex
    ) {
        _renderer = renderer.SdlRenderer;
        _font = font;
        _tex = new(renderer, tex);
        _tex.SetTint(Settings.DefaultTextColor);

        if (shadowTex is not null) {
            _shadowTex = new(renderer, shadowTex);
            _shadowTex.SetTint(Settings.DefaultTextShadowColor);
        }

        Width = Math.Max(_tex.Width, _shadowTex?.Width ?? 0);
        Height = Math.Max(_tex.Height, _shadowTex?.Height ?? 0);
    }

    public SpriteFile Asset => throw new NotImplementedException();

    public unsafe void Destroy () {
        _tex.Destroy();
        _shadowTex?.Destroy();
    }

    public unsafe void Draw (IVec2 position) {
        position = new(position.X, _font.Asset.GetCorrectY(position.Y));
        if (_shadowTex is not null) {
            _shadowTex.Draw(position);
        }
        _tex.Draw(position);
    }

    public unsafe void Draw (IVec2 position, IVec2 size, ResizeMode resizeMode = ResizeMode.Stretch) {
        position = new(position.X, _font.Asset.GetCorrectY(position.Y));
        _shadowTex?.Draw(position, size, resizeMode);
        _tex.Draw(position, size, resizeMode);
    }

    public void DrawSubsprite (IVec2 position, int subsprite) {
        position = new(position.X, _font.Asset.GetCorrectY(position.Y));
        Draw(position);
    }

    public unsafe void DrawSection (SDL_FRect section, IVec2 position) {
        position = new(position.X, _font.Asset.GetCorrectY(position.Y));
        _shadowTex?.DrawSection(section, position);
        _tex.DrawSection(section, position);
    }

    public unsafe void DrawSection (SDL_FRect section, IVec2 position, IVec2 size, ResizeMode resizeMode = ResizeMode.Stretch) {
        position = new(position.X, _font.Asset.GetCorrectY(position.Y));
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
