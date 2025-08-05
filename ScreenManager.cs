using battlesdk.graphics;

namespace battlesdk;
public static class ScreenManager {
    // TODO: Check potential cross-thread bugs.
    private static readonly Stack<IScreenLayer> _layers = [];
    private static readonly List<IScreenLayer> _renderedLayers = [];

    /// <summary>
    /// The renderer that works on the game's main window.
    /// </summary>
    public static Renderer? MainRenderer { get; private set; } = null;

    public static void Push (IScreenLayer layer) {
        _layers.Push(layer);
    }

    public static void Pop () {
        _layers.Pop();
    }

    public static void Draw () {
        _renderedLayers.Clear();
        foreach (var layer in _layers) {
            _renderedLayers.Add(layer);
            if (layer.IsTransparent == false) {
                break;
            }
        }

        for (int i = _renderedLayers.Count - 1; i >= 0; i--) {
            _renderedLayers[i].Draw();
        }
    }

    public static void SetMainRenderer (Renderer renderer) {
        MainRenderer = renderer;
    }
}

public interface IScreenLayer {
    /// <summary>
    /// If true, layers below this one won't be rendered.
    /// </summary>
    bool IsTransparent { get; }

    /// <summary>
    /// Draws this layer to the screen.
    /// </summary>
    void Draw ();
}
