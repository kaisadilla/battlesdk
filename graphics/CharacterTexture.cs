using battlesdk.data;
using SDL;

namespace battlesdk.graphics;
public unsafe class CharacterTexture {
    public SDL_Texture* Texture { get; private init; }

    public int Width { get; private init; }
    public int Height { get; private init; }

    private int _offsetX;
    private int _offsetY;

    public CharacterTexture (SDL_Renderer* renderer, AssetFile sprite) {
        var surface = SDL3_image.IMG_Load(sprite.Path);

        Width = surface->w / Constants.DEFAULT_CHAR_SIZE;
        Height = surface->h / Constants.DEFAULT_CHAR_SIZE;

        _offsetX = -((Constants.DEFAULT_CHAR_SIZE - Constants.TILE_SIZE) / 2);
        _offsetY = -(Constants.DEFAULT_CHAR_SIZE - Constants.TILE_SIZE);

        Texture = SDL3.SDL_CreateTextureFromSurface(renderer, surface);
        if (Texture == null) throw new Exception("No tex.");

        SDL3.SDL_DestroySurface(surface);
        SDL3.SDL_SetTextureBlendMode(Texture, SDL_BlendMode.SDL_BLENDMODE_BLEND);
        SDL3.SDL_SetTextureScaleMode(Texture, SDL_ScaleMode.SDL_SCALEMODE_NEAREST);
    }

    public SDL_FRect GetSprite (Direction dir, int set, int frame) {
        return new() {
            x = frame * Constants.DEFAULT_CHAR_SIZE + (set * 3 * Constants.DEFAULT_CHAR_SIZE),
            y = (int)dir * Constants.DEFAULT_CHAR_SIZE,
            w = Constants.DEFAULT_CHAR_SIZE,
            h = Constants.DEFAULT_CHAR_SIZE,
        };
    }

    public unsafe void Draw (
        SDL_Renderer* renderer, Vec2 position, Direction dir, int set, int frame
    ) {
        var src = GetSprite(dir, set, frame);

        SDL_FRect dst = new() {
            x = (int)position.X + _offsetX,
            y = (int)position.Y + _offsetY,
            w = Constants.DEFAULT_CHAR_SIZE,
            h = Constants.DEFAULT_CHAR_SIZE,
        };

        SDL3.SDL_RenderTexture(renderer, Texture, &src, &dst);
    }

    public void Destroy () {
        SDL3.SDL_DestroyTexture(Texture);
    }
}
