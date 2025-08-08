using battlesdk.json;
using SDL;
using StackCleaner;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;

namespace battlesdk.types;

[JsonConverter(typeof(IVec2Converter))]
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

    public static IRect operator + (IRect rect, IVec2 pos) {
        return new() {
            Top = rect.Top + pos.Y,
            Left = rect.Left + pos.X,
            Bottom = rect.Bottom + pos.Y,
            Right = rect.Right + pos.X
        };
    }

    public static IRect operator * (IRect rect, float val) {
        return new() {
            Top = (int)(rect.Top * val),
            Left = (int)(rect.Left * val),
            Bottom = (int)(rect.Bottom * val),
            Right = (int)(rect.Right * val),
        };
    }
}

public readonly record struct Rect (float Top, float Left, float Bottom, float Right) {
    public readonly static IRect Zero = new(0, 0, 0, 0);

    public override string ToString () {
        return $"(top: {Top}, left: {Left}, bottom: {Bottom}, right: {Right})";
    }

    public static Rect operator + (Rect rect, Vec2 pos) {
        return new() {
            Top = rect.Top + pos.Y,
            Left = rect.Left + pos.X,
            Bottom = rect.Bottom + pos.Y,
            Right = rect.Right + pos.X
        };
    }

    public static Rect operator * (Rect rect, float val) {
        return new() {
            Top = rect.Top * val,
            Left = rect.Left * val,
            Bottom = rect.Bottom * val,
            Right = rect.Right * val,
        };
    }
}

[JsonConverter(typeof(ColorRGBConverter))]
public readonly record struct ColorRGB (int R, int G, int B) {

}

[JsonConverter(typeof(ColorRGBAConverter))]
public readonly record struct ColorRGBA (int R, int G, int B, int A) {

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

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResizeMode {
    Stretch,
    Repeat,
}

public readonly unsafe struct Ptr<T> where T : unmanaged {
    private readonly T* _ptr;

    public Ptr (T* ptr) {
        _ptr = ptr;
    }

    public Ptr (nint raw) {
        _ptr = (T*)raw;
    }

    public T* Raw => _ptr;
    public nint Nint => (nint)_ptr;
    public IntPtr IntPtr => (IntPtr)_ptr;

    public bool IsNull => _ptr == null;
    public ref T Ref => ref *_ptr;

    public override string ToString () {
        return ((nint)_ptr).ToString();
    }
}


public static class TypesUtils {
    private static readonly StackTraceCleaner _cleaner = new(new() {
        ColorFormatting = StackColorFormatType.ExtendedANSIColor,
        IncludeFileData = true,
        IncludeILOffset = true,
        IncludeNamespaces = false,
        Colors = Color32Config.Default,
        IncludeSourceData = true,
        UseTypeAliases = true,
    });

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

    public static IVec2 TileToPixelSpace (Vec2 tilePos) {
        return new IVec2(
            (int)(tilePos.X * Constants.TILE_SIZE),
            (int)(tilePos.Y * Constants.TILE_SIZE)
        );
    }

    public static IRect TileToPixelSpace (IRect rect) {
        return rect * Constants.TILE_SIZE;
    }

    public static float Lerp (this float a, float b, float t) {
        return a + (b - a) * t;
    }

    public static float Lerp (this int a, float b, float t) {
        return a + (b - a) * t;
    }

    public static IVec2 OffsetAt (this IVec2 v, Direction dir) {
        return dir switch {
            Direction.Down => v + new IVec2(0, 1),
            Direction.Right => v + new IVec2(1, 0),
            Direction.Up => v + new IVec2(0, -1),
            Direction.Left => v + new IVec2(-1, 0),
            _ => v,
        };
    }

    public static SDL_Color SdlColor (byte r, byte g, byte b, byte a) {
        return new() {
            r = r,
            g = g,
            b = b,
            a = a,
        };
    }

    public static SDL_FRect SdlFRect (float x, float y, float w, float h) {
        return new() {
            x = x,
            y = y,
            w = w,
            h = h,
        };
    }

    /// <summary>
    /// Returns a snake_case version of this string. Non-ASCII characters are
    /// not supported.
    /// </summary>
    /// <param name="str">The string to transform, preferably ASCII.</param>
    public static string ToSnakeCase (this string str) {
        var sb = new StringBuilder();

        for (int i = 0; i < str.Length; i++) {
            char c = str[i];

            if (char.IsUpper(c) && i > 0) {
                sb.Append('_');
            }

            sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Prints the exception fancily to the console. IMPORTANT: This output is
    /// not added to any logger.
    /// </summary>
    /// <param name="exception">The exception to fancily print.</param>
    public static void PrintFancy (this Exception? exception) {
        List<Exception> exceptionStack = [];

        while (exception is not null) {
            exceptionStack.Add(exception);
            exception = exception.InnerException;
        }

        Console.WriteLine();
        Console.WriteLine("===========================");
        Console.WriteLine("====     EXCEPTION     ====");
        Console.WriteLine("===========================");
        Console.WriteLine();

        foreach (var ex in exceptionStack) {
            var stack = StackTraceCleaner.GetStackTrace(ex);

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(ex.GetType().Name);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);

            if (stack is  null) return;
            _cleaner.WriteToConsole(stack.Reverse());
        }

        Console.ResetColor();
    }

    public static StackTrace Reverse (this StackTrace stack) {
        var frames = stack.GetFrames();

        if (frames is null || frames.Length < 2) return stack;

        return new StackTrace(frames.Reverse());
    }
}