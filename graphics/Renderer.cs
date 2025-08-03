using battlesdk.data;
using battlesdk.world;
using NLog;
using SDL;

namespace battlesdk.graphics;

public unsafe class Renderer {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private Dictionary<int, TilesetTexture> _tilesetTexes = [];
    private Dictionary<int, CharacterTexture> _charTexes = [];
    private Dictionary<int, StdTexture> _miscTexes = [];

    private int _width;
    private int _height;
    private float _scale;
    private Camera _camera;

    // These dictionaries map assets by their Registry id to Graphics elements
    // that are loaded into the SDL renderer. This class has methods to obtain
    // elements from these dictionaries, automatically creating new elements
    // if they don't already exist.
    private readonly Dictionary<int, GraphicsFont> _fonts = [];
    private readonly Dictionary<int, GraphicsTextboxTexture> _textboxes = [];

    public unsafe SDL_Renderer* SdlRenderer { get; private set; }

    public Renderer (SDL_Window* window, int width, int height, float scale) {
        SdlRenderer = SDL3.SDL_CreateRenderer(window, (string?)null);
        SDL3.SDL_SetDefaultTextureScaleMode(SdlRenderer, SDL_ScaleMode.SDL_SCALEMODE_NEAREST);

        _width = width;
        _height = height;
        _scale = scale;
        _camera = new(width, height, IVec2.Zero);

        EnableScale();

        foreach (var t in Registry.Tilesets) {
            LoadTileset(t);
        }

        foreach (var ch in Registry.CharSprites) {
            LoadCharacterSprite(ch);
        }

        foreach (var ch in Registry.MiscSprites) {
            LoadMiscSprite(ch);
        }

        Hud.Init(this);
    }

    public void EnableScale () {
        SDL3.SDL_SetRenderLogicalPresentation(
            SdlRenderer,
            _width,
            _height,
            SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_INTEGER_SCALE
        );
    }

    public void DisableScale () {
        SDL3.SDL_SetRenderLogicalPresentation(
            SdlRenderer,
            (int)(_width * _scale),
            (int)(_height * _scale),
            SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_INTEGER_SCALE
        );
    }


    private readonly List<GameMap> _visibleMaps = [];
    public unsafe void Render () {
        SDL3.SDL_SetRenderDrawColor(SdlRenderer, 0, 0, 0, 255);
        SDL3.SDL_RenderClear(SdlRenderer);

        _camera.SetPosition(TileToPixelSpace(G.World.Player.Subposition));

        List<GameMap> visibleMaps = [];
        foreach (var map in G.World.Maps) {
            if (_camera.IsRectVisible(TileToPixelSpace(map.WorldPos))) {
                visibleMaps.Add(map);
            }
        }
        Debug.InfoRenderedMaps = visibleMaps.Count;

        for (int i = 0; i < 8; i++) {
            foreach (var map in visibleMaps) {
                foreach (var l in map.Terrain) {
                    if (l.ZIndex == i) {
                        DrawLayer(l, map.WorldPos.Left, map.WorldPos.Top);
                    }
                }
            }

            List<Character> chars = [];

            foreach (var npc in G.World.Npcs) {
                if (npc.VisualZ == i) {
                    chars.Add(npc);
                }
            }
            if (G.World.Player.VisualZ == i) {
                chars.Add(G.World.Player);
            }

            chars.Sort((a, b) => a.Subposition.Y.CompareTo(b.Subposition.Y));

            foreach (var ch in chars) {
                DrawCharacter(ch);
            }
        }

        ApplyTimeTint();
        Hud.Draw();

        DisableScale();
        Debug.Draw(SdlRenderer);
        EnableScale();

        SDL3.SDL_RenderPresent(SdlRenderer);
    }

    public unsafe void Destroy () {
        SDL3.SDL_DestroyRenderer(SdlRenderer);
    }

    private unsafe void LoadTileset (Tileset tileset) {
        if (Registry.Tilesets.TryGetId(tileset.Name, out int id)) {
            TilesetTexture tex = new(SdlRenderer, tileset.TexturePath);
            _tilesetTexes[id] = tex;
        }
    }

    private unsafe void LoadCharacterSprite (AssetFile sprite) {
        if (Registry.CharSprites.TryGetId(sprite.Name, out int id)) {
            CharacterTexture tex = new(SdlRenderer, sprite);
            _charTexes[id] = tex;
        }
    }

    private unsafe void LoadMiscSprite (AssetFile sprite) {
        if (Registry.MiscSprites.TryGetId(sprite.Name, out int id)) {
            StdTexture tex = new(SdlRenderer, sprite);
            _miscTexes[id] = tex;
        }
    }

    private void DrawLayer (TileLayer layer, int xOffset, int yOffset) {
        for (int y = 0; y < layer.Height; y++) {
            int yPos = _camera.GetScreenY((y + yOffset) * Constants.TILE_SIZE);

            if (yPos + Constants.TILE_SIZE < 0) continue;
            if (yPos >= _height) continue;

            for (int x = 0; x < layer.Width; x++) {
                int xPos = _camera.GetScreenX((x + xOffset) * Constants.TILE_SIZE);

                if (xPos + Constants.TILE_SIZE < 0) continue;
                if (xPos >= _width) continue;

                MapTile? tile = layer[x, y];

                if (tile is null) continue;

                var ts = Registry.Tilesets[tile.TilesetId];
                var tex = _tilesetTexes[tile.TilesetId];

                SDL_FRect src = new() {
                    x = (tile.TileId % ts.Width) * Constants.TILE_SIZE,
                    y = (tile.TileId / ts.Width) * Constants.TILE_SIZE,
                    h = Constants.TILE_SIZE,
                    w = Constants.TILE_SIZE,
                };

                SDL_FRect dst = new() {
                    x = xPos,
                    y = yPos,
                    h = Constants.TILE_SIZE,
                    w = Constants.TILE_SIZE,
                };

                unsafe {
                    SDL3.SDL_RenderTexture(SdlRenderer, tex.Texture, &src, &dst);
                }
            }
        }
    }

