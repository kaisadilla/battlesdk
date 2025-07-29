using battlesdk.data;

namespace battlesdk;

public static class Registry {
    public static Collection<Tileset> Tilesets { get; } = new();
    public static Collection<Map> Maps { get; } = new();
    public static Collection<AssetFile> CharSprites { get; } = new();
    public static Collection<AssetFile> Sounds { get; } = new();

    public static int FlagsTilesetIndex { get; private set; } = -1;
}
