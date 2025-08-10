using SDL;

namespace battlesdk;
public static class Time {
    private static ulong _lastCounter = 0ul;

    /// <summary>
    /// The amount of time, in seconds, since the last frame.
    /// </summary>
    public static float DeltaTime { get; private set; } = 0f;
    /// <summary>
    /// The amount of time, in seconds, since the application started.
    /// </summary>
    public static double TotalTime { get; private set; } = 0f;
    /// <summary>
    /// The frequency of the performance counter. That is, how many units equal
    /// one second.
    /// </summary>
    public static float Frequency { get; } = SDL3.SDL_GetPerformanceFrequency();

    public static void Init () {
        _lastCounter = SDL3.SDL_GetPerformanceCounter();
        DeltaTime = 0f;
        TotalTime = SDL3.SDL_GetPerformanceCounter();
    }

    public static void Update () {
        ulong now = SDL3.SDL_GetPerformanceCounter();

        DeltaTime = ((now - _lastCounter) / Frequency) * 1f;
        TotalTime = now / Frequency;

        _lastCounter = now;
    }

    /// <summary>
    /// Returns a counter that represents time. The unit of this counter is not
    /// any specific time unit, and as such, performance values only make sense
    /// in relation to each other. The amount of units that represent one second
    /// can be obtained with <see cref="Frequency"/>.
    /// 
    /// This counter is only necessary to measure time inside a single frame.
    /// To measure the amount of time passed since the last frame, use
    /// <see cref="DeltaTime"/> instead.
    /// </summary>
    public static ulong GetPerformanceCounter () {
        return SDL3.SDL_GetPerformanceCounter();
    }

    /// <summary>
    /// Returns the amount of minutes that have passed, since midnight, in the
    /// computer's timezone.
    /// </summary>
    public static int RealMinutes () {
        return (DateTime.Now.Hour * 60) + DateTime.Now.Minute;
    }
}
