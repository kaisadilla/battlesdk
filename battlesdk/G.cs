using battlesdk.world;
using NLog;

namespace battlesdk;
public static class G {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public static World World { get; private set; } = new();
    public static bool DexUnlocked { get; private set; } = true;

    public static void LoadGame () {
        World.TransferTo(Registry.Worlds[0], new(6, -3));
        _logger.Info("Loaded game.");
    }
}
