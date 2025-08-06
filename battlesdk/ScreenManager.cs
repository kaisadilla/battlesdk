using battlesdk.graphics;
using battlesdk.screen;
using NLog;

namespace battlesdk;
public static class ScreenManager {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    // TODO: Check potential cross-thread bugs.
    private static readonly Stack<IScreenLayer> _layers = [];
    private static readonly List<IScreenLayer> _renderedLayers = [];

    // Note: These fields will be null if you call them before calling Init().
    public static ScriptScreenLayer MainMenu { get; private set; } = null!;

    /// <summary>
    /// The renderer that works on the game's main window.
    /// </summary>
    public static Renderer? MainRenderer { get; private set; } = null;

    public static void Init (Renderer renderer) {
        MainRenderer = renderer;

        if (Registry.Scripts.TryGetElementByName("screens/main_menu", out var scr) == false) {
            throw new InitializationException(
                "ScreenManager - Missing script: 'screens/main_menu'."
            );
        }

        try {
            MainMenu = new ScriptScreenLayer(MainRenderer, scr);
        }
        catch (Exception ex) {
            throw new InitializationException(
                "ScreenManager - Failed to load main menu's script.", ex
            );
        }

    }

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
