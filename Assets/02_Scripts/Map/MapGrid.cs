using UnityEngine;

public class MapGrid : MonoBehaviour
{
    public bool IsCollapsed { get; set; } = false;
    public MapTile[] TileOptions { get; set; }

    public void CreateMapGrid(bool isCollapsed, MapTile[] tileOptions)
    {
        IsCollapsed = isCollapsed;
        TileOptions = tileOptions;
    }

    public void SetTileOptions(MapTile[] tileOptions)
    {
        TileOptions = tileOptions;
    }
}
