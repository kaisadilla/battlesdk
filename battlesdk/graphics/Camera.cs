namespace battlesdk.graphics;

public class Camera {
    /// <summary>
    /// The width of the camera, in pixels.
    /// </summary>
    public int Width { get; private set; }
    /// <summary>
    /// The height of the camera, in pixels.
    /// </summary>
    public int Height { get; private set; }
    /// <summary>
    /// The view formed by this camera.
    /// </summary>
    private IRect _localView;

    /// <summary>
    /// The tile position in the world where the camera is centered.
    /// </summary>
    public Vec2 Position { get; private set; }
    /// <summary>
    /// The rectangle of the world that is visible, in pixels.
    /// </summary>
    public IRect View { get; private set; }

    /// <param name="width">The width of the viewport, in pixels.</param>
    /// <param name="height">The height of the viewport, in pixels.</param>
    /// <param name="position">The position of the camera, in pixel space.</param>
    public Camera (int width, int height, IVec2 position) {
        Width = width;
        Height = height;

        int off = Constants.TILE_SIZE / 2;

        _localView = new() {
            Left = -(width / 2) + off,
            Right = -(width / 2) + off + width, // Doing it this way ensures odd numbers don't result in incorrect values.
            Top = -(height / 2) + off,
            Bottom = -(height / 2) + off + height,
        };

        SetPosition(position);
    }

    /// <summary>
    /// Sets a new center for the camera.
    /// </summary>
    /// <param name="position">A position in pixel space.</param>
    public void SetPosition (IVec2 position) {
        Position = position;
        View = _localView + position;
    }

    /// <summary>
    /// Given an x position in the world (in pixel space), returns its position
    /// in the screen. A value below 0, or equal or above <see cref="Width"/>
    /// is out of the screen.
    /// </summary>
    /// <param name="worldX">An x position in the world, in pixels.</param>
    public int GetScreenX (int worldX) {
        return worldX - View.Left;
    }

    /// <summary>
    /// Given an x position in the world (in pixel space), returns its position
    /// in the screen. A value below 0, or equal or above <see cref="Width"/>
    /// is out of the screen.
    /// </summary>
    /// <param name="worldX">An x position in the world, in pixels.</param>
    public int GetScreenY (int worldY) {
        return worldY - View.Top;
    }

    /// <summary>
    /// Given a position in the world (in pixel space), returns its position
    /// in the screen.
    /// </summary>
    /// <param name="pos">A position in the world, in pixels.</param>
    public IVec2 GetScreenPos (IVec2 pos) {
        return new(GetScreenX(pos.X), GetScreenY(pos.Y));
    }

    /// <summary>
    /// Given a rectangle, returns whether it's visible by the camera.
    /// </summary>
    /// <param name="rect">The rectangle to check</param>
    /// <returns></returns>
    public bool IsRectVisible (IRect rect) {
        return !(
            View.Left >= rect.Right
            || View.Right <= rect.Left
            || View.Top >= rect.Bottom
            || View.Bottom <= rect.Top
        );
    }
}
