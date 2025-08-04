using NLog;
using System.Collections.Immutable;
using Tomlyn;
using Tomlyn.Model;

namespace battlesdk;

public static class Localization {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static ImmutableDictionary<string, string> _text
        = new Dictionary<string, string>().ToImmutableDictionary();

    public static int CurrentLanguageId { get; private set; } = -1;

    /// <summary>
    /// Given the key to a localized string, returns the string in the current
    /// language of the game. If such key doesn't exist, returns the key itself.
    /// </summary>
    /// <param name="key">The key that identifies the localized string.</param>
    public static string Text (string key) {
        if (_text.TryGetValue(key, out var value)) return value;

        return key;
    }

    /// <summary>
    /// Sets the game's language to the one given. If desired, a backup language
    /// can be indicated. In this case, strings missing in the chosen language
    /// will be supplied by the backup language.
    /// </summary>
    /// <param name="name">The name that identifies the language in the registry.</param>
    /// <param name="backupLang">The name of a language to use as backup.</param>
    public static void SetLanguage (string name, string? backupLang = null) {
        var dict = new Dictionary<string, string>();
        if (backupLang != null) {
            AddLanguage(dict, backupLang);
        }

        CurrentLanguageId = AddLanguage(dict, name);

        _text = dict.ToImmutableDictionary();
    }

    /// <summary>
    /// Adds the keys existing in a language to the dictionary given. Returns
    /// the id of the language added.
    /// </summary>
    /// <param name="dict">The dictionary to which to add values.</param>
    /// <param name="lang">The language to add.</param>
    /// <returns>The id of the language added.</returns>
    private static int AddLanguage (Dictionary<string, string> dict, string lang) {
        if (Registry.Languages.TryGetElementByName(lang, out var asset) == false) {
            _logger.Error($"Language '{lang}' does not exist.");
            return -1;
        }

        var dir = Path.GetDirectoryName(asset.Path);
        if (dir is null) {
            _logger.Error($"Invalid directory obtained from '{asset.Path}'.");
            return -1;
        }

        foreach (var f in Directory.EnumerateFiles(
            dir,
            "*.toml",
            SearchOption.TopDirectoryOnly
        )) {
            var fileName = Path.GetFileName(f);

            if (fileName == $"{lang}.json" || fileName.StartsWith(lang + ".")) {
                string txt = File.ReadAllText(f);
                var toml = Toml.ToModel(txt);
                AddTomlTable(dict, toml);
            }
        }

        return asset.Id;
    }

    /// <summary>
    /// Adds the contents of a TOML table, including any nested tables, to the
    /// dictionary given.
    /// </summary>
    /// <param name="dict">The dictionary to which to add values.</param>
    /// <param name="tbl">The TOML table to read.</param>
    /// <param name="prefix">The prefix of the table given, if it's not the
    /// root table.</param>
    private static void AddTomlTable (
        Dictionary<string, string> dict, TomlTable tbl, string prefix = ""
    ) {
        foreach (var kv in tbl) {
            string key = prefix == "" ? kv.Key : prefix + "." + kv.Key;

            if (kv.Value is TomlTable nested) {
                AddTomlTable(dict, nested, key);
            }
            else if (kv.Value is string str) {
                dict[key] = str;
            }
        }
    }
}
