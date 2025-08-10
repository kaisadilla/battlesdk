using battlesdk.world;
using NLog;

namespace battlesdk;
public static class G {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public static World World { get; private set; } = new();
    /// <summary>
    /// The name of the player.
    /// </summary>
    public static string PlayerName { get; private set; } = "Dawn";
    /// <summary>
    /// The amount of money the player has.
    /// </summary>
    public static int Money { get; private set; } = 3000;
    /// <summary>
    /// If true, the player has access to a dex.
    /// </summary>
    public static bool DexUnlocked { get; private set; } = true;

    public static void LoadGame () {
        World.TransferTo(Registry.Worlds[0], new(9, 16));
        _logger.Info("Loaded game.");
    }

    public static void RemoveMoney (int amount) {
        Money = Math.Max(Settings.MinMoney, Money - amount);
    }

    public static void AddMoney (int amount) {
        Money = Math.Min(Settings.MaxMoney, Money + amount);
    }
}
