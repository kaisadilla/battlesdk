using battlesdk.graphics;
using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

public class LuaRenderer : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "Renderer";

    [MoonSharpHidden]
    private readonly Renderer _renderer;

    public int width => _renderer.Width;
    public int height => _renderer.Height;

    [MoonSharpHidden]
    public LuaRenderer (Renderer renderer) {
        _renderer = renderer;
    }

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

    public string? to_string () {
        return $"<renderer>";
    }
}
