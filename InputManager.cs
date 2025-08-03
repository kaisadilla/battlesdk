using battlesdk.input;

namespace battlesdk;
public static class InputManager {
    private static readonly Stack<IInputListener> _listeners = [];

    public static void Subscribe (IInputListener listener) {
        _listeners.Push(listener);
    }

    public static void Unsubscribe () {
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
