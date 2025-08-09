using battlesdk.graphics;
using battlesdk.hud;
using NLog;

namespace battlesdk;

public static class Hud {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static Renderer _renderer = null!;

    private static readonly IdProvider _elementsIdProvider = new();
    private static readonly Dictionary<int, IHudElement> _elements = [];

    public static unsafe void Init (Renderer renderer) {
        _renderer = renderer;
    }

    public static void Update () {
        foreach (var el in _elements.Values) {
            el.Update();
        }
    }

    public static unsafe void Draw () {
        if (_renderer is null) return;

        foreach (var el in _elements.Values) {
            el.Draw();
        }
    }

    /// <summary>
    /// Shows a message with the given text on the screen.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <returns>The message being shown.</returns>
    public static MessageHudElement ShowMessage (string text) {
        var id = _elementsIdProvider.NextId();
        var msg = new MessageHudElement(
            _renderer,
            Settings.TextboxFrame,
            Settings.TextFont,
            true,
            text
        );

        _elements[id] = msg;
        msg.OnClose += (s, evt) => _elements.Remove(id);

        return msg;
    }

    public static ChoiceMessageHudElement ShowChoiceMessage (
        string message,
        List<string> choices,
        bool canBeCancelled = true,
        int defaultChoice = -1
    ) {
        var id = _elementsIdProvider.NextId();
        var choice = new ChoiceMessageHudElement(
            _renderer,
            Settings.TextboxFrame,
            Settings.TextFont,
            message,
            choices,
            canBeCancelled,
            defaultChoice
        );

        _elements[id] = choice;
        choice.OnClose += (s, evt) => _elements.Remove(id);

        return choice;
    }
}

public enum HudState {
    None,
    Hud,
}