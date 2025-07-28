namespace battlesdk.data;

public class AssetFile : INameable {
    /// <summary>
    /// The name of the file.
    /// </summary>
    public string Name { get; private init; }
    /// <summary>
    /// The absolute path to the file.
    /// </summary>
    public string Path { get; private init; }

    public AssetFile (string name, string path) {
        Name = name;
        Path = path;
    }
}
