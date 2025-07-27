// See https://aka.ms/new-console-template for more information
using battlesdk;
using battlesdk.data;
using battlesdk.graphics;
using Hexa.NET.SDL2.Image;
using MoonSharp.Interpreter;
using SDL2;

const string MAP_PATH = "res/maps/test-map-1.tmx";

//var map = new Map(MAP_PATH);


var win = new Window();

var tilesetFiles = Directory.GetFiles("res/tilesets", "*.tsx").Select(Path.GetFileName);
foreach (var f in tilesetFiles) {
    if (f is null) continue;

    try {
        Tileset tileset = new(f, Path.Combine("res/tilesets", f));
        Registry.RegisterTileset(tileset);
    }
    catch (Exception) {
        Console.WriteLine($"Failed to load tileset: '{f}'.");
    }
}

var mapFiles = Directory.GetFiles("res/maps", "*.tmx").Select(Path.GetFileName);
foreach (var f in mapFiles) {
    if (f is null) continue;

    try {
        Map map = new(f, Path.Combine("res/maps", f));
        Registry.RegisterMap(map);
    }
    catch (Exception) {
        Console.WriteLine($"Failed to load map: '{f}'.");
    }
}

foreach (var t in Registry.Tilesets) {
    win.LoadTileset(t);
}


while (win.CloseRequested == false) {
    win.ProcessEvents();
    win.Render();
}

win.Destroy();

/*const int WIDTH = 680;
const int HEIGHT = 480;

SDL.SDL_Init(SDL.SDL_INIT_VIDEO);

nint window = SDL.SDL_CreateWindow("Test", SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, WIDTH, HEIGHT, 0);
nint renderer = SDL.SDL_CreateRenderer(window, 0, 0);

bool running = true;
SDL.SDL_Event evt;

Script script = new();
script.DoString(@"
    function print_something ()
        print('Lua called!');
    end
");

script.Call(script.Globals.Get("print_something"));


nint tex;
unsafe {
    tex = (nint)SDLImage.LoadTexture((Hexa.NET.SDL2.SDLRenderer*)renderer, "res/graphics/tilesets/test.png");
    //tex = SDL.SDL_CreateTextureFromSurface(renderer, surface);
    //SDL.SDL_FreeSurface(surface);
}
Console.WriteLine("Cool");
Console.WriteLine(tex);

while (running) {
    while (SDL.SDL_PollEvent(out evt) != 0) {
        if (evt.type == SDL.SDL_EventType.SDL_QUIT) {
            running = false;
        }
    }

    SDL.SDL_Rect dst = new() {
        x = 0,
        y = 0,
        w = 64,
        h = 64,
    };

    SDL.SDL_RenderClear(renderer);
    SDL.SDL_RenderCopy(renderer, tex, ref dst, ref dst);

    SDL.SDL_RenderPresent(renderer);
}

SDL.SDL_DestroyTexture(tex);
SDL.SDL_DestroyRenderer(renderer);
SDL.SDL_DestroyWindow(window);
SDL.SDL_Quit();*/