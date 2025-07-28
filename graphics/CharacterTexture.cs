using battlesdk.data;
using Hexa.NET.SDL2;
using Hexa.NET.SDL2.Image;

namespace battlesdk.graphics;
public unsafe class CharacterTexture {
    public SDLTexture* Texture { get; private init; }

    public int Width { get; private init; }
    public int Height { get; private init; }

    private int _offsetX;
    private int _offsetY;

    public CharacterTexture (SDLRenderer* renderer, AssetFile sprite) {
        var surface = SDLImage.Load(sprite.Path);

        Width = surface->W / Constants.DEFAULT_CHAR_SIZE;
        Height = surface->H / Constants.DEFAULT_CHAR_SIZE;

        _offsetX = -((Constants.DEFAULT_CHAR_SIZE - Constants.TILE_SIZE) / 2);
        _offsetY = -(Constants.DEFAULT_CHAR_SIZE - Constants.TILE_SIZE);

        Texture = SDL.CreateTextureFromSurface(renderer, surface);
        if (Texture == null) throw new Exception("No tex.");

        SDL.FreeSurface(surface);
        SDL.SetTextureBlendMode(Texture, SDLBlendMode.Blend);
    }

    public SDLRect GetSprite (Direction dir, int set, int frame) {
        return new() {
            X = frame * Constants.DEFAULT_CHAR_SIZE + (set * 3 * Constants.DEFAULT_CHAR_SIZE),
            Y = (int)dir * Constants.DEFAULT_CHAR_SIZE,
            W = Constants.DEFAULT_CHAR_SIZE,
            H = Constants.DEFAULT_CHAR_SIZE,
        };
    }

    public unsafe void Draw (
        SDLRenderer* renderer, Vec2 position, Direction dir, int set, int frame
    ) {
        var src = GetSprite(dir, set, frame);

        SDLRect dst = new() {
            X = (int)position.X + _offsetX,
            Y = (int)position.Y + _offsetY,
            W = Constants.DEFAULT_CHAR_SIZE,
            H = Constants.DEFAULT_CHAR_SIZE,
        };

        SDL.RenderCopy(renderer, Texture, ref src, ref dst);
    }

    public void Destroy () {
        SDL.DestroyTexture(Texture);
    }
}
