using System.Collections.Generic;
using UnityEngine;

public class MapGrid : MonoBehaviour
{
    public bool IsCollapsed { get; set; } = false;
    public MapTile[] TileOptions { get; set; }
    public int Index { get; private set; }

    public void CreateMapGrid(bool isCollapsed, List<MapTile> tileOptions, int index)
    {
        IsCollapsed = isCollapsed;
        TileOptions = tileOptions.ToArray();
        Index = index;
    }

    public void SetTileOptions(MapTile[] tileOptions)
    {
        TileOptions = tileOptions;
    }
}
