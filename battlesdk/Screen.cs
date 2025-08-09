using battlesdk.graphics;
using battlesdk.screen;
using NLog;
using SDL;

namespace battlesdk;
public static class Screen {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    // TODO: Check potential cross-thread bugs.
    private static readonly Stack<IScreenLayer> _layers = [];
    private static readonly List<IScreenLayer> _renderedLayers = [];

    private static int _fadeDirection = 0;
    private static float _fadeDuration = 0f;
    private static float _fadeProgress = 0f;

    // Note: These fields will be null if you call them before calling Init().
    public static ScriptScreenLayer MainMenu { get; private set; } = null!;
    public static ScriptScreenLayer SaveGame { get; private set; } = null!;

    /// <summary>
    /// The renderer that works on the game's main window.
    /// </summary>
    public static Renderer? MainRenderer { get; private set; } = null;

    public static void Init (Renderer renderer) {
        MainRenderer = renderer;

        if (Registry.Scripts.TryGetElementByName("screens/main_menu", out var mainMenuScr) == false) {
            throw new InitializationException("Missing script: 'screens/main_menu'.");
        }

        if (Registry.Scripts.TryGetElementByName("screens/save_game", out var saveGameScr) == false) {
            throw new InitializationException("Missing script: 'screens/save_game'.");
        }

        try {
            MainMenu = new ScriptScreenLayer(MainRenderer, mainMenuScr);
            SaveGame = new ScriptScreenLayer(MainRenderer, saveGameScr);
        }
        catch (Exception ex) {
            throw new InitializationException("Failed to load screen layer.", ex);
        }
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
        UpdateFade();
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

        if (MainRenderer is not null) {
            ApplyFadeToBlack(MainRenderer);
        }
    }

    private static unsafe void ApplyFadeToBlack (Renderer renderer) {
        if (_fadeProgress == 0) return;

        SDL3.SDL_SetRenderDrawBlendMode(renderer.SdlRenderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);

        SDL3.SDL_SetRenderDrawColor(
            renderer.SdlRenderer, 0, 0, 0, (byte)(255 * _fadeProgress)
        );
        SDL3.SDL_RenderFillRect(renderer.SdlRenderer, null);
    }

    private static void UpdateFade () {
        if (_fadeDirection == 0) return;
        if (_fadeDirection == 1) {
            _fadeProgress += (1 / _fadeDuration) * Time.DeltaTime;

            if (_fadeProgress >= 1) {
                _fadeProgress = 1;
                _fadeDirection = 0;
            }
        }
        else if (_fadeDirection == -1) {
            _fadeProgress -= (1 / _fadeDuration) * Time.DeltaTime;

            if (_fadeProgress <= 0) {
                _fadeProgress = 0;
                _fadeDirection = 0;
            }
        }
    }

    public static void FadeToBlack (float seconds) {
        _fadeDirection = 1;
        _fadeDuration = seconds;
    }

    public static void FadeFromBlack (float seconds) {
        _fadeDirection = -1;
        _fadeDuration = seconds;
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
