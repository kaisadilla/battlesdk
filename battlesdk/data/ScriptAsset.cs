namespace battlesdk.data;
public class ScriptAsset : AssetFile {
    private string? _source = null;

    public ScriptAsset (string name, string path) : base(name, path) {}

    public string GetSource () {
        if (_source is not null) return _source;

        if (File.Exists(Path) == false) {
            throw new Exception($"File '{Path}' no longer exists.");
        }

        _source = File.ReadAllText(Path);

        return _source;
    }
}
