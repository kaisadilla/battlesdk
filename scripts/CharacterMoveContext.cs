using battlesdk.world;
using MoonSharp.Interpreter;

namespace battlesdk.scripts;

[MoonSharpUserData]
internal class CharacterMoveContext {
    [MoonSharpHidden]
    private readonly Character _character;

    public CharacterMoveContext (Character character) {
        _character = character;
    }

    public void MoveUp () {
        _character.Move(Direction.Up, true);
    }

    public void MoveDown () {
        _character.Move(Direction.Down, true);
    }

    public void MoveLeft () {
        _character.Move(Direction.Left, true);
    }

    public void MoveRight () {
        _character.Move(Direction.Right, true);
    }
}
