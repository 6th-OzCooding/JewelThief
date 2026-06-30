using System.Collections.Generic;

public struct MapSpawnRequest
{
    public MapSpawnData SpawnData;
    public int SpawnCount;

    public MapSpawnRequest(MapSpawnData spawnData, int spawnCount)
    {
        SpawnData = spawnData;
        SpawnCount = spawnCount;
    }
}

public class MapSpawnRequestBuilder
{
    public List<MapSpawnRequest> Build(IReadOnlyDictionary<string, MapSpawnData> spawnDataTable, StageData stageData)
    {
        var requests = new List<MapSpawnRequest>();

        foreach (var spawnDataKV in spawnDataTable)
        {
            MapSpawnData spawnData = spawnDataKV.Value;
            MapSpawnObjectType objectType = spawnData.GetMapSpawnObjectType();

            int spawnCount = GetSpawnCount(objectType, stageData);

            requests.Add(new MapSpawnRequest(spawnData, spawnCount));
        }

        return requests;
    }

    private int GetSpawnCount(MapSpawnObjectType type, StageData stageData)
    {
        return type switch
        {
            MapSpawnObjectType.Jewel => stageData.MaxJewel,
            MapSpawnObjectType.Junk => stageData.MaxJunk,
            MapSpawnObjectType.Statue => stageData.MaxStatue,
            MapSpawnObjectType.FloorTrap => stageData.MaxFloorTrap,
            MapSpawnObjectType.CeilingTrap => stageData.MaxCeilingTrap,
            MapSpawnObjectType.FloorContainer => stageData.MaxFloorContainer,
            MapSpawnObjectType.WallContainer => stageData.MaxWallContainer,
            MapSpawnObjectType.Painting => stageData.MaxPainting,
            _ => 0
        };
    }
}
