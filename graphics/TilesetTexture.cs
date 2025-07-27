using Hexa.NET.SDL2;
using Hexa.NET.SDL2.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battlesdk.graphics;
public unsafe class TilesetTexture {
    public SDLTexture* Texture { get; private init; }

    public int Width { get; private init; }
    public int Height { get; private init; }

    public TilesetTexture (SDLRenderer* renderer, string path) {
        var surface = SDLImage.Load(path);

        Width = surface->W / 16;
        Height = surface->H / 16;

        Texture = SDL.CreateTextureFromSurface(renderer, surface);
        if (Texture == null) throw new Exception("No tex.");

        SDL.FreeSurface(surface);
        SDL.SetTextureBlendMode(Texture, SDLBlendMode.Blend);
    }

    public void Destroy () {
        SDL.DestroyTexture(Texture);
    }
}
