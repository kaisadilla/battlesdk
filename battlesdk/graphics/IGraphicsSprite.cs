using SDL;

namespace battlesdk.graphics;

public interface IGraphicsSprite {
    /// <summary>
    /// Draws this sprite at the position given.
    /// </summary>
    /// <param name="position">The position to draw the sprite at.</param>
    void Draw (IVec2 position);
    /// <summary>
    /// Draws this sprite at the position given with the size given.
    /// </summary>
    /// <param name="position">The position to draw the sprite at.</param>
    /// <param name="size">The size to stretch the sprite to.</param>
    /// <param name="resizeMode">The strategy to use to resize the sprite.</param>
    void Draw (IVec2 position, IVec2 size, ResizeMode resizeMode = ResizeMode.Stretch);
    /// <summary>
    /// Draws a section of the sprite at the position given.
    /// </summary>
    /// <param name="section">The section of the sprite to draw.</param>
    /// <param name="position">The position to draw the sprite at.</param>
    void DrawSection (SDL_FRect section, IVec2 position);
    /// <summary>
    /// Draws a section of the sprite at the position given with the size given.
    /// </summary>
    /// <param name="section">The section of the sprite to draw.</param>
    /// <param name="position">The position to draw the sprite at.</param>
    /// <param name="size">The size to stretch the sprite to.</param>
    /// <param name="resizeMode">The strategy to use to resize the sprite.</param>
    void DrawSection (
        SDL_FRect section, IVec2 position, IVec2 size, ResizeMode resizeMode = ResizeMode.Stretch
    );
    /// <summary>
    /// Frees all the memory resources used by this sprite, if any. After this
    /// action, this sprite can no longer be used.
    /// </summary>
    void Destroy();
}
