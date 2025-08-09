using battlesdk.graphics;

namespace battlesdk.hud;
public class ChoiceMessageHudElement : IHudElement {
    private MessageHudElement _message;
    private ChoiceHudElement? _choice = null;

    public bool IsClosed { get; private set; } = false;
    public int Choice => _choice?.Choice ?? -1;

    public event EventHandler<EventArgs>? OnClose;

    public ChoiceMessageHudElement (
        Renderer renderer,
        int textboxId,
        int fontId,
        string text,
        List<string> choices,
        bool canBeCancelled = false,
        int defaultChoice = -1
    ) {
        _message = new(renderer, textboxId, fontId, false, text);

        _message.Textbox.OnMessageShown += (s, evt) => {
            _choice = new(
                renderer,
                Settings.ChoiceFrame,
                fontId,
                new(renderer.Width - 3, renderer.Height - 50),
                Position.BottomRight,
                choices,
                canBeCancelled,
                defaultChoice
            );

            _choice.OnClose += (s, evt) => Close();
        };
    }

    public void Draw () {
        _message.Draw();

        _choice?.Draw();
    }

    public void Update () {
        _message.Update();

        _choice?.Update();
    }

    public void Close () {
        _message.Close();
        _choice?.Close();

        OnClose?.Invoke(this, EventArgs.Empty);
    }
}
