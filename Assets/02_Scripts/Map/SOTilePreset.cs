using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TilePresetData
{
    [Header("타일 프리펩")]
    public MapTile tilePrefab;
    [Header("타일 위치")]
    public Vector2Int position;
    public bool IsStartTile;
}

[CreateAssetMenu(fileName = "SOTilePreset", menuName = "Scriptable Objects/Map/SOTilePreset")]
public class SOTilePreset : ScriptableObject
{
    [Header("맵 크기")]
    public int MapWidth = 10;
    [Header("맵 생성 시도 횟수")]
    public int MaxGenerateRetryCount = 20;    // 맵 생성 실패 시 재시도 횟수
    [Header("최소 도달해야 할 타일 수")]
    public int MinReachableTileCount = 30;    // 도달 가능한 타일 수 최소값
    [Header("최소 도달해야 할 방 수")]
    public int MinReachableRoomCount = 5;     // 도달 가능한 방 개수 최소값
    [Header("방 비율")]
    public float MaxRoomRatio = 0.5f; // 방 비율 (0~1)
    [Header("타일 프리셋")]
    public List<TilePresetData> presetTiles = new();

}
