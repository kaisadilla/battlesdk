using battlesdk.data;
using NLog;
using SDL;

namespace battlesdk.graphics;

public unsafe class Renderer {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    // These dictionaries map assets by their Registry id to Graphics elements
    // that are loaded into the SDL renderer. This class has methods to obtain
    // elements from these dictionaries, automatically creating new elements
    // if they don't already exist.
    private readonly Dictionary<int, GraphicsFont> _fonts = [];
    private readonly Dictionary<int, GraphicsTileset> _tilesets = [];
    private readonly Dictionary<int, IGraphicsSprite> _sprites = [];
    private readonly Dictionary<int, GraphicsSprite> _spriteAtlases = [];
    private readonly Dictionary<int, Dictionary<string, GraphicsAtlasSprite>> _spritesheetSprites = [];

    /// <summary>
    /// The SDL Renderer managed by this instance.
    /// </summary>
    public unsafe SDL_Renderer* SdlRenderer { get; private set; }
    /// <summary>
    /// The viewport's logical width (assuming scale = 1).
    /// </summary>
    public int Width { get; private set; }
    /// <summary>
    /// The viewport's logical height (assuming scale = 1).
    /// </summary>
    public int Height { get; private set; }
    /// <summary>
    /// The scale of the viewport. The width and height of the window, as well
    /// as everything drawn into it, will be scaled by this value.
    /// </summary>
    public float Scale { get; private set; }

    public Renderer (SDL_Window* window, int width, int height, float scale) {
        SdlRenderer = SDL3.SDL_CreateRenderer(window, (string?)null);
        SDL3.SDL_SetDefaultTextureScaleMode(SdlRenderer, SDL_ScaleMode.SDL_SCALEMODE_NEAREST);

        Width = width;
        Height = height;
        Scale = scale;

        EnableScale();

        Hud.Init(this);
    }

    /// <summary>
    /// Enables scaling in this render. This means that everything drawn to the
    /// screen will be multiplied by this renderer's scale.
    /// </summary>
    public void EnableScale () {
        SDL3.SDL_SetRenderLogicalPresentation(
            SdlRenderer,
            Width,
            Height,
            SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_INTEGER_SCALE
        );
    }

    /// <summary>
    /// Disables scaling in this render. This means that everything drawn to the
    /// screen will have a scale of 1, regardless of the renderer's scale.
    /// </summary>
    public void DisableScale () {
        SDL3.SDL_SetRenderLogicalPresentation(
            SdlRenderer,
            (int)(Width * Scale),
            (int)(Height * Scale),
            SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_INTEGER_SCALE
        );
    }

    public unsafe void Render () {
        SDL3.SDL_SetRenderDrawColor(SdlRenderer, 0, 0, 0, 255);
        SDL3.SDL_RenderClear(SdlRenderer);

        ScreenManager.Draw();
        Hud.Draw();

        DisableScale();
        Debug.Draw(SdlRenderer);
        EnableScale();

        SDL3.SDL_RenderPresent(SdlRenderer);
    }

    public unsafe void Destroy () {
        SDL3.SDL_DestroyRenderer(SdlRenderer);
    }

    #region GPU asset getters
    /// <summary>
    /// Obtains the drawable font with the given id, loading it in the renderer
    /// if it's not already loaded.
    /// </summary>
    /// <param name="id">The font's id.</param>
    public GraphicsFont? GetFont (int id) {
        return GetGraphicTexture(
            Registry.Fonts,
            _fonts,
            id,
            asset => new(this, asset)
        );
    }

    public GraphicsFont GetFontOrDefault (int id) {
        return GetFont(id) ?? GetFont(0) ?? throw new("No font available.");
    }

    public IGraphicsSprite? GetSprite (int id) {
        if (_sprites.TryGetValue(id, out var sprite)) {
            return sprite;
        }

        if (Registry.Sprites.TryGetElement(id, out var asset) == false) {
            return null;
        }

        else {
            _sprites[id] = GraphicsSprite.New(this, asset);
        }
        
        return _sprites[id];
    }

    /// <summary>
    /// Returns the frame with the id given if that sprite exists and it's a
    /// frame.
    /// </summary>
    /// <param name="id">An id of a sprite that is expected to be a frame.</param>
    public GraphicsFrameSprite? GetFrame (int id) {
        var sprite = GetSprite(id);

        if (sprite is null) return null;

        if (sprite is not GraphicsFrameSprite frameSprite) {
            _logger.Error($"Sprite #{id} is not a frame.");
            return null;
        }

        return frameSprite;
    }

    public GraphicsAtlasSprite? GetSheetSprite (int id, string subsprite) {
        if (_spritesheetSprites.TryGetValue(id, out var dict) == false) {
            dict = _spritesheetSprites[id] = [];
        }

        if (dict.TryGetValue(subsprite, out var sprite)) {
            return sprite;
        }

        if (
            Registry.Sprites.TryGetElement(id, out var asset) == false
            || asset is not SpritesheetFile ssAsset
        ) {
            return null;
        }

        dict[subsprite] = new(this, ssAsset, subsprite);
        return dict[subsprite];
    }

    public GraphicsTileset? GetTileset (int id) {
        return GetGraphicTexture(
            Registry.Tilesets,
            _tilesets,
            id,
            asset => new(SdlRenderer, asset.TexturePath)
        );
    }

    private static TGraphics? GetGraphicTexture<TGraphics, TAsset> (
        Collection<TAsset> assetCol,
        Dictionary<int, TGraphics> dictionary,
        int id,
        Func<TAsset, TGraphics> buildRes
    ) where TAsset : IIdentifiable {
        if (dictionary.TryGetValue(id, out var el)) {
            return el;
        }

        if (assetCol.TryGetElement(id, out var asset) == false) {
            return default;
        }

        dictionary[id] = buildRes(asset);
        return dictionary[id];
    }
    #endregion
}
