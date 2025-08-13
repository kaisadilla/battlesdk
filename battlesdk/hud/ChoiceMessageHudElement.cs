using battlesdk.graphics;
using battlesdk.graphics.elements;
using battlesdk.input;

namespace battlesdk.hud;
public class ChoiceMessageHudElement : IHudElement, IInputListener {
    private bool _hasControl = false;

    private MessageHudElement _message;
    private ChoiceBox _choice;

    private bool _canBeCancelled;
    private int _defaultChoice;

    public bool IsClosed { get; private set; } = false;
    public int Choice { get; private set; } = -1;

    public string Name => "Choice message hud element";

    public bool BlockOtherInput => true;

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
        OnClose += (s, evt) => IsClosed = true;

        _canBeCancelled = canBeCancelled;
        _defaultChoice = defaultChoice;

        _message = new(renderer, textboxId, fontId, text);
        _choice = new(
            renderer,
            PlayerSettings.BoxFrame,
            fontId,
            new(renderer.Width - 3, renderer.Height - 50),
            AnchorPoint.BottomRight,
            choices
        );
    }

    public void CedeControl () {
        _hasControl = true;
        InputManager.Push(this);
    }

    public void Draw () {
        _message.Draw();

        if (_message.Textbox.IsMessageShown) {
            _choice.Draw();
        }
    }

    public void Update () {
        _message.Update();
    }

    public void Close () {
        if (IsClosed) return;

        _message.Close();

        if (_hasControl) {
            InputManager.Pop();
        }

        OnClose?.Invoke(this, EventArgs.Empty);
    }

    public void HandleInput () {
        // While the message is printing its text, forward input to it.
        if (_message.Textbox.IsMessageShown == false) {
            _message.HandleInput();
            return;
        }

        if (Controls.GetKeyDown(ActionKey.Up)) {
            Audio.PlayBeepShort();
            _choice.MoveUp();
        }
        else if (Controls.GetKeyDown(ActionKey.Down)) {
            Audio.PlayBeepShort();
            _choice.MoveDown();
        }
        else if (Controls.GetKeyDown(ActionKey.Primary)) {
            Audio.PlayBeepShort();
            Choice = _choice.CurrentChoice;
            Close();
        }
        else if (Controls.GetKeyDown(ActionKey.Secondary)) {
            if (_canBeCancelled) {
                Audio.PlayBeepShort();
                Choice = _defaultChoice;
                Close();
            }
        }
    }

    public void OnInputBlocked () {

    }
}
