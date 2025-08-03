using battlesdk.data;
using SDL;

namespace battlesdk.graphics;
public unsafe class StdTexture {
    public SDL_Texture* Texture { get; private init; }

    public int Width { get; private init; }
    public int Height { get; private init; }

    public StdTexture (SDL_Renderer* renderer, AssetFile sprite) {
        var surface = SDL3_image.IMG_Load(sprite.Path);

        Width = surface->w;
        Height = surface->h;

        Texture = SDL3.SDL_CreateTextureFromSurface(renderer, surface);
        if (Texture == null) throw new Exception("No tex.");

        SDL3.SDL_DestroySurface(surface);
        SDL3.SDL_SetTextureBlendMode(Texture, SDL_BlendMode.SDL_BLENDMODE_BLEND);
    }

    public unsafe void Draw (SDL_Renderer* renderer, Vec2 position) {
        SDL_FRect dst = new() {
            x = (int)position.X,
            y = (int)position.Y,
            w = Width,
            h = Height,
        };

        SDL3.SDL_RenderTexture(renderer, Texture, null, &dst);
    }

    public void Destroy () {
        SDL3.SDL_DestroyTexture(Texture);
    }
}
