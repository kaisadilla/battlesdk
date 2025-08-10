using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaScreen : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "Screen";

    /// <summary>
    /// Closes the current screen.
    /// </summary>
    public static void close_current_screen () {
        Screen.Pop();
    }

    /// <summary>
    /// Opens the main menu.
    /// </summary>
    public static void open_main_menu () {
        Screen.MainMenu.Open();
    }

    /// <summary>
    /// Opens the bag.
    /// </summary>
    public static void open_bag () {
        Screen.Bag.Open();
    }

    /// <summary>
    /// Opens the save game screen.
    /// </summary>
    public static void open_save_game () {
        Screen.SaveGame.Open();
    }

    public override string ToString () {
        return $"<Screen>";
    }
}
