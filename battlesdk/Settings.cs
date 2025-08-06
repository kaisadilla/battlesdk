using NLog;

namespace battlesdk;

/// <summary>
/// Contains the settings configured by the player in the game.
/// </summary>
public static class Settings {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public static int TextFont { get; private set; } = 0;
    public static int TextBox { get; private set; } = 0;

    public static void Init () {
        if (Registry.Fonts.TryGetId("power_clear", out var textFont) == false) {
            _logger.Warn("Font 'power_clear' is not in the Registry.");
        }
        else {
            TextFont = textFont;
        }

        if (Registry.Sprites.TryGetId("ui/frames/dp_textbox_1", out var tb) == false) {
            _logger.Warn("Textbox sprite 'ui/frames/dp_textbox_1' is not in the Registry.");
        }
        else {
            TextBox = tb;
        }
    }
}
