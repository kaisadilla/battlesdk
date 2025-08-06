using NLog;
using System.Text.Json;

namespace battlesdk.data;

public class MusicFile : IIdentifiable {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public string Name { get; private init; }
    public int Id { get; private set; }
    /// <summary>
    /// The absolute path to the file.
    /// </summary>
    public string Path { get; private init; }

    /// <summary>
    /// The start of the soundtrack loop, in milliseconds.
    /// </summary>
    public int? LoopStart { get; } = null;
    /// <summary>
    /// The end of the soundtrack loop, in milliseconds.
    /// </summary>
    public int? LoopEnd { get; } = null;

    public MusicFile (string name, string path) {
        Name = name;
        Path = path;

        var jsonPath = System.IO.Path.ChangeExtension(path, "json");
        if (File.Exists(jsonPath) == false) return;

        var txt = File.ReadAllText(jsonPath);
        using var json = JsonDocument.Parse(txt);
        var root = json.RootElement;

        if (root.TryGetProperty("loopStart", out var loopStartJson)) {
            if (loopStartJson.TryGetInt32(out int loopStart)) {
                LoopStart = loopStart;
            }
            else {
                _logger.Error($"'loopStart' in {jsonPath} has an invalid value.");
            }
        }

        if (root.TryGetProperty("loopEnd", out var loopEndJson)) {
            if (loopEndJson.TryGetInt32(out int loopEnd)) {
                LoopEnd = loopEnd;
            }
            else {
                _logger.Error($"'loopEnd' in {jsonPath} has an invalid value.");
            }
        }
    }

    public void SetId (int id) {
        Id = id;
    }
}
