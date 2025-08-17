using MoonSharp.Interpreter;

namespace battlesdk.scripts.types;

[LuaApiClass]
public class LuaFmt {
    [MoonSharpHidden]
    public const string CLASSNAME = "Fmt";

    public static string pad_left (DynValue val, char padding, int total_width) {
        return val.ToString().PadLeft(total_width, padding);
    }

    /// <summary>
    /// Formats time as HH:mm.
    /// </summary>
    /// <param name="time">An amount of time, in seconds.</param>
    /// <returns></returns>
    public static string time_span_as_hh_mm (double time) {
        var ts = TimeSpan.FromSeconds(time);

        return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}";
    }

    /// <summary>
    /// Formats time as X d X h X m, ignoring any part that is 0 (e.g. 0 days,
    /// 12 hours, 15 minutes becomes "12 h 15 m").
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static string time_span_as_h_m (double time) {
        var ts = TimeSpan.FromSeconds(time);

        if (ts.Days > 0) {
            return $"{(int)ts.TotalDays} d {ts.Hours} h {ts.Minutes} m";
        }
        if (ts.Hours > 0) {
            return $"{ts.Hours} h {ts.Minutes} m";
        }
        return $"{ts.Minutes} m";
    }
}
