using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaG {
    [MoonSharpHidden]
    public const string CLASSNAME = "G";

    /// <summary>
    /// The amount of money the player has.
    /// </summary>
    public static int money => G.Money;

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
