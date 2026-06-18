using UnityEngine;

public class MapGrid : MonoBehaviour
{
    public bool IsCollapsed { get; set; } = false;
    public MapTile[] TileOptions { get; set; }
    public int Index { get; private set; }

    public void CreateMapGrid(bool isCollapsed, MapTile[] tileOptions, int index)
    {
        IsCollapsed = isCollapsed;
        TileOptions = tileOptions;
        Index = index;
    }

    public void SetTileOptions(MapTile[] tileOptions)
    {
        TileOptions = tileOptions;
    }
}
