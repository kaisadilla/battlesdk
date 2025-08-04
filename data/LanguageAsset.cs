using NLog;
using Tomlyn;

namespace battlesdk.data;
public class LanguageAsset : AssetFile {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The name of the language in the language itself. 
    /// </summary>
    public string LocalName { get; private init; }
    /// <summary>
    /// The name of the language in English.
    /// </summary>
    public string EnglishName { get; private init; }

    public LanguageAsset (string name, string path) : base(name, path) {
        LocalName = name;
        EnglishName = name;

        var file = File.ReadAllText(path);
        var toml = Toml.ToModel(file);

        if (
            toml.TryGetValue("local_name", out var localNameObj)
            && localNameObj is string localName
        ) {
            LocalName = localName;
            EnglishName = localName;
        }
        else {
            _logger.Warn(
                $"Language file '{path}' is missing field 'local_name' of type String."
            );
        }

        if (
            toml.TryGetValue("english_name", out var englishNameObj)
            && englishNameObj is string englishName
        ) {
            EnglishName = englishName;
        }
        else {
            _logger.Warn(
                $"Language file '{path}' is missing field 'english_name' of type String."
            );
        }
    }
}

file class MetadataFile {
    public string? DisplayName { get; init; }
    public int? Size { get; init; }
    public int? LineHeight { get; init; }
}