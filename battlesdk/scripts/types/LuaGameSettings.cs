using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;
[LuaApiClass]
public class LuaGameSettings : ILuaType {
    [MoonSharpHidden]
    public const string CLASSNAME = "GameSettings";

    [MoonSharpHidden]
    private readonly GameSettings _settings;

    /// <summary>
    /// The scale of the renderer.
    /// </summary>
    public float Window_scale => _settings.WindowScale;
    /// <summary>
    /// True when the game is in full screen. This setting has priority over
    /// 'window_scale'.
    /// </summary>
    public bool is_full_screen => _settings.IsFullScreen;
    /// <summary>
    /// A value between 0 and 1 indicating the volume of background music.
    /// </summary>
    public float Music_volume => _settings.MusicVolume;
    /// <summary>
    /// A value between 0 and 1 indicating the volume of sound effects.
    /// </summary>
    public float Sound_volume => _settings.SoundVolume;
    /// <summary>
    /// When false, moves don't have any animation during battle.
    /// </summary>
    public bool Battle_animations => _settings.BattleAnimations;
    /// <summary>
    /// When false, the player won't be asked to give a nickname to creatures
    /// they receive.
    /// </summary>
    public bool Give_nicknames => _settings.GiveNicknames;
    /// <summary>
    /// The default frame to use for messages.
    /// </summary>
    public string Message_frame => _settings.MessageFrame;
    /// <summary>
    /// The default frame to use for other kinds of boxes.
    /// </summary>
    public string Box_frame => _settings.BoxFrame;
    /// <summary>
    /// The default font to use.
    /// </summary>
    [MoonSharpProperty]
    public string Font => _settings.Font;

    [MoonSharpHidden]
    public LuaGameSettings (GameSettings settings) {
        _settings = settings;
    }

    public void set_window_scale (float scale) {
        _settings.SetWindowScale(scale);
    }

    public void set_full_screen (bool active) {
        _settings.SetFullScreen(active);
    }

    public void set_music_volume (float volume) {
        _settings.SetMusicVolume(volume);
    }

    public void set_sound_volume (float volume) {
        _settings.SetSoundVolume(volume);
    }

    public void set_battle_animations (bool active) {
        _settings.SetBattleAnimations(active);
    }

    public void set_give_nicknames (bool active) {
        _settings.SetGiveNicknames(active);
    }

    public void set_message_frame (string sprite_name) {
        _settings.SetMessageFrame(sprite_name);
    }

    public void set_box_frame (string sprite_name) {
        _settings.SetBoxFrame(sprite_name);
    }

    public void set_font (string font_name) {
        _settings.SetFont(font_name);
    }

    public override string ToString () {
        return $"[GameSettings]";
    }

    public string str () => ToString();
}
