using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaG {
    [MoonSharpHidden]
    public const string CLASSNAME = "G";

    /// <summary>
    /// The options chosen by the player for this specific game.
    /// </summary>
    public static LuaGameSettings game_options => new(G.GameOptions);

    /// <summary>
    /// The current player's name.
    /// </summary>
    public static string name => G.PlayerName;

    /// <summary>
    /// The amount of time, in seconds, that this game has been played.
    /// </summary>
    public static double time_played => G.TimePlayed;

    /// <summary>
    /// The amount of money the player has.
    /// </summary>
    public static int money => G.Money;

    public static LuaInventory inventory => new(G.Inventory);

    public static bool dex_unlocked => G.DexUnlocked;

    /// <summary>
    /// Adds the amount of money given to the player. Do not use negative
    /// numbers to remove money from them, instead use `G.remove_money(int)`.
    /// </summary>
    /// <param name="amount">The amount of money to give to the player.</param>
    public static void add_money (int amount) {
        G.AddMoney(amount);
    }

    /// <summary>
    /// Removes the amount of money given to the player. Do not use negative
    /// numbers to give money to them, instead use `G.add_money(int)`.
    /// </summary>
    /// <param name="amount">The amount of money to take from the player.</param>
    public static void remove_money (int amount) {
        G.RemoveMoney(amount);
    }
}
