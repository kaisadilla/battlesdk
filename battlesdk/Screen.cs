using battlesdk.graphics;
using battlesdk.screen;
using NLog;

namespace battlesdk;
public static class Screen {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    // TODO: Check potential cross-thread bugs.
    private static readonly Stack<IScreenLayer> _layers = [];
    private static readonly List<IScreenLayer> _renderedLayers = [];

    /// <summary>
    /// While true, the screen is black.
    /// </summary>
    private static bool _blackScreen = false;
    private static int _transitionDirection = 0;
    private static float _transitionDuration = 0f;
    private static float _transitionProgress = 0f;
    /// <summary>
    /// Maps script ids to transitions.
    /// </summary>
    private static Dictionary<int, ScriptTransition> _scriptTransitions = [];
    private static ITransition? _currentTransition = null;
    private static ITransition _backupTransition = null!;

    // Note: These fields will be null if you call them before calling Init().
    public static ScriptScreenLayer MainMenu { get; private set; } = null!;
    public static ScriptScreenLayer Bag { get; private set; } = null!;
    public static ScriptScreenLayer SaveGame { get; private set; } = null!;
    public static ScriptScreenLayer? Shop { get; private set; } = null!;

    /// <summary>
    /// The renderer that works on the game's main window.
    /// </summary>
    public static Renderer MainRenderer { get; private set; } = null!;

    public static void Init (Renderer renderer) {
        MainRenderer = renderer;

        if (Registry.Scripts.TryGetElementByName("screens/main_menu", out var mainMenuScr) == false) {
            throw new InitializationException("Missing script: 'screens/main_menu'.");
        }

        if (Registry.Scripts.TryGetElementByName("screens/bag", out var bagScr) == false) {
            throw new InitializationException("Missing script: 'screens/bag'.");
        }

        if (Registry.Scripts.TryGetElementByName("screens/save_game", out var saveGameScr) == false) {
            throw new InitializationException("Missing script: 'screens/save_game'.");
        }

        try {
            MainMenu = new ScriptScreenLayer(MainRenderer, mainMenuScr);
            Bag = new ScriptScreenLayer(MainRenderer, bagScr);
            SaveGame = new ScriptScreenLayer(MainRenderer, saveGameScr);

            if (Registry.Scripts.TryGetElementByName("screens/shop", out var shopScr)) {
                Shop = new ScriptScreenLayer(MainRenderer, shopScr);
            }
        }
        catch (Exception ex) {
            throw new InitializationException("Failed to load screen layer.", ex);
        }

        _backupTransition = new FadeTransition(MainRenderer);
    }

    public static void Push (IScreenLayer layer) {
        _layers.Push(layer);
        _logger.Debug($"Pushed screen layer: {layer.Name}.");
    }

    public static void Pop () {
        var discard = _layers.Pop();
        _logger.Debug($"Popped screen layer: {discard.Name}.");
    }

    public static void Update () {
        UpdateTransition();
    }

    public static void Draw () {
        if (_blackScreen) return;

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

        ApplyTransition();
    }

    private static unsafe void ApplyTransition () {
        if (_transitionProgress == 0) return;

        _currentTransition?.Draw(_transitionProgress);
    }

    private static void UpdateTransition () {
        if (_transitionDirection == 0) return;
        if (_transitionDirection == 1) {
            _transitionProgress += (1 / _transitionDuration) * Time.DeltaTime;

            if (_transitionProgress >= 1) {
                _blackScreen = true;
                _transitionProgress = 1;
                _transitionDirection = 0;
            }
        }
        else if (_transitionDirection == -1) {
            _transitionProgress -= (1 / _transitionDuration) * Time.DeltaTime;

            if (_transitionProgress <= 0) {
                _transitionProgress = 0;
                _transitionDirection = 0;
            }
        }
    }

    public static void PlayScriptTransition (int scriptId, float seconds, bool reverse) {
        if (MainRenderer is null) return; // TODO: !!!

        ITransition trans;
        // If the scriptId is less than zero, that script id can't exist, so we
        // use the backup script. This covers the convention of using -1 to
        // indicate no element.
        if (scriptId < 0) {
            trans = _backupTransition;
        }
        else {
            // If we have a transition cached with that id, use it.
            if (_scriptTransitions.TryGetValue(scriptId, out var scriptTrans)) {
                trans = scriptTrans;
            }
            // If we don't, and the script exists, create it.
            else if (Registry.Scripts.TryGetElement(scriptId, out var script)) {
                scriptTrans = new(
                    MainRenderer,
                    Registry.Scripts[scriptId]
                );
                _scriptTransitions[scriptId] = scriptTrans;
                trans = scriptTrans;
            }
            // If we don't and the script doesn't exist, use the backup.
            else {
                trans = _backupTransition;
            }
        }

        _currentTransition = trans;
        _transitionDirection = reverse ? -1 : 1;
        _transitionDuration = seconds;
        _blackScreen = false;
    }
}

public interface IScreenLayer {
    /// <summary>
    /// A name that identifies this screen.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// If true, layers below this one won't be rendered.
    /// </summary>
    bool IsTransparent { get; }

    /// <summary>
    /// Draws this layer to the screen.
    /// </summary>
    void Draw();
}
