using Hexa.NET.SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battlesdk;

/// <summary>
/// This class manages the action keys pressed by the user. Action keys are
/// keyboard-agnostic and represent each individual control the game has (such
/// as the primary key or the menu key). This class is updated at the start of
/// each frame with the input the user has provided for this specific frame.
/// </summary>
public static class Controls {
    /*
     * This class acts on the scancode of the keys pressed by the player. The
     * scancode refers to the physical key in the keyboard, regardless of the
     * keyboard layout the player is currently using. This class does not
     * expose the keys pressed by the user, but rather the in-game controls that
     * are bound to the given keys.
     */

    /// <summary>
    /// The actions that are being input this frame.
    /// </summary>
    private static readonly HashSet<ActionKey> _currentActions = [];
    /// <summary>
    /// The actions that were input the last frame.
    /// </summary>
    private static readonly HashSet<ActionKey> _lastActions = [];

    /// <summary>
    /// Maps each physical key in the keyboard to a given action. It is perfectly
    /// possible for more than one key to be mapped to the same action, or to
    /// have an action that is not mapped to any key (although this will make
    /// said action unavailable).
    /// </summary>
    private static readonly Dictionary<SDLScancode, ActionKey> _bindings = new() {
        [SDLScancode.X] = ActionKey.Primary,
        [SDLScancode.C] = ActionKey.Secondary,
        [SDLScancode.Up] = ActionKey.Up,
        [SDLScancode.Kp8] = ActionKey.Up,
        [SDLScancode.Down] = ActionKey.Down,
        [SDLScancode.Kp2] = ActionKey.Down,
        [SDLScancode.Left] = ActionKey.Left,
        [SDLScancode.Kp4] = ActionKey.Left,
        [SDLScancode.Right] = ActionKey.Right,
        [SDLScancode.Kp6] = ActionKey.Right,
        [SDLScancode.Z] = ActionKey.Menu,
        [SDLScancode.Return] = ActionKey.Enter,
        [SDLScancode.KpEnter] = ActionKey.Enter,
        [SDLScancode.Escape] = ActionKey.Escape,
    };

    public static void Update () {
        // Each frame, we update the list of actions pressed last frame with
        // what is currently in the list. We do not delete the list for this
        // frame, as the key may still be pressed.
        _lastActions.Clear();

        foreach (var key in _currentActions) {
            _lastActions.Add(key);
        }
    }

    public static void RegisterEvent (SDLEvent evt) {
        if (_bindings.TryGetValue(evt.Key.Keysym.Scancode, out var action) == false) {
            return;
        }

        if (evt.Type == (uint)SDLEventType.Keydown && evt.Key.Repeat == 0) {
            _currentActions.Add(action);
        }
        else if (evt.Type == (uint)SDLEventType.Keyup) {
            _currentActions.Remove(action);
        }
    }

    public static bool GetKeyDown (ActionKey key) {
        // A key was pressed down this frame if it's pressed this frame but
        // was not pressed last frame.

        return _currentActions.Contains(key) && _lastActions.Contains(key) == false;
    }

    public static bool GetKey (ActionKey key) {
        return _currentActions.Contains(key);
    }

    public static bool GetKeyUp (ActionKey key) {
        // A key was released this frame if it's not pressed this frame but was
        // pressed last frame.

        return _currentActions.Contains(key) == false && _lastActions.Contains(key);
    }

    // TODO: Methods to change bindings.
}

public enum ActionKey {
    Unknown,
    Primary,
    Secondary,
    Up,
    Down,
    Left,
    Right,
    Menu,
    Enter,
    Escape,
}