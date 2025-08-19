using NLog;

namespace battlesdk;

/// <summary>
/// Contains the settings configured by the player in the game.
/// </summary>
public class GameSettings {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private string _messageFrame = "";
    private string _boxFrame = "";
    private string _font = "";

    /// <summary>
    /// The scale of the renderer.
    /// </summary>
    public float WindowScale { get; private set; } = 3f;
    /// <summary>
    /// If true, this setting overrides <see cref="WindowScale"/>.
    /// </summary>
    public bool IsFullScreen { get; private set; } = false;
    /// <summary>
    /// A value between 0 and 1 indicating the volume of background music.
    /// </summary>
    public float MusicVolume { get; private set; } = 0.32f;
    /// <summary>
    /// A value between 0 and 1 indicating the volume of sound effects.
    /// </summary>
    public float SoundVolume { get; private set; } = 1f;
    /// <summary>
    /// When false, moves don't have any animation during battle.
    /// </summary>
    public bool BattleAnimations { get; private set; } = true;
    /// <summary>
    /// When false, the player won't be asked to give a nickname to creatures
    /// they receive.
    /// </summary>
    public bool GiveNicknames { get; private set; } = true;

    /// <summary>
    /// The default frame to use for messages.
    /// </summary>
    public string MessageFrame {
        get {
            return _messageFrame;
        }
        private set {
            _messageFrame = value;
            if (Registry.Sprites.TryGetId(value, out var tb) == false) {
                _logger.Warn($"Sprite '{value}' is not in the Registry.");
            }
            else {
                MessageFrameId = tb;
            }
        }
    }
    public int MessageFrameId { get; private set; } = 0;

    /// <summary>
    /// The default frame to use for other kinds of boxes.
    /// </summary>
    public string BoxFrame {
        get {
            return _boxFrame;
        }
        private set {
            _boxFrame = value;
            if (Registry.Sprites.TryGetId(value, out var tb) == false) {
                _logger.Warn($"Sprite '{value}' is not in the Registry.");
            }
            else {
                BoxFrameId = tb;
            }
        }
    }
    public int BoxFrameId { get; private set; } = 0;

    /// <summary>
    /// The default font to use.
    /// </summary>
    public string Font {
        get {
            return _font;
        }
        private set {
            _font = value;
            if (Registry.Fonts.TryGetId(value, out var font) == false) {
                _logger.Warn($"Font '{value}' is not in the Registry.");
            }
            else {
                FontId = font;
            }
        }
    }

    public int FontId { get; private set; } = 0;

    private GameSettings () { }

    public static GameSettings Load () {
        GameSettings settings = new() {
            MessageFrame = "ui/frames/dp_textbox_1",
            BoxFrame = "ui/frames/dp_box",
            Font = "power_clear"
        };

        return settings;
    }

    public void SetWindowScale (float scale) {
        WindowScale = scale;
    }

    public void SetFullScreen (bool active) {
        IsFullScreen = active;
    }

    public void SetMusicVolume (float volume) {
        MusicVolume = volume;
    }

    public void SetSoundVolume (float volume) {
        SoundVolume = volume;
    }

    public void SetBattleAnimations (bool active) {
        BattleAnimations = active;
    }

    public void SetGiveNicknames (bool active) {
        GiveNicknames = active;
    }

    public void SetMessageFrame (string spriteName) {
        MessageFrame = spriteName;
    }

    public void SetBoxFrame (string spriteName) {
        BoxFrame = spriteName;
    }

    public void SetFont (string fontName) {
        Font = fontName;
    }
}
