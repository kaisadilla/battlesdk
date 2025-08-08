namespace battlesdk.data;

public class MapTile {
    public required int TilesetId { get; init; }
    public required int TileId { get; init; }
    public required TileProperties Properties { get; init; }

    public MapTile? OnStepOverTile { get; init; } = null;
    public MapTile? OnStepUnderTile { get; init; } = null;
}