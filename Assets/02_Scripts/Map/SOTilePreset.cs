using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TilePresetData
{
    [Header("타일 프리펩")]
    public MapTile tilePrefab;
    [Header("타일 위치")]
    public Vector2Int position;
}

[CreateAssetMenu(fileName = "SOTilePreset", menuName = "Scriptable Objects/Map/SOTilePreset")]
public class SOTilePreset : ScriptableObject
{
    [Header("타일 프리셋")]
    public List<TilePresetData> presetTiles = new();
}
