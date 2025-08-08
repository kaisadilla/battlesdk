using battlesdk.input;

namespace battlesdk;
public static class InputManager {
    // TODO: Check potential cross-thread bugs.
    private static readonly Stack<IInputListener> _listeners = [];

    /// <summary>
    /// Pushes a new input listener on top of the stack, taking precedence
    /// over any existing listeners.
    /// </summary>
    /// <param name="listener">The listener to add to the stack.</param>
    public static void Push (IInputListener listener) {
        // If this listener blocks previous listeners, we notify them.
        if (listener.BlockOtherInput) {
            foreach (var l in _listeners) {
                l.OnInputBlocked();

                // If this listener also blocks, then any listener before it
                // was already blocked and doesn't need any notification.
                if (l.BlockOtherInput) {
                    break;
                }
            }
        }

        _listeners.Push(listener);
    }
    
    /// <summary>
    /// Blocks input for all input listeners that are already subscribed. New
    /// input managers will have priority over the block and thus won't be
    /// blocked. To remove the block, pop it normally.
    /// </summary>
    public static void PushBlock () {
        Push(new InputBlock());
    }

    /// <summary>
    /// Removes the most recent listener from the stack.
    /// </summary>
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
    public void OnInputBlocked () {}
}