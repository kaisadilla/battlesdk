using battlesdk.input;

namespace battlesdk.world.entities;
public class Player : Character, IInputListener {
    private const float MOVE_INPUT_DELAY = 0.1f;

    private PlayerSoundManager _sounds = new();

    private float _leftKeyStart = float.MinValue;
    private float _rightKeyStart = float.MinValue;
    private float _upKeyStart = float.MinValue;
    private float _downKeyStart = float.MinValue;

    public bool BlockOtherInput => true;

    public Player (IVec2 position) : base(position, "dawn") {
        InputManager.Push(this);
    }

    public override void Update () {
        base.Update();
    }

    public void HandleInput () {
        if (IsMoving == false) {
            if (Controls.GetKeyDown(ActionKey.Left)) {
                SetDirection(Direction.Left);
                _leftKeyStart = Time.TotalTime;
            }
            else if (Controls.GetKeyDown(ActionKey.Right)) {
                SetDirection(Direction.Right);
                _rightKeyStart = Time.TotalTime;
            }
            else if (Controls.GetKeyDown(ActionKey.Up)) {
                SetDirection(Direction.Up);
                _upKeyStart = Time.TotalTime;
            }
            else if (Controls.GetKeyDown(ActionKey.Down)) {
                SetDirection(Direction.Down);
                _downKeyStart = Time.TotalTime;
            }

            if (Controls.GetKey(ActionKey.Left)) {
                if (Time.TotalTime - _leftKeyStart > MOVE_INPUT_DELAY) {
                    Move(Direction.Left, false);
                }
            }
            else if (Controls.GetKey(ActionKey.Right)) {
                if (Time.TotalTime - _rightKeyStart > MOVE_INPUT_DELAY) {
                    Move(Direction.Right, false);
                }
            }
            else if (Controls.GetKey(ActionKey.Up)) {
                if (Time.TotalTime - _upKeyStart > MOVE_INPUT_DELAY) {
                    Move(Direction.Up, false);
                }
            }
            else if (Controls.GetKey(ActionKey.Down)) {
                if (Time.TotalTime - _downKeyStart > MOVE_INPUT_DELAY) {
                    Move(Direction.Down, false);
                }
            }

            if (Collided) {
                _sounds.PlayCollision();
            }
            if (IsJumping) {
                _sounds.PlayJump();
            }
        }

        if (Controls.GetKeyUp(ActionKey.Left)) {
            _leftKeyStart = float.MinValue;
        }
        else if (Controls.GetKeyUp(ActionKey.Right)) {
            _rightKeyStart = float.MinValue;
        }
        else if (Controls.GetKeyUp(ActionKey.Up)) {
            _upKeyStart = float.MinValue;
        }
        else if (Controls.GetKeyUp(ActionKey.Down)) {
            _downKeyStart = float.MinValue;
        }

        if (Controls.GetKeyDown(ActionKey.Primary)) {
            HandlePrimaryInput();
        }
        if (Controls.GetKeyDown(ActionKey.Secondary)) {
            IsRunning = true;
        }

        if (Controls.GetKeyUp(ActionKey.Secondary)) {
            IsRunning = false;
        }

        if (IsMoving) {
            G.World.SetFocus(Position);
        }
    }

    private void HandlePrimaryInput () {
        var ch = G.World.GetCharacterAt(GetPositionInFront());
        if (ch is not null) {
            ch.Interact(Direction.Opposite());
            return;
        }
    }
}
