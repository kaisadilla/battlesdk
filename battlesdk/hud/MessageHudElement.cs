using battlesdk.graphics;

namespace battlesdk.hud;
public class MessageHudElement : IHudElement {
    /// <summary>
    /// The textbox controlled by this message.
    /// </summary>
    public TextboxHudElement Textbox { get; }

    public bool IsClosed { get; private set; } = false;

    public bool IsCloseRequested = false;

    /// <summary>
    /// Triggers when the textbox is requesting to be closed. This usually
    /// happens when the player has seen the full text and is pressing a key
    /// to continue.
    /// </summary>
    public event EventHandler<EventArgs>? OnClose;

    public MessageHudElement (
        Renderer renderer, int frameId, int fontId, bool closeable, string text
    ) {
        Textbox = new(
            renderer,
            frameId,
            fontId,
            new(3, Settings.ViewportHeight - 48),
            new(Settings.ViewportWidth - 6, 46),
            closeable,
            text
        );

        Textbox.OnClose
            += (s, evt) => OnClose?.Invoke(this, EventArgs.Empty);
    }

    public void CedeControl () {
        Textbox.CedeControl();
    }

    public void Update () {
        Textbox.Update();
    }

    public void Draw () {
        Textbox.Draw();
    }

    public void Close () {
        Textbox.Close();
    }
}