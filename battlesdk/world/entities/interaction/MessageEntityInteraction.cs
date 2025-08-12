using battlesdk.data;
using NLog;

namespace battlesdk.world.entities.interaction;

public class MessageEntityInteraction : EntityInteraction {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private List<string> _textKeys;

    public MessageEntityInteraction (
        Entity entity, MessageEntityInteractionData data
    ) : base(entity) {
        _textKeys = [.. data.TextKeys];
    }

    public override void Interact (Direction from) {
        if (IsInteracting == true) return;
        IsInteracting = true;

        base.Interact(from);

        ShowNextTextbox(0);
    }

    private void ShowNextTextbox (int index) {
        if (index >= _textKeys.Count) {
            End();
            return;
        }

        var tb = Hud.ShowMessage(Localization.Text(_textKeys[index]));
        tb.OnClose += (s, evt) => ShowNextTextbox(index + 1);
    }
}
