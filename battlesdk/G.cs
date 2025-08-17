using battlesdk.game;
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
    /// The amount of time, in seconds, that this game has been played.
    /// </summary>
    public static double TimePlayed { get; private set; } = 0f;
    /// <summary>
    /// The amount of money the player has.
    /// </summary>
    public static int Money { get; private set; } = 3000;
    /// <summary>
    /// The player's inventory.
    /// </summary>
    public static Inventory Inventory { get; private set; } = new();
    /// <summary>
    /// If true, the player has access to a dex.
    /// </summary>
    public static bool DexUnlocked { get; private set; } = true;

    public static void LoadGame () {
        Inventory = Inventory.Load();

        // TODO: Remove
        Inventory.AddAmount("poke_ball", 22);
        Inventory.AddAmount("great_ball", 15);
        Inventory.AddAmount("ultra_ball", 7);
        Inventory.AddAmount("master_ball", 1);
        Inventory.AddAmount("net_ball", 11);
        //Inventory.AddAmount("dive_ball", 8);
        Inventory.AddAmount("nest_ball", 6);
        Inventory.AddAmount("repeat_ball", 23);
        Inventory.AddAmount("timer_ball", 999);
        Inventory.AddAmount("luxury_ball", 14);
        Inventory.AddAmount("premier_ball", 7);
        Inventory.AddAmount("dusk_ball", 3);
        Inventory.AddAmount("heal_ball", 5);
        Inventory.AddAmount("quick_ball", 5);
        Inventory.AddAmount("cherish_ball", 12);

        World.TransferTo(Registry.Worlds[0], new(9, 16));
        _logger.Info("Loaded game.");
    }

    public static void Update () {
        TimePlayed += Time.DeltaTime;

        World.Update();
    }

    public static void RemoveMoney (int amount) {
        Money = Math.Max(Settings.MinMoney, Money - amount);
    }

    public static void AddMoney (int amount) {
        Money = Math.Min(Settings.MaxMoney, Money + amount);
    }
}
