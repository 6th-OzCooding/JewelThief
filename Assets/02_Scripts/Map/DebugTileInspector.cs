using System.Collections.Generic;
using UnityEngine;

public class DebugTileInspector : MonoBehaviour
{
    public bool isOnOffCeiling = true;
    private bool preValue = false;

    List<MapTile> _tiles = new List<MapTile>();

    public void AddTile(MapTile tile)
    {
        _tiles.Add(tile);
    }

    private void OnValidate()
    {
        if (preValue == isOnOffCeiling) return;


        foreach (var tile in _tiles)
        {
            if (tile == null) continue;
            tile.SetCeilingGO(isOnOffCeiling);
        }

        preValue = isOnOffCeiling;
    }
}
