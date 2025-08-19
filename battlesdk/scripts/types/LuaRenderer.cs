using battlesdk.graphics;
using battlesdk.graphics.elements;
using battlesdk.hud;
using MoonSharp.Interpreter;
using NLog;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaRenderer : ILuaType {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    [MoonSharpHidden]
    public const string CLASSNAME = "Renderer";

    [MoonSharpHidden]
    private readonly Renderer _renderer;

    /// <summary>
    /// The viewport's logical width when scale = 1.
    /// </summary>
    public int width => _renderer.Width;
    /// <summary>
    /// The viewport's logical height when scale = 1.
    /// </summary>
    public int height => _renderer.Height;

    [MoonSharpHidden]
    public LuaRenderer (Renderer renderer) {
        _renderer = renderer;
    }

    /// <summary>
    /// Returns the sprite with the name given.
    /// </summary>
    /// <param name="arg">The name of the sprite.</param>
    public LuaSprite? get_sprite (DynValue arg) {
        if (arg.Type != DataType.String) {
            throw new ScriptRuntimeException("Invalid parameter type.");
        }
        string name = arg.String;

        if (Registry.Sprites.TryGetId(name, out int id) == false) {
            var lastSlash = name.LastIndexOf('/');
            if (lastSlash == -1) return null;

            var parentName = name[..lastSlash];
            var subspriteName = name[(lastSlash + 1)..];

            if (Registry.Sprites.TryGetId(parentName.ToString(), out int parentId) == false) {
                return null;
            }

            var sheetSprite = _renderer.GetSheetSprite(parentId, subspriteName);
            if (sheetSprite is null) return null;

            return new(sheetSprite);
        }

        var sprite = _renderer.GetSpriteOrNull(id);
        if (sprite is null) return null;

        return new(sprite);
    }

    /// <summary>
    /// Returns the sprite with the name given as a FrameSprite, or null if no
    /// such sprite exists, or if it's not a frame.
    /// </summary>
    /// <param name="arg">The name of the sprite.</param>
    public LuaFrameSprite? get_frame (DynValue arg) {
        if (arg.Type != DataType.String) {
            throw new ScriptRuntimeException("Invalid parameter type.");
        }
        string name = arg.String;

        if (Registry.Sprites.TryGetId(name, out int id) == false) {
            return null;
        }

        var frame = _renderer.GetFrame(id);
        if (frame is null) return null;

        return new(frame);
    }

    /// <summary>
    /// Returns the default frame for messages. This function can only be
    /// called during gameplay.
    /// </summary>
    public LuaFrameSprite? get_default_message_frame () {
        var frame = _renderer.GetFrame(G.GameOptions.MessageFrameId);
        if (frame is null) throw new("No default message frame exists.");

        return new(frame);
    }

    /// <summary>
    /// Returns the default frame for boxes. This function can only be called
    /// during gameplay.
    /// </summary>
    public LuaFrameSprite? get_default_box_frame () {
        var frame = _renderer.GetFrame(G.GameOptions.BoxFrameId);
        if (frame is null) throw new("No default box frame exists.");

        return new(frame);
    }

    /// <summary>
    /// Returns the font with the name given, or null if no such font exists.
    /// </summary>
    /// <param name="name">The name of the font.</param>
    public LuaFont? get_font (string name) {
        if (Registry.Fonts.TryGetId(name, out int id) == false) {
            return null;
        }

        var font = _renderer.GetFont(id);
        if (font is null) return null;

        return new(font);
    }

    /// <summary>
    /// Returns the default text font of the game. This function can only be
    /// called during gameplay.
    /// </summary>
    public LuaFont get_default_font () {
        var font = _renderer.GetFont(G.GameOptions.FontId);
        if (font is null) throw new("No default font exists.");

        return new(font);
    }

    public LuaTextbox? get_textbox (
        string frame,
        string font,
        LuaVec2 pos,
        LuaVec2 size,
        string text
    ) {
        if (Registry.Sprites.TryGetId(frame, out int frameId) == false) {
            _logger.Error($"Sprite does not exist: '{frame}'.");
            return null;
        }
        if (Registry.Fonts.TryGetId(font, out int fontId) == false) {
            _logger.Error($"Font does not exist: '{font}'.");
            return null;
        }

        return new(new Textbox(
            _renderer,
            frameId,
            fontId,
            pos.ToIVec2(),
            size.ToIVec2(),
            text
        ));
    }

    public LuaAnimatableTextbox? get_animatable_textbox (
        string frame,
        string font,
        LuaVec2 pos,
        LuaVec2 size,
        string text
    ) {
        if (Registry.Sprites.TryGetId(frame, out int frameId) == false) {
            _logger.Error($"Sprite does not exist: '{frame}'.");
            return null;
        }
        if (Registry.Fonts.TryGetId(font, out int fontId) == false) {
            _logger.Error($"Font does not exist: '{font}'.");
            return null;
        }

        return new(new AnimatableTextbox(
            _renderer,
            frameId,
            fontId,
            pos.ToIVec2(),
            size.ToIVec2(),
            text
        ));
    }

    public LuaChoiceBox? get_choice_box (
        string frame,
        string font, 
        LuaVec2 pos,
        int anchor,
        List<string> choices
    ) {
        if (Registry.Sprites.TryGetId(frame, out int frameId) == false) {
            _logger.Error($"Sprite does not exist: '{frame}'.");
            return null;
        }
        if (Registry.Fonts.TryGetId(font, out int fontId) == false) {
            _logger.Error($"Font does not exist: '{font}'.");
            return null;
        }

        return new(new(
            _renderer,
            frameId,
            fontId,
            pos.ToIVec2(),
            (AnchorPoint)anchor,
            choices
        ));
    }

    public LuaScrollbar get_scrollbar (int width, float value) {
        return new(new(_renderer, width, value));
    }

    public LuaScriptGraphicElement? get_script_element (string script_name) {
        if (Registry.Scripts.TryGetElementByName(script_name, out var script) == false) {
            throw new ScriptRuntimeException("Invalid script name.");
        }

        return new(new(_renderer, script, new()));
    }

    /// <summary>
    /// Gets a message hud element that isn't controlled by the Hud.
    /// </summary>
    /// <param name="frame">The name of the frame to use.</param>
    /// <param name="font">The name of the font to use.</param>
    /// <param name="text">The text contained in the message box.</param>
    public LuaMessageHudElement? get_message_hud_element (
        string frame, string font, string text
    ) {
        if (Registry.Sprites.TryGetId(frame, out int frameId) == false) {
            _logger.Error($"Sprite does not exist: '{frame}'.");
            return null;
        }
        if (Registry.Fonts.TryGetId(font, out int fontId) == false) {
            _logger.Error($"Font does not exist: '{font}'.");
            return null;
        }

        return new(new(_renderer, frameId, fontId, text));
    }

    public LuaScriptHudElement? get_script_hud_element (
        string script_name, LuaObject? args = null
    ) {
        if (Registry.Scripts.TryGetElementByName(script_name, out var script) == false) {
            throw new ScriptRuntimeException("Invalid script name.");
        }

        var el = new ScriptHudElement(_renderer, script);
        el.Open(args ?? new());

        return new(el);
    }

    /// <summary>
    /// Paints the entire screen on the color given.
    /// </summary>
    /// <param name="color">The color to use.</param>
    public void paint_screen (LuaColor color) {
        IVec2 size = new(width, height);

        _renderer.DrawRectangle(IVec2.Zero, size, color.ToNative());
    }

    /// <summary>
    /// Draws a rectangle with the parameters given.
    /// </summary>
    /// <param name="pos">The position of the top-left corner.</param>
    /// <param name="size">The rectangle's size.</param>
    /// <param name="color">The rectangle's color.</param>
    public void draw_rectangle (LuaVec2 pos, LuaVec2 size, LuaColor color) {
        _renderer.DrawRectangle(pos.ToIVec2(), size.ToIVec2(), color.ToNative());
    }

    public override string ToString () {
        return $"[Renderer]";
    }

    public string str () => ToString();
}
