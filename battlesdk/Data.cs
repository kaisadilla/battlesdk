using NLog;
using System.Text.Json;

namespace battlesdk;

/// <summary>
/// Contains all the data defined in the resources folder. <see cref="Init"/>
/// must be called once before any data in this class can be read.
/// </summary>
public static class Data {
    // Note: if any of the non-nullable fields in this class contains `null`,
    // that usually means that the field was accessed before Init() was ever
    // called.

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public static MiscData Misc { get; private set; } = null!;

    /// <summary>
    /// Loads all the data in the resources folder. This method must be called
    /// once before data can be read from this class.
    /// </summary>
    public static void Init () {
        var timeTints = ReadTimeTints();

        Misc = new() {
            TimeTints = timeTints,
        };
    }

    private static List<ColorRGB>? ReadTimeTints () {
        if (File.Exists("res/data/misc/time_tints.json") == false) {
            _logger.Warn("No time_tints file found. Time tints will not be applied");
            return null;
        }

        try {
            var txt = File.ReadAllText("res/data/misc/time_tints.json");
            var list = JsonSerializer.Deserialize<List<ColorRGB>>(txt);

            if (list is null) return null;

            if (list.Count < 24) {
                _logger.Warn(
                    "time_tints contains less than 24 entries. Missing entries " +
                    "will be filled with default (no) tint."
                );
                while (list.Count < 24) {
                    list.Add(new(0, 0, 0));
                }
            }
            else if (list.Count > 24) {
                _logger.Warn(
                    "time_tints contains more than 24 entries. Excess entries " +
                    "will be removed."
                );

                list.RemoveRange(24, list.Count - 24);
            }

            return list;
        }
        catch (Exception ex) {
            _logger.ErrorEx(ex, "Failed to raed time_tints.");
            return null;
        }
    }
}

public class MiscData {
    public required List<ColorRGB>? TimeTints { get; init; }
}
