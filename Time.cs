using SDL;

namespace battlesdk;
public static class Time {
    private readonly static float _freq = SDL3.SDL_GetPerformanceFrequency();
    private static ulong _lastCounter = SDL3.SDL_GetPerformanceCounter();

    /// <summary>
    /// The amount of time, in seconds, since the last frame.
    /// </summary>
    public static float DeltaTime { get; private set; } = 0f;
    /// <summary>
    /// The amount of time, in seconds, since the application started.
    /// </summary>
    public static float TotalTime { get; private set; } = SDL3.SDL_GetPerformanceCounter();

    public static void Update () {
        ulong now = SDL3.SDL_GetPerformanceCounter();

        DeltaTime = (now - _lastCounter) / _freq;
        TotalTime = now / _freq;

        _lastCounter = now;
    }
}
