using battlesdk.data;
using SDL;

namespace battlesdk.graphics.resources;
public class GraphicsFrameSprite : GraphicsSprite {
    public new FrameSpriteFile Asset => (FrameSpriteFile)base.Asset;

    // Each of the 9 parts of a textbox: 4 corners, 4 borders and 1 center.
    private SDL_FRect _topLeft;
    private SDL_FRect _topRight;
    private SDL_FRect _bottomLeft;
    private SDL_FRect _bottomRight;
    private SDL_FRect _left;
    private SDL_FRect _right;
    private SDL_FRect _top;
    private SDL_FRect _bottom;
    private SDL_FRect _center;

    public unsafe GraphicsFrameSprite (Renderer renderer, FrameSpriteFile file)
        : base(renderer, file)
    {
        _topLeft = new() {
            x = 0,
            y = 0,
            w = Asset.X[0],
            h = Asset.Y[0]
        };
        _topRight = new() {
            x = Asset.X[1],
            y = 0,
            w = Width - Asset.X[1],
            h = Asset.Y[0]
        };
        _bottomLeft = new() {
            x = 0,
            y = Asset.Y[1],
            w = Asset.X[0],
            h = Height - Asset.Y[1]
        };
        _bottomRight = new() {
            x = Asset.X[1],
            y = Asset.Y[1],
            w = Width - Asset.X[1],
            h = Height - Asset.Y[1]
        };

        _left = new() {
            x = 0,
            y = Asset.Y[0],
            w = Asset.X[0],
            h = Asset.Y[1] - Asset.Y[0]
        };
        _right = new() {
            x = Asset.X[1],
            y = Asset.Y[0],
            w = Width - Asset.X[1],
            h = Asset.Y[1] - Asset.Y[0]
        };
        _top = new() {
            x = Asset.X[0],
            y = 0,
            w = Asset.X[1] - Asset.X[0],
            h = Asset.Y[0]
        };
        _bottom = new() {
            x = Asset.X[0],
            y = Asset.Y[1],
            w = Asset.X[1] - Asset.X[0],
            h = Height - Asset.Y[1]
        };

        _center = new() {
            x = Asset.X[0],
            y = Asset.Y[0],
            w = Asset.X[1] - Asset.X[0],
            h = Asset.Y[1] - Asset.Y[0]
        };
    }

    public unsafe override void Draw (
        IVec2 pos, IVec2 size, ResizeMode resizeMode = ResizeMode.Stretch
    ) {
        int x1 = pos.X + Asset.X[0];
        int x2 = pos.X + size.X - (Asset.Width - Asset.X[1]);
        int y1 = pos.Y + Asset.Y[0];
        int y2 = pos.Y + size.Y - (Asset.Height - Asset.Y[1]);

        DrawSection(_topLeft, pos);
        DrawSection(_topRight, new(x2, pos.Y));
        DrawSection(_bottomLeft, new(pos.X, y2));
        DrawSection(_bottomRight, new(x2, y2));
        DrawSection(_left, new(pos.X, y1), new((int)_left.w, y2 - y1), Asset.YMode);
        DrawSection(_right, new(x2, y1), new((int)_right.w, y2 - y1), Asset.YMode);
        DrawSection(_top, new(x1, pos.Y), new(x2 - x1, (int)_top.h), Asset.XMode);
        DrawSection(_bottom, new(x1, y2), new(x2 - x1, (int)_bottom.h), Asset.XMode);
        DrawSection(_center, new(x1, y1), new(x2 - x1, y2 - y1), Asset.CenterMode);
    }
}
