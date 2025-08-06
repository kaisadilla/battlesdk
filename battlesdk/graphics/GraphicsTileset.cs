using SDL;

namespace battlesdk.graphics;
public unsafe class GraphicsTileset {
    public SDL_Texture* Texture { get; private init; }

    public int Width { get; private init; }
    public int Height { get; private init; }

    public GraphicsTileset (SDL_Renderer* renderer, string path) {
        var surface = SDL3_image.IMG_Load(path);

        Width = surface->w / Constants.TILE_SIZE;
        Height = surface->h / Constants.TILE_SIZE;

        Texture = SDL3.SDL_CreateTextureFromSurface(renderer, surface);
        if (Texture == null) throw new Exception("No tex.");

        SDL3.SDL_DestroySurface(surface);
        SDL3.SDL_SetTextureBlendMode(Texture, SDL_BlendMode.SDL_BLENDMODE_BLEND);
    }

    public void Destroy () {
        SDL3.SDL_DestroyTexture(Texture);
    }
}
