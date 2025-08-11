using MoonSharp.Interpreter;
using NLog;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaScreen : ILuaType {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

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

    /// <summary>
    /// Plays a transition on screen as described.
    /// </summary>
    /// <param name="script_name">The name of the transition's script.</param>
    /// <param name="seconds">The time, in seconds, that will take for the
    /// transition to complete.</param>
    /// <param name="reverse">True to transition FROM black, rather than TO black.</param>
    public static void play_transition (string script_name, float seconds, bool reverse) {
        if (Registry.Scripts.TryGetId(script_name, out int scriptId) == false) {
            _logger.Error($"Script '{script_name}' doesn't exist.");
            scriptId = -1;
        }

        Screen.PlayScriptTransition(scriptId, seconds, reverse);
    }

    public override string ToString () {
        return $"<Screen>";
    }
}
