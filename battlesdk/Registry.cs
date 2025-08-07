using battlesdk.data;
using battlesdk.data.definitions;
using battlesdk.json;
using NLog;

namespace battlesdk;


public static class Registry {
    private delegate T BuildAsset<T> (string name, string fullPath);

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public const string FOLDER_GRAPHICS = "graphics";
    public const string FOLDER_FRAMES = "frames";
    public const string FOLDER_FONTS = "fonts";
    public const string FOLDER_LOCALIZATION = "localization";
    public const string FOLDER_MAPS = "maps";
    public const string FOLDER_MUSIC = "music";
    public const string FOLDER_WORLDS = "worlds";
    public const string FOLDER_SCRIPTS = "scripts";
    public const string FOLDER_SOUNDS = "sounds";
    public const string FOLDER_TILESETS = "tilesets";

    public static string ResFolderPath { get; private set; } = "res";

    public static Collection<LanguageAsset> Languages { get; } = new();
    public static Collection<FontAsset> Fonts { get; } = new();
    public static Collection<Tileset> Tilesets { get; } = new();
    public static Collection<MapAsset> Maps { get; } = new();
    public static Collection<WorldAsset> Worlds { get; } = new();
    public static Collection<SpriteFile> Sprites { get; } = new();
    public static Collection<MusicFile> Music { get; } = new();
    public static Collection<ScriptAsset> Scripts { get; } = new();
    public static Collection<AssetFile> Sounds { get; } = new();

    public static int FlagsTilesetIndex { get; private set; } = -1;

    /// <summary>
    /// The index of overworld characters' shadow in <see cref="MiscSprites"/>,
    /// or -1 if no such asset exists.
    /// </summary>
    public static int CharSpriteShadow { get; private set; } = -1; // TODO: Remove. The registry doesn't cache anything.

    /// <summary>
    /// Given an assets folder and a path to a specific asset, generates the
    /// name that would identify said asset. Keep in mind that, since names are
    /// based on the asset's file name, discarding its extension, having two
    /// assets with the same name in the same folder, even if their extension
    /// is different, is not supported.
    /// </summary>
    /// <param name="rootFolder">The folder that contains the assets of this
    /// asset's type.</param>
    /// <param name="assetPath">The path to the asset.</param>
    public static string GetAssetName (string rootFolder, string assetPath) {
        string rootPath = Path.GetFullPath(Path.Combine(ResFolderPath, rootFolder));
        string targetPath = Path.GetFullPath(assetPath);

        string relativePath = Path.GetRelativePath(rootPath, targetPath);

        string dir = Path.GetDirectoryName(relativePath)?.Replace("\\", "/") ?? "";
        string fileName = Path.GetFileNameWithoutExtension(relativePath);

        return string.IsNullOrEmpty(dir)
            ? fileName
            : $"{dir}/{fileName}";
    }

    /// <summary>
    /// Registers all the resources in the resources folder. This method must
    /// be called once before trying to read any data from the registry.
    /// </summary>
    public static void Init () {
        // NOTE: The order in which the resources are loaded is important, as
        // some resources depend on other resources.

        LoadLanguages();
        Localization.SetLanguage("en_US"); // TODO: Temporary hardcode.
        LoadSprites();
        LoadFonts();
        LoadMusic();
        LoadScripts();
        LoadSounds();
        LoadTilesets();
        LoadMaps(); // Maps require the tilesets they use to be loaded.
        LoadWorlds(); // Worlds require the maps they include to be loaded.

        foreach (var map in Maps) {
            map.Bind();
        }

        if (Sprites.TryGetId("misc/char_shadow", out var charShadowId)) {
            CharSpriteShadow = charShadowId;
        }
    }

    private static void LoadFonts () {
        LoadAssets(
            AssetType.Font,
            FOLDER_FONTS,
            [".ttf", ".otf"],
            Fonts,
            (name, path) => new FontAsset(name, path)
        );
    }

    private static void LoadTilesets () {
        LoadAssets(
            AssetType.Tileset,
            FOLDER_TILESETS,
            [".tsx"],
            Tilesets,
            (name, path) => new Tileset(name, path)
        );
    }

    private static void LoadMaps () {
        LoadAssets(
            AssetType.Map,
            FOLDER_MAPS,
            [".tmx"],
            Maps,
            (name, path) => new MapAsset(name, path)
        );
    }

    private static void LoadWorlds () {
        LoadAssets(
            AssetType.World,
            FOLDER_WORLDS,
            [".world"],
            Worlds,
            (name, path) => new WorldAsset(name, path)
        );
    }

    private static void LoadLanguages () {
        string dir = Path.Combine(ResFolderPath, FOLDER_LOCALIZATION);
        if (Directory.Exists(dir) == false) return;

        var mainFiles = new HashSet<string>();

        // Add files only if they don't contain dots in their name. A file like
        // "lang.toml" is the main file for the language "lang", while a file
        // like "important.lang.toml" is actually a secondary file for the
        // language "important"; instead of a language called "important.lang".
        foreach (var path in Directory.EnumerateFiles(
            dir,
            "*.toml",
            SearchOption.TopDirectoryOnly
        )) {
            var fileName = Path.GetFileName(path);
            var noExt = Path.GetFileNameWithoutExtension(fileName);

            if (noExt.Contains('.')) continue;

            mainFiles.Add(path);
        }

        foreach (var f in mainFiles) {
            try {
                var name = GetAssetName(FOLDER_LOCALIZATION, f);
                var el = new LanguageAsset(name, Path.GetFullPath(f));
                Languages.Register(el);

                LogLoadSuccess(AssetType.Language.ToString(), f);
            }
            catch (Exception ex) {
                LogLoadError(AssetType.Language.ToString(), f, ex);
            }
        }
    }

