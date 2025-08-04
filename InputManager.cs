using battlesdk.input;

namespace battlesdk;
public static class InputManager {
    // TODO: Check potential cross-thread bugs.
    private static readonly Stack<IInputListener> _listeners = [];

    public static void Push (IInputListener listener) {
        _listeners.Push(listener);
    }
    
    /// <summary>
    /// Blocks input for all input listeners that are already subscribed. New
    /// input managers will have priority over the block and thus won't be
    /// blocked. To remove the block, pop it normally.
    /// </summary>
    public static void PushBlock () {
        _listeners.Push(new InputBlock());
    }

    public static void Pop () {
        _listeners.Pop();
    }

    public static void Update () {
        foreach (var listener in _listeners) {
            listener.HandleInput();
            if (listener.BlockOtherInput) {
                break;
            }
        }
    }
}

public class InputBlock : IInputListener {
    public bool BlockOtherInput => true;

    public void HandleInput () {}
}