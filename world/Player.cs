namespace battlesdk.world;
public class Player : Character {
    private const float MOVE_INPUT_DELAY = 0.1f;

    private PlayerSoundManager _sounds = new();

    private float _leftKeyStart = float.MinValue;
    private float _rightKeyStart = float.MinValue;
    private float _upKeyStart = float.MinValue;
    private float _downKeyStart = float.MinValue;

    public Player (IVec2 position) : base(position, "dawn") { }

    public override void Update () {
        base.Update();

        ProcessInput();
    }

    private void ProcessInput () {
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
                    Move(Direction.Left);
                }
            }
            else if (Controls.GetKey(ActionKey.Right)) {
                if (Time.TotalTime - _rightKeyStart > MOVE_INPUT_DELAY) {
                    Move(Direction.Right);
                }
            }
            else if (Controls.GetKey(ActionKey.Up)) {
                if (Time.TotalTime - _upKeyStart > MOVE_INPUT_DELAY) {
                    Move(Direction.Up);
                }
            }
            else if (Controls.GetKey(ActionKey.Down)) {
                if (Time.TotalTime - _downKeyStart > MOVE_INPUT_DELAY) {
                    Move(Direction.Down);
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

        if (Controls.GetKeyDown(ActionKey.Secondary)) {
            IsRunning = true;
        }

        if (Controls.GetKeyUp(ActionKey.Secondary)) {
            IsRunning = false;
        }
    }
}
