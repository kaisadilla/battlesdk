using battlesdk.graphics;
using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaRenderer : ILuaType {
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

        var sprite = _renderer.GetSprite(id);
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
    /// Returns the default text font of the game.
    /// </summary>
    public LuaFont get_default_font () {
        var font = _renderer.GetFont(Settings.TextFont);
        if (font is null) throw new("No default font exists.");

        return new(font);
    }

    public override string ToString () {
        return $"<renderer>";
    }
}
