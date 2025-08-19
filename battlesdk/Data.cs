using battlesdk.json;
using NLog;

namespace battlesdk;

/// <summary>
/// Contains all the data defined in the resources folder. <see cref="Init"/>
/// must be called once before any data in this class can be read.
/// </summary>
public static class Data {
    public static string ResFolderPath { get; private set; } = "res";

    public const string PATH_MESSAGE_FRAMES = "data/misc/message_frames.json";
    public const string PATH_BOX_FRAMES = "data/misc/box_frames.json";

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
        var messageFrames = ReadMessageFrames();
        var boxFrames = ReadBoxFrames();
        var timeTints = ReadTimeTints();

        Misc = new() {
            MessageFrames = messageFrames,
            BoxFrames = boxFrames,
            TimeTints = timeTints,
        };
    }

    private static List<string> ReadMessageFrames () {
        var path = Path.Combine(ResFolderPath, PATH_MESSAGE_FRAMES);
        if (File.Exists(path) == false) {
            throw new("No message_frames file found."); // TODO: Better exception.
        }

        var txt = File.ReadAllText(path);
        var list = Json.Parse<List<string>>(txt)
            ?? throw new("Failed to read message_frames file.");

        return list;
    }

    private static List<string> ReadBoxFrames () {
        var path = Path.Combine(ResFolderPath, PATH_BOX_FRAMES);
        if (File.Exists(path) == false) {
            throw new("No box_frames file found."); // TODO: Better exception.
        }

        var txt = File.ReadAllText(path);
        var list = Json.Parse<List<string>>(txt)
            ?? throw new("Failed to read box_frames file.");

        return list;
    }

    private static List<ColorRGB>? ReadTimeTints () {
        if (File.Exists("res/data/misc/time_tints.json") == false) {
            _logger.Warn("No time_tints file found. Time tints will not be applied");
            return null;
        }

        try {
            var txt = File.ReadAllText("res/data/misc/time_tints.json");
            var list = Json.Parse<List<ColorRGB>>(txt);

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
    public required List<string> MessageFrames { get; init; }
    public required List<string> BoxFrames { get; init; }
    public required List<ColorRGB>? TimeTints { get; init; }
}
