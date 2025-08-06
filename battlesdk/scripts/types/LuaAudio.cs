using MoonSharp.Interpreter;
using NLog;

namespace battlesdk.scripts.types;

public class LuaAudio {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    [MoonSharpHidden]
    public const string CLASSNAME = "Audio";

    public static void play (DynValue sound_name) {
        if (sound_name.Type != DataType.String) {
            throw new ScriptRuntimeException("Invalid parameter type.");
        }

        if (Registry.Sounds.TryGetId(sound_name.String, out int id) == false) {
            _logger.Error($"Sound '{sound_name}' doesn't exist.");
            return;
        }

        Audio.Play(id);
    }

    public static void play_beep_short () {
        Audio.PlayBeepShort();
    }
}