    private void DrawTile (MapTile tile, int screenX, int screenY) {
        var ts = Registry.Tilesets[tile.TilesetId];
        var tex = _tilesetTexes[tile.TilesetId];

        SDL_FRect src = new() {
            x = (tile.TileId % ts.Width) * Constants.TILE_SIZE,
            y = (tile.TileId / ts.Width) * Constants.TILE_SIZE,
            h = Constants.TILE_SIZE,
            w = Constants.TILE_SIZE,
        };

        SDL_FRect dst = new() {
            x = screenX,
            y = screenY,
            h = Constants.TILE_SIZE,
            w = Constants.TILE_SIZE,
        };

        unsafe {
            SDL3.SDL_RenderTexture(SdlRenderer, tex.Texture, &src, &dst);
        }
    }

    private void DrawCharacter (Character character) {
        int frame = 0;
        if (
            character.IsMoving
            && character.MoveProgress >= 0.25f
            && character.MoveProgress < 0.75f
        ) {
            frame = 1 + (character.MoveCount % 2);
        }

        var subpos = character.Subposition;

        if (character.IsJumping) {
            if (character.MoveProgress < 0.5) {
                subpos += new Vec2(0, -2 * character.MoveProgress);
            }
            else {
                subpos += new Vec2(0, -2 * (1 - character.MoveProgress));
            }
        }

        unsafe {
            if (Registry.CharSpriteShadow != -1) {
                _miscTexes[Registry.CharSpriteShadow].Draw(
                    SdlRenderer,
                    _camera.GetScreenPos(TileToPixelSpace(character.Subposition)) + new IVec2(0, 8)
                );
            }

            _charTexes[character.Sprite].Draw(
                SdlRenderer,
                _camera.GetScreenPos(TileToPixelSpace(subpos)),
                character.Direction,
                (character.IsMoving && !character.IsJumping && character.IsRunning) ? 1 : 0,
                frame
            );
        }
    }

    /// <summary>
    /// Applies the day/night tint to everything that's drawn to the screen so
    /// far, if day/night tint is enabled and properly configured.
    /// </summary>
    private void ApplyTimeTint () {
        // If time tints are not defined, then they aren't applied.
        if (Data.Misc.TimeTints is null) return;

        int minutes = Time.RealMinutes();
        int hour = minutes / 60;
        float progress = (minutes % 60) / 60f;

        var thisTint = Data.Misc.TimeTints[hour % 24];
        var nextTint = Data.Misc.TimeTints[(hour + 1) % 24];

        var tint = new ColorRGB() {
            R = (int)thisTint.R.Lerp(nextTint.R, progress),
            G = (int)thisTint.G.Lerp(nextTint.G, progress),
            B = (int)thisTint.B.Lerp(nextTint.B, progress),
        };

        SDL3.SDL_SetRenderDrawBlendMode(SdlRenderer, CustomBlendModes.Subtract);
        SDL3.SDL_SetRenderDrawColor(
            SdlRenderer,
            (byte)Math.Max(0, -tint.R),
            (byte)Math.Max(0, -tint.G),
            (byte)Math.Max(0, -tint.B),
            255
        );
        SDL3.SDL_RenderFillRect(SdlRenderer, null);

        SDL3.SDL_SetRenderDrawBlendMode(SdlRenderer, CustomBlendModes.Add);
        SDL3.SDL_SetRenderDrawColor(
            SdlRenderer,
            (byte)Math.Max(0, tint.R),
            (byte)Math.Max(0, tint.G),
            (byte)Math.Max(0, tint.B),
            255
        );
        SDL3.SDL_RenderFillRect(SdlRenderer, null);
    }

    #region GPU asset getters
    /// <summary>
    /// Obtains the drawable font with the given id, loading it in the renderer
    /// if it's not already loaded.
    /// </summary>
    /// <param name="id">The font's id.</param>
    public GraphicsFont? GetFont (int id) {
        if (_fonts.TryGetValue(id, out var font)) {
            return font;
        }

        if (Registry.Fonts.TryGetElement(id, out var fontAsset) == false) {
            return null;
        }

        _fonts[id] = new(SdlRenderer, fontAsset);
        return _fonts[id];
    }

    public GraphicsFont GetFontOrDefault (int id) {
        return GetFont(id) ?? GetFont(0) ?? throw new("No font available.");
    }

    public GraphicsTextboxTexture? GetTextbox (int id) {
        if (_textboxes.TryGetValue(id, out var textbox)) {
            return textbox;
        }

        if (Registry.TextboxSprites.TryGetElement(id, out var tbAsset) == false) {
            return null;
        }

        _textboxes[id] = new(SdlRenderer, tbAsset);
        return _textboxes[id];
    }

    public GraphicsTextboxTexture GetTextboxOrDefault (int id) {
        return GetTextbox(id) ?? GetTextbox(0) ?? throw new("No textbox available.");
    }
    #endregion
}
