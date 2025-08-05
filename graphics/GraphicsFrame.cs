using battlesdk.data;
using SDL;

namespace battlesdk.graphics;
public class GraphicsFrame : GraphicsTexture {
    public FrameAsset File { get; private init; }

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

    public unsafe GraphicsFrame (SDL_Renderer* renderer, FrameAsset file)
        : base(renderer, file.Path)
    {
        File = file;

        _topLeft = new() {
            x = 0,
            y = 0,
            w = file.X[0],
            h = file.Y[0]
        };
        _topRight = new() {
            x = file.X[1],
            y = 0,
            w = _width - file.X[1],
            h = file.Y[0]
        };
        _bottomLeft = new() {
            x = 0,
            y = file.Y[1],
            w = file.X[0],
            h = _height - file.Y[1]
        };
        _bottomRight = new() {
            x = file.X[1],
            y = file.Y[1],
            w = _width - file.X[1],
            h = _height - file.Y[1]
        };

        _left = new() {
            x = 0,
            y = file.Y[0],
            w = file.X[0],
            h = file.Y[1] - file.Y[0]
        };
        _right = new() {
            x = file.X[1],
            y = file.Y[0],
            w = _width - file.X[1],
            h = file.Y[1] - file.Y[0]
        };
        _top = new() {
            x = file.X[0],
            y = 0,
            w = file.X[1] - file.X[0],
            h = file.Y[0]
        };
        _bottom = new() {
            x = file.X[0],
            y = file.Y[1],
            w = file.X[1] - file.X[0],
            h = _height - file.Y[1]
        };

        _center = new() {
            x = file.X[0],
            y = file.Y[0],
            w = file.X[1] - file.X[0],
            h = file.Y[1] - file.Y[0]
        };
    }

    public unsafe void Draw (IVec2 pos, IVec2 size) {
        int x1 = pos.X + File.X[0];
        int x2 = pos.X + size.X - (File.Width - File.X[1]);
        int y1 = pos.Y + File.Y[0];
        int y2 = pos.Y + size.Y - (File.Height - File.Y[1]);

        DrawSection(_topLeft, pos);
        DrawSection(_topRight, new(x2, pos.Y));
        DrawSection(_bottomLeft, new(pos.X, y2));
        DrawSection(_bottomRight, new(x2, y2));
        DrawSectionResize(_left, new(pos.X, y1), new((int)_left.w, y2 - y1), File.YMode);
        DrawSectionResize(_right, new(x2, y1), new((int)_right.w, y2 - y1), File.YMode);
        DrawSectionResize(_top, new(x1, pos.Y), new(x2 - x1, (int)_top.h), File.XMode);
        DrawSectionResize(_bottom, new(x1, y2), new(x2 - x1, (int)_bottom.h), File.XMode);
        DrawSectionResize(_center, new(x1, y1), new(x2 - x1, y2 - y1), File.CenterMode);
    }
}
