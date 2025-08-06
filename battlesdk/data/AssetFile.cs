namespace battlesdk.data;

public class AssetFile : IIdentifiable {
    public string Name { get; private init; }
    public int Id { get; private set; }
    /// <summary>
    /// The absolute path to the file.
    /// </summary>
    public string Path { get; private init; }

    public AssetFile (string name, string path) {
        Name = name;
        Path = path;
    }

    public void SetId (int id) {
        Id = id;
    }
}
