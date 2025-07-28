using Hexa.NET.SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battlesdk;

// TODO.
file static class _Input {
    /*
     * This class handles input by using SDL's scancodes rather than keycodes.
     * Scancodes reference the physical key that was pressed by the player,
     * regardless of the character associated to that key by their keyboard
     * layout. This allows input FOR CONTROLS to be agnostic to the player's
     * keyboard language.
     */

    /// <summary>
    /// The key codes that are being pressed this frame.
    /// </summary>
    private static readonly HashSet<SDLScancode> _pressedCodes = [];
    /// <summary>
    /// The key codes that were pressed the last frame.
    /// </summary>
    private static readonly HashSet<SDLScancode> _lastCodes = [];

    public static void Update () {
        // Each frame, we update the list of codes pressed last frame with
        // what is currently in the list. We do not delete the list for this
        // frame, as the key may still be pressed.
        _lastCodes.Clear();

        foreach (var key in _pressedCodes) {
            _lastCodes.Add(key);
        }
    }

    public static void RegisterEvent (SDLEvent evt) {
        if (evt.Type == (uint)SDLEventType.Keydown && evt.Key.Repeat == 0) {
            _pressedCodes.Add(evt.Key.Keysym.Scancode);
        }
        else if (evt.Type == (uint)SDLEventType.Keyup) {
            _pressedCodes.Remove(evt.Key.Keysym.Scancode);
        }
    }

    public static bool GetKeyDown (KeyCode key) {
        // A key was pressed down this frame if it's pressed this frame but
        // was not pressed last frame.

        var sdlCode = (SDLScancode)key;
        return _pressedCodes.Contains(sdlCode) && _lastCodes.Contains(sdlCode) == false;
    }

    public static bool GetKey (KeyCode key) {
        return _pressedCodes.Contains((SDLScancode)key);
    }

    public static bool GetKeyUp (KeyCode key) {
        // A key was released this frame if it's not pressed this frame but was
        // pressed last frame.

        var sdlCode = (SDLScancode)key;
        return _pressedCodes.Contains(sdlCode) == false && _lastCodes.Contains(sdlCode);
    }
}

