namespace battlesdk.types;

public readonly record struct IVec2 (int X, int Y) {
    public readonly static IVec2 Zero = new(0, 0);

    public IVec2 (int val) : this(val, val) { }

    public static implicit operator Vec2 (IVec2 v) {
        return new(v.X, v.Y);
    }

    public static IVec2 operator + (IVec2 a, IVec2 b) {
        return new(a.X + b.X, a.Y + b.Y);
    }

    public static IVec2 operator - (IVec2 a, IVec2 b) {
        return new(a.X - b.X, a.Y - b.Y);
    }

    public static IVec2 operator * (IVec2 a, int n) {
        return new(a.X * n, a.Y * n);
    }

    public static Vec2 operator * (IVec2 a, float n) {
        return new(a.X * n, a.Y * n);
    }

    public override string ToString () {
        return $"({X}, {Y})";
    }
}

public readonly record struct Vec2 (float X, float Y) {
    public readonly static Vec2 Zero = new(0, 0);

    public Vec2 (int val) : this(val, val) { }

    public static implicit operator IVec2 (Vec2 v) {
        return new((int)v.X, (int)v.Y);
    }

    public static Vec2 operator + (Vec2 a, Vec2 b) {
        return new(a.X + b.X, a.Y + b.Y);
    }

    public static Vec2 operator - (Vec2 a, Vec2 b) {
        return new(a.X - b.X, a.Y - b.Y);
    }

    public static Vec2 operator * (Vec2 a, int n) {
        return new(a.X * n, a.Y * n);
    }

    public static Vec2 operator * (Vec2 a, float n) {
        return new(a.X * n, a.Y * n);
    }

    public override string ToString () {
        return $"({X}, {Y})";
    }
}

public readonly record struct IRect (int Top, int Left, int Bottom, int Right) {
    public readonly static IRect Zero = new(0, 0, 0, 0);

    public override string ToString () {
        return $"(top: {Top}, left: {Left}, bottom: {Bottom}, right: {Right})";
    }
}

public enum Direction {
    Down,
    Right,
    Up,
    Left,
    None,
};

[Flags]
public enum DirectionMask {
    Down = 0b1,
    Right = 0b10,
    Up = 0b100,
    Left = 0b1000,
};

public static class TypesUtils {
    public static DirectionMask ToMask (this Direction direction) {
        return direction switch {
            Direction.Down => DirectionMask.Down,
            Direction.Right => DirectionMask.Right,
            Direction.Up => DirectionMask.Up,
            Direction.Left => DirectionMask.Left,
            _ => (DirectionMask)direction,
        };
    }

    public static Direction Opposite (this Direction direction) {
        return direction switch {
            Direction.Down => Direction.Up,
            Direction.Right => Direction.Left,
            Direction.Up => Direction.Down,
            Direction.Left => Direction.Right,
            _ => direction,
        };
    }

    public static Direction ToOrdinal (this DirectionMask direction) {
        return direction switch {
            DirectionMask.Down => Direction.Down,
            DirectionMask.Right => Direction.Right,
            DirectionMask.Up => Direction.Up,
            DirectionMask.Left => Direction.Left,
            _ => (Direction)direction,
        };
    }
}