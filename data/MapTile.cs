namespace battlesdk.data;

public class MapTile {
    public required int TilesetId { get; init; }
    public required int TileId { get; init; }
    public required TileProperties Properties { get; init; }
    public int ZIndex { get; init; } = 0;
    public List<TileFlag> Flags { get; init; } = [];
}