using battlesdk.graphics;
using battlesdk.hud;
using NLog;

namespace battlesdk;

public static class Hud {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static Renderer? _renderer = null;

    private static Textbox? _textbox = null;
    private static Task? _textboxTask = null;

    public static unsafe void Init (Renderer renderer) {
        _renderer = renderer;
    }

    public static void Update () {
        _textbox?.Update();

        if (_textboxTask?.IsCompleted == true) {
            _textbox = null;
            _textboxTask = null;
        }
    }

    public static unsafe void Draw () {
        if (_renderer is null) return;

        _textbox?.Draw();
    }

    /// <summary>
    /// Shows a textbox with the given message on the screen, and returns a
    /// task that will be completed when the textbox is closed.
    /// </summary>
    /// <param name="text">The text to display.</param>
    public static Task ShowTextbox (string text) {
        _textbox = new(
            _renderer,
            Settings.TextBox,
            Settings.TextFont,
            new(3, Constants.VIEWPORT_HEIGHT - 48),
            new(Constants.VIEWPORT_WIDTH - 6, 46),
            text
        );
        _textboxTask = _textbox.WaitUntilClose();

        return _textboxTask;
    }
}

public enum HudState {
    None,
    Hud,
}