    private static void LoadSprites () {
        string dir = Path.Combine(ResFolderPath, FOLDER_GRAPHICS);
        if (Directory.Exists(dir) == false) return;

        var files = ReadSpriteFolder(dir);

        foreach (var f in files) {
            Sprites.Register(f);
        }
    }

    private static List<SpriteFile> ReadSpriteFolder (string folderPath) {
        List<SpriteFile> files = [];

        SpriteMetadataDefinition? baseMeta = null;

        var baseMetaPath = Path.Combine(folderPath, ".json");
        if (File.Exists(baseMetaPath)) {
            try {
                var txt = File.ReadAllText(baseMetaPath);
                baseMeta = Json.Parse<SpriteMetadataDefinition>(txt);
            }
            catch (Exception ex) {
                _logger.Error(ex, $"Failed to read base metadata file '{baseMetaPath}'");
            }
        }

        foreach (var f in EnumerateAssetFiles(folderPath, [".png"])) {
            try {
                var name = GetAssetName(FOLDER_GRAPHICS, f);
                var path = Path.GetFullPath(f);

                files.Add(SpriteFile.New(name, f, baseMeta));
                LogLoadSuccess("sprite", f);
            }
            catch (Exception ex) {
                LogLoadError("sprite", f, ex);
            }
        }

        foreach (var dir in Directory.EnumerateDirectories(folderPath)) {
            var dirFiles = ReadSpriteFolder(dir);
            files.AddRange(dirFiles);
        }

        return files;
    }

    private static void LoadScripts () {
        LoadAssets(
            AssetType.Script,
            FOLDER_SCRIPTS,
            [".lua"],
            Scripts,
            (name, path) => new ScriptAsset(name, path)
        );
    }

    private static void LoadMusic () {
        LoadAssets(
            AssetType.Music,
            FOLDER_MUSIC,
            [".ogg", ".mp3"],
            Music,
            (name, path) => new MusicFile(name, path)
        );
    }

    private static void LoadSounds () {
        LoadAssets(
            AssetType.Sound,
            FOLDER_SOUNDS,
            [".wav"],
            Sounds,
            (name, path) => new AssetFile(name, path)
        );
    }

    private static IEnumerable<string> EnumerateAssetFiles (
        string folderPath,
        IEnumerable<string>? extensions
    ) {
        foreach (var f in Directory.EnumerateFiles(
            folderPath,
            "*",
            SearchOption.TopDirectoryOnly
        )) {
            if (extensions is not null && extensions.Contains(Path.GetExtension(f)) == false) {
                continue;
            }

            yield return f;
        }
    }

    /// <summary>
    /// Loads all the assets in the folder given, assuming they are of a
    /// specific type, and adds them to the collection given.
    /// </summary>
    /// <typeparam name="T">The type of asset to load.</typeparam>
    /// <param name="assetType">The kind of asset to load.</param>
    /// <param name="folder">The folder, inside the resources directory, where
    /// the assets are located.</param>
    /// <param name="extensions">A list of extensions to accept, or
    /// <see cref="null"/> to accept all.</param>
    /// <param name="collection">The collection in the registry to which the
    /// assets will be added.</param>
    /// <param name="getter">A function that, given the name and full path of
    /// an asset file, returns the asset built from it.</param>
    private static void LoadAssets<T> (
        AssetType assetType,
        string folder,
        IEnumerable<string>? extensions,
        Collection<T> collection,
        BuildAsset<T> getter
    ) where T : IIdentifiable {
        foreach (string f in ScanDir(folder, extensions)) {
            try {
                var name = GetAssetName(folder, f);
                var el = getter(name, Path.GetFullPath(f));
                collection.Register(el);

                LogLoadSuccess(assetType.ToString(), f);
            }
            catch (Exception ex) {
                LogLoadError(assetType.ToString(), f, ex);
            }
        }
    }

    /// <summary>
    /// Scans the given asset directory (recursively), returning all the files
    /// whose extension matches one of the extensions provided. If extensions
    /// are not provided, all files will be returned.
    /// </summary>
    /// <param name="assetDir">The path to the asset folder INSIDE the resources
    /// folder.</param>
    /// <param name="extensions">A list of extensions to accept, or
    /// <see cref="null"/> to accept all.</param>
    private static IEnumerable<string> ScanDir (
        string assetDir, IEnumerable<string>? extensions = null
    ) {
        string dir = Path.Combine(ResFolderPath, assetDir);
        if (Directory.Exists(dir) == false) yield break;

        foreach (var f in Directory.EnumerateFiles(
            dir,
            "*",
            SearchOption.AllDirectories
        )) {
            if (extensions is null || extensions.Contains(Path.GetExtension(f))) {
                yield return f;
            }
        }
    }

    private static void LogLoadSuccess (string assetKind, string path) {
        _logger.Info($"Loaded {assetKind} '{path}'");
    }

    private static void LogLoadError (string assetKind, string path, Exception? ex) {
        _logger.Error(ex, $"Failed to load {assetKind} '{path}'.");
        ex.PrintFancy();
    }
}

public enum AssetType {
    Sprite,
    Font,
    Language,
    Map,
    Music,
    Script,
    Sound,
    Tileset,
    World,
}