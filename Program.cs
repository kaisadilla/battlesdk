global using battlesdk.types;
global using static battlesdk.types.TypesUtils;

using battlesdk;
using battlesdk.data;
using battlesdk.graphics;
using NLog;

Logger _logger = LogManager.GetCurrentClassLogger();

const int WIDTH = 340;
const int HEIGHT = 240;
const float ZOOM = 3f;

BuildRegistry();
G.LoadGame();

var win = new Window(WIDTH, HEIGHT, ZOOM);

while (win.CloseRequested == false) {
    win.ProcessEvents();
    G.World?.OnFrameStart();

    G.World?.Update();

    win.Render();
}

win.Destroy();

void BuildRegistry () {
    var tilesetFiles = Directory.GetFiles("res/tilesets", "*.tsx").Select(Path.GetFileName);
    foreach (var f in tilesetFiles) {
        if (f is null) continue;

        try {
            Tileset tileset = new(f, Path.Combine("res/tilesets", f));
            Registry.Tilesets.Register(tileset);
            _logger.Info($"Loaded tileset '{f}'.");
        }
        catch (Exception ex) {
            _logger.Error(ex, $"Failed to load tileset '{f}'.");
        }
    }

    var mapFiles = Directory.GetFiles("res/maps", "*.tmx").Select(Path.GetFileName);
    foreach (var f in mapFiles) {
        if (f is null) continue;

        try {
            Map map = new(f, Path.Combine("res/maps", f));
            Registry.Maps.Register(map);
            _logger.Info($"Loaded map '{f}'.");
        }
        catch (Exception ex) {
            _logger.Error(ex, $"Failed to load map '{f}'.");
        }
    }

    var charSpriteFiles = Directory.GetFiles("res/graphics/characters", "*.png")
        .Select(Path.GetFileName);
    foreach (var f in charSpriteFiles) {
        if (f is null) continue;

        try {
            AssetFile sprite = new(f, Path.Combine("res/graphics/characters", f));
            Registry.CharSprites.Register(sprite);
            _logger.Info($"Loaded character sprite '{f}'.");
        }
        catch (Exception ex) {
            _logger.Error(ex, $"Failed to register character sprite '{f}'.");
        }
    }

    var soundFiles = Directory.GetFiles("res/sounds", "*.wav").Select(Path.GetFileName);
    foreach (var f in mapFiles) {
        if (f is null) continue;

        try {
            AssetFile sound = new(f, Path.Combine("res/sounds", f));
            Registry.Sounds.Register(sound);
            _logger.Info($"Loaded sound '{f}'.");
        }
        catch (Exception ex) {
            _logger.Error(ex, $"Failed to register sound '{f}'.");
        }
    }
}
