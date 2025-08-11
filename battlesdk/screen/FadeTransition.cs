using battlesdk.graphics;

namespace battlesdk.screen;
public class FadeTransition : ITransition {
    private Renderer _renderer;

    public string Name { get; }

    public FadeTransition (Renderer renderer) {
        _renderer = renderer;
        Name = $"[Fade transition]";
    }

    public void Draw (float progress) {
        _renderer.DrawRectangle(
            IVec2.Zero,
            _renderer.Size,
            new(0, 0, 0, (int)(255 * progress))
        );
    }
}
