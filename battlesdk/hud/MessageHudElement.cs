using battlesdk.graphics;
using battlesdk.graphics.elements;
using battlesdk.input;

namespace battlesdk.hud;
public class MessageHudElement : IHudElement, IInputListener {
    /// <summary>
    /// The textbox controlled by this message.
    /// </summary>
    public AnimatableTextbox Textbox { get; }

    private bool _hasControl = false;

    public bool IsClosed { get; private set; } = false;

    public string Name => "Textbox Hud Element";
    public bool BlockOtherInput => true;

    /// <summary>
    /// Triggers when the textbox is requesting to be closed. This usually
    /// happens when the player has seen the full text and is pressing a key
    /// to continue.
    /// </summary>
    public event EventHandler<EventArgs>? OnClose;

    public MessageHudElement (
        Renderer renderer, int frameId, int fontId, string text
    ) {
        OnClose += (s, evt) => IsClosed = true;

        Textbox = new(
            renderer,
            frameId,
            fontId,
            new(3, Settings.ViewportHeight - 48),
            new(Settings.ViewportWidth - 6, 46),
            text
        );
    }

    public void TakeControl () {
        if (_hasControl) return;

        InputManager.Push(this);
        _hasControl = true;
    }

    public void ReleaseControl () {
        if (_hasControl == false) return;

        _hasControl = false;
        InputManager.Pop();
    }

    public void Update () {
        Textbox.Update();
    }

    public void Draw () {
        Textbox.Draw();
    }

    public void Close () {
        if (IsClosed) return;

        ReleaseControl();

        OnClose?.Invoke(this, EventArgs.Empty);
        Textbox.Destroy();
    }

    public void HandleInput () {
        if (Controls.GetKeyDown(ActionKey.Primary) || Controls.GetKeyDown(ActionKey.Secondary)) {
            if (Textbox.IsMessageShown) {
                Audio.PlayBeepShort();
                Close();
            }
            else {
                var advanced = Textbox.Next();
                if (advanced) Audio.PlayBeepShort();
            }
        }
    }

    public void OnInputBlocked () {

    }
}