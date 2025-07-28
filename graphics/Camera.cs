using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battlesdk.graphics;

public class Camera {
    private int _width;
    private int _height;

    private int _offsetX;
    private int _offsetY;

    private IVec2 _center;
    private float _pxCenterX;
    private float _pxCenterY;

    /// <summary>
    /// The tile position in the world where the camera is centered.
    /// </summary>
    public Vec2 Center {
        get => _center;
        set {
            _center = value;
            _pxCenterX = (value.X * Constants.TILE_SIZE) + (Constants.TILE_SIZE / 2);
            _pxCenterY = (value.Y * Constants.TILE_SIZE) + (Constants.TILE_SIZE / 2);
        }
    }

    public Camera (int width, int height, IVec2 center) {
        _width = width;
        _height = height;

        _offsetX = _width / 2;
        _offsetY = _height / 2;

        Center = center;
    }

    public int GetScreenX (float worldX) {
        return (int)(((worldX * Constants.TILE_SIZE) - _pxCenterX) + _offsetX);
    }

    public int GetScreenY (float worldY) {
        return (int)(((worldY * Constants.TILE_SIZE) - _pxCenterY) + _offsetY);
    }

    public IVec2 GetScreenPos (Vec2 pos) {
        return new(GetScreenX(pos.X), GetScreenY(pos.Y));
    }
}
