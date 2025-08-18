using NLog;
using System.Collections.Immutable;
using System.Text;
using Tomlyn;
using Tomlyn.Model;

namespace battlesdk;

public static class Localization {
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static ImmutableDictionary<string, string> _text
        = new Dictionary<string, string>().ToImmutableDictionary();

    private static readonly Dictionary<string, Func<string>> _placeholderValues = new() {
        ["player"] = () => G.PlayerName,
        ["money"] = () => G.Money.ToString(),
    };

    public static int CurrentLanguageId { get; private set; } = -1;

    /// <summary>
    /// Given the key to a localized string, returns the string in the current
    /// language of the game. If such key doesn't exist, returns the key itself.
    /// </summary>
    /// <param name="key">The key that identifies the localized string.</param>
    /// <param name="args">A list of arguments to fill '%' placeholders.</param>
    public static string Text (string key, params object[] args) {
        if (_text.TryGetValue(key, out var value)) return FillPlaceholders(value, args);

        return FillPlaceholders(key, args);
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

    public static string FillPlaceholders (string template, params object[] args) {
        var tokens = Tokenize(template);
        var sb = new StringBuilder();

        foreach (var tok in tokens) {
            if (tok.IsPlaceholder == false) {
                sb.Append(tok.Text);
            }
            else {
                if (tok.Text[0] == '%' && int.TryParse(tok.Text[1..], out int argIndex)) {
                    if (argIndex < args.Length) {
                        sb.Append(args[argIndex]);
                    }
                    else {
                        _logger.Error($"Value placeholder index {argIndex} is out of bouds.");
                        sb.Append(tok.Text);
                    }
                }
                else if (_placeholderValues.TryGetValue(tok.Text, out var strProvider)) {
                    sb.Append(strProvider());
                }
                else {
                    _logger.Error($"Invalid placeholder name: {tok.Text}.");
                    sb.Append(tok.Text);
                }
            }
        }

        return sb.ToString();
    }

    public static List<TextToken> Tokenize (string template) {
        var tokens = new List<TextToken>();
        var sb = new StringBuilder();
        bool inPlaceholder = false;

        for (int i = 0; i < template.Length; i++) {
            char c = template[i];
            if (inPlaceholder) {
                if (c == '{') {
                    _logger.Error("Cannot use '{' inside a placeholder value.");
                    continue;
                }
                else if (c == '}') {
                    if (sb.Length > 0) {
                        tokens.Add(new(true, sb.ToString()));
                        sb.Clear();
                    }
                    inPlaceholder = false;
                    continue;
                }
            }
            else {
                if (c == '{') {
                    if (i + 1 < template.Length && template[i + 1] == '{') {
                        sb.Append('{');
                        i++;
                    }
                    else {
                        if (sb.Length > 0) {
                            tokens.Add(new(false, sb.ToString()));
                            sb.Clear();
                        }
                        inPlaceholder = true;
                    }
                    continue;
                }
                else if (c == '}') {
                    if (i + 1 < template.Length && template[i + 1] == '}') {
                        sb.Append('}');
                        i++;
                    }
                    else {
                        _logger.Warn("'}' token at unexpected position.");
                        sb.Append('}');
                    }
                    continue;
                }
            }

            sb.Append(c);
        }

        if (inPlaceholder) {
            _logger.Error("Unterminated placeholder value.");
        }
        else if (sb.Length > 0) {
            tokens.Add(new(false, sb.ToString()));
        }

        return tokens;
    }
}

public record TextToken (bool IsPlaceholder, string Text);