public enum KeyCode {
    Unknown = SDLScancode.Unknown,
    A = SDLScancode.A,
    B = SDLScancode.B,
    C = SDLScancode.C,
    D = SDLScancode.D,
    E = SDLScancode.E,
    F = SDLScancode.F,
    G = SDLScancode.G,
    H = SDLScancode.H,
    I = SDLScancode.I,
    J = SDLScancode.J,
    K = SDLScancode.K,
    L = SDLScancode.L,
    M = SDLScancode.M,
    N = SDLScancode.N,
    O = SDLScancode.O,
    P = SDLScancode.P,
    Q = SDLScancode.Q,
    R = SDLScancode.R,
    S = SDLScancode.S,
    T = SDLScancode.T,
    U = SDLScancode.U,
    V = SDLScancode.V,
    W = SDLScancode.W,
    X = SDLScancode.X,
    Y = SDLScancode.Y,
    Z = SDLScancode.Z,
    Scancode1 = SDLScancode.Scancode1,
    Scancode2 = SDLScancode.Scancode2,
    Scancode3 = SDLScancode.Scancode3,
    Scancode4 = SDLScancode.Scancode4,
    Scancode5 = SDLScancode.Scancode5,
    Scancode6 = SDLScancode.Scancode6,
    Scancode7 = SDLScancode.Scancode7,
    Scancode8 = SDLScancode.Scancode8,
    Scancode9 = SDLScancode.Scancode9,
    Scancode0 = SDLScancode.Scancode0,
    Return = SDLScancode.Return,
    Escape = SDLScancode.Escape,
    Backspace = SDLScancode.Backspace,
    Tab = SDLScancode.Tab,
    Space = SDLScancode.Space,
    Minus = SDLScancode.Minus,
    Equals = SDLScancode.Equals,
    Leftbracket = SDLScancode.Leftbracket,
    Rightbracket = SDLScancode.Rightbracket,
    Backslash = SDLScancode.Backslash,
    Nonushash = SDLScancode.Nonushash,
    Semicolon = SDLScancode.Semicolon,
    Apostrophe = SDLScancode.Apostrophe,
    Grave = SDLScancode.Grave,
    Comma = SDLScancode.Comma,
    Period = SDLScancode.Period,
    Slash = SDLScancode.Slash,
    Capslock = SDLScancode.Capslock,
    F1 = SDLScancode.F1,
    F2 = SDLScancode.F2,
    F3 = SDLScancode.F3,
    F4 = SDLScancode.F4,
    F5 = SDLScancode.F5,
    F6 = SDLScancode.F6,
    F7 = SDLScancode.F7,
    F8 = SDLScancode.F8,
    F9 = SDLScancode.F9,
    F10 = SDLScancode.F10,
    F11 = SDLScancode.F11,
    F12 = SDLScancode.F12,
    Printscreen = SDLScancode.Printscreen,
    Scrolllock = SDLScancode.Scrolllock,
    Pause = SDLScancode.Pause,
    Insert = SDLScancode.Insert,
    Home = SDLScancode.Home,
    Pageup = SDLScancode.Pageup,
    Delete = SDLScancode.Delete,
    End = SDLScancode.End,
    Pagedown = SDLScancode.Pagedown,
    Right = SDLScancode.Right,
    Left = SDLScancode.Left,
    Down = SDLScancode.Down,
    Up = SDLScancode.Up,
    Numlockclear = SDLScancode.Numlockclear,
    KpDivide = SDLScancode.KpDivide,
    KpMultiply = SDLScancode.KpMultiply,
    KpMinus = SDLScancode.KpMinus,
    KpPlus = SDLScancode.KpPlus,
    KpEnter = SDLScancode.KpEnter,
    Kp1 = SDLScancode.Kp1,
    Kp2 = SDLScancode.Kp2,
    Kp3 = SDLScancode.Kp3,
    Kp4 = SDLScancode.Kp4,
    Kp5 = SDLScancode.Kp5,
    Kp6 = SDLScancode.Kp6,
    Kp7 = SDLScancode.Kp7,
    Kp8 = SDLScancode.Kp8,
    Kp9 = SDLScancode.Kp9,
    Kp0 = SDLScancode.Kp0,
    KpPeriod = SDLScancode.KpPeriod,
    Nonusbackslash = SDLScancode.Nonusbackslash,
    Application = SDLScancode.Application,
    Power = SDLScancode.Power,
    KpEquals = SDLScancode.KpEquals,
    F13 = SDLScancode.F13,
    F14 = SDLScancode.F14,
    F15 = SDLScancode.F15,
    F16 = SDLScancode.F16,
    F17 = SDLScancode.F17,
    F18 = SDLScancode.F18,
    F19 = SDLScancode.F19,
    F20 = SDLScancode.F20,
    F21 = SDLScancode.F21,
    F22 = SDLScancode.F22,
    F23 = SDLScancode.F23,
    F24 = SDLScancode.F24,
    Execute = SDLScancode.Execute,
    Help = SDLScancode.Help,
    Menu = SDLScancode.Menu,
    Select = SDLScancode.Select,
    Stop = SDLScancode.Stop,
    Again = SDLScancode.Again,
    Undo = SDLScancode.Undo,
    Cut = SDLScancode.Cut,
    Copy = SDLScancode.Copy,
    Paste = SDLScancode.Paste,
    Find = SDLScancode.Find,
    Mute = SDLScancode.Mute,
    Volumeup = SDLScancode.Volumeup,
    Volumedown = SDLScancode.Volumedown,
    KpComma = SDLScancode.KpComma,
    KpEqualsas400 = SDLScancode.KpEqualsas400,
    International1 = SDLScancode.International1,
    International2 = SDLScancode.International2,
    International3 = SDLScancode.International3,
    International4 = SDLScancode.International4,
    International5 = SDLScancode.International5,
    International6 = SDLScancode.International6,
    International7 = SDLScancode.International7,
    International8 = SDLScancode.International8,
    International9 = SDLScancode.International9,
    Lang1 = SDLScancode.Lang1,
    Lang2 = SDLScancode.Lang2,
    Lang3 = SDLScancode.Lang3,
    Lang4 = SDLScancode.Lang4,
    Lang5 = SDLScancode.Lang5,
    Lang6 = SDLScancode.Lang6,
    Lang7 = SDLScancode.Lang7,
    Lang8 = SDLScancode.Lang8,
    Lang9 = SDLScancode.Lang9,
    Alterase = SDLScancode.Alterase,
    Sysreq = SDLScancode.Sysreq,
    Cancel = SDLScancode.Cancel,
    Clear = SDLScancode.Clear,
    Prior = SDLScancode.Prior,
    Return2 = SDLScancode.Return2,
    Separator = SDLScancode.Separator,
    Out = SDLScancode.Out,
    Oper = SDLScancode.Oper,
    Clearagain = SDLScancode.Clearagain,
    Crsel = SDLScancode.Crsel,
    Exsel = SDLScancode.Exsel,
    Kp00 = SDLScancode.Kp00,
    Kp000 = SDLScancode.Kp000,
    Thousandsseparator = SDLScancode.Thousandsseparator,
    Decimalseparator = SDLScancode.Decimalseparator,
    Currencyunit = SDLScancode.Currencyunit,
    Currencysubunit = SDLScancode.Currencysubunit,
    KpLeftparen = SDLScancode.KpLeftparen,
    KpRightparen = SDLScancode.KpRightparen,
    KpLeftbrace = SDLScancode.KpLeftbrace,
    KpRightbrace = SDLScancode.KpRightbrace,
    KpTab = SDLScancode.KpTab,
    KpBackspace = SDLScancode.KpBackspace,
    KpA = SDLScancode.KpA,
    KpB = SDLScancode.KpB,
    KpC = SDLScancode.KpC,
    KpD = SDLScancode.KpD,
    KpE = SDLScancode.KpE,
    KpF = SDLScancode.KpF,
    KpXor = SDLScancode.KpXor,
    KpPower = SDLScancode.KpPower,
    KpPercent = SDLScancode.KpPercent,
    KpLess = SDLScancode.KpLess,
    KpGreater = SDLScancode.KpGreater,
    KpAmpersand = SDLScancode.KpAmpersand,
    KpDblampersand = SDLScancode.KpDblampersand,
    KpVerticalbar = SDLScancode.KpVerticalbar,
    KpDblverticalbar = SDLScancode.KpDblverticalbar,
    KpColon = SDLScancode.KpColon,
    KpHash = SDLScancode.KpHash,
    KpSpace = SDLScancode.KpSpace,
    KpAt = SDLScancode.KpAt,
    KpExclam = SDLScancode.KpExclam,
    KpMemstore = SDLScancode.KpMemstore,
    KpMemrecall = SDLScancode.KpMemrecall,
    KpMemclear = SDLScancode.KpMemclear,
    KpMemadd = SDLScancode.KpMemadd,
    KpMemsubtract = SDLScancode.KpMemsubtract,
    KpMemmultiply = SDLScancode.KpMemmultiply,
    KpMemdivide = SDLScancode.KpMemdivide,
    KpPlusminus = SDLScancode.KpPlusminus,
    KpClear = SDLScancode.KpClear,
    KpClearentry = SDLScancode.KpClearentry,
    KpBinary = SDLScancode.KpBinary,
    KpOctal = SDLScancode.KpOctal,
    KpDecimal = SDLScancode.KpDecimal,
    KpHexadecimal = SDLScancode.KpHexadecimal,
    Lctrl = SDLScancode.Lctrl,
    Lshift = SDLScancode.Lshift,
    Lalt = SDLScancode.Lalt,
    Lgui = SDLScancode.Lgui,
    Rctrl = SDLScancode.Rctrl,
    Rshift = SDLScancode.Rshift,
    Ralt = SDLScancode.Ralt,
    Rgui = SDLScancode.Rgui,
    Mode = SDLScancode.Mode,
    Audionext = SDLScancode.Audionext,
    Audioprev = SDLScancode.Audioprev,
    Audiostop = SDLScancode.Audiostop,
    Audioplay = SDLScancode.Audioplay,
    Audiomute = SDLScancode.Audiomute,
    Mediaselect = SDLScancode.Mediaselect,
    Www = SDLScancode.Www,
    Mail = SDLScancode.Mail,
    Calculator = SDLScancode.Calculator,
    Computer = SDLScancode.Computer,
    AcSearch = SDLScancode.AcSearch,
    AcHome = SDLScancode.AcHome,
    AcBack = SDLScancode.AcBack,
    AcForward = SDLScancode.AcForward,
    AcStop = SDLScancode.AcStop,
    AcRefresh = SDLScancode.AcRefresh,
    AcBookmarks = SDLScancode.AcBookmarks,
    Brightnessdown = SDLScancode.Brightnessdown,
    Brightnessup = SDLScancode.Brightnessup,
    Displayswitch = SDLScancode.Displayswitch,
    Kbdillumtoggle = SDLScancode.Kbdillumtoggle,
    Kbdillumdown = SDLScancode.Kbdillumdown,
    Kbdillumup = SDLScancode.Kbdillumup,
    Eject = SDLScancode.Eject,
    Sleep = SDLScancode.Sleep,
    App1 = SDLScancode.App1,
    App2 = SDLScancode.App2,
    Audiorewind = SDLScancode.Audiorewind,
    Audiofastforward = SDLScancode.Audiofastforward,
    Softleft = SDLScancode.Softleft,
    Softright = SDLScancode.Softright,
    Call = SDLScancode.Call,
    Endcall = SDLScancode.Endcall,
    NumScancodes = SDLScancode.NumScancodes,
}