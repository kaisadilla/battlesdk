using Hexa.NET.SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battlesdk;
public static class Time {
    private readonly static float _freq = SDL.GetPerformanceFrequency();
    private static ulong _lastCounter = SDL.GetPerformanceCounter();

    /// <summary>
    /// The amount of time, in seconds, since the last frame.
    /// </summary>
    public static float DeltaTime { get; private set; } = 0f;
    /// <summary>
    /// The amount of time, in seconds, since the application started.
    /// </summary>
    public static float TotalTime { get; private set; } = SDL.GetPerformanceCounter();

    public static void Update () {
        ulong now = SDL.GetPerformanceCounter();

        DeltaTime = (now - _lastCounter) / _freq;
        TotalTime = now / _freq;

        _lastCounter = now;
    }
}
