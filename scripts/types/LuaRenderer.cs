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

    public LuaFrame? get_frame_or_default (DynValue arg) {
        if (arg.Type != DataType.String) {
            throw new ScriptRuntimeException("Invalid parameter type.");
        }

        string name = arg.String;

        if (Registry.FrameSprites.TryGetId(name, out int id) == false) {
            return null;
        }

        var frame = _renderer.GetFrameOrDefault(id);
        return new(frame);
    }

    public LuaFrame? get_frame (DynValue arg) {
        if (arg.Type != DataType.String) {
            throw new ScriptRuntimeException("Invalid parameter type.");
        }

        string name = arg.String;

        if (Registry.FrameSprites.TryGetId(name, out int id) == false) {
            return null;
        }

        var frame = _renderer.GetFrame(id);
        if (frame is null) return null;

        return new(frame);
    }
}
