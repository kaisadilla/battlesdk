global using battlesdk.types;
global using static battlesdk.types.TypesUtils;

using battlesdk;
using NLog;

Logger _logger = LogManager.GetCurrentClassLogger();

const int WIDTH = 340;
const int HEIGHT = 240;
const float ZOOM = 3f;

_logger.Info("Launching BattleSDK.");

Registry.BuildRegistry();
G.LoadGame();

Audio.Init();
Audio.RegisterSounds();
var win = new battlesdk.graphics.Window(WIDTH, HEIGHT, ZOOM);

while (win.CloseRequested == false) {
    win.ProcessEvents();
    G.World?.OnFrameStart();

    G.World?.Update();

    win.Render();
}

win.Destroy();
