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
            MapSpawnObjectType.Tool => stageData.MaxTool,
            MapSpawnObjectType.Jewel => stageData.MaxJewel,
            MapSpawnObjectType.Junk => stageData.MaxJunk,
            MapSpawnObjectType.Statue => stageData.MaxStatue,
            MapSpawnObjectType.Trap => stageData.MaxTrap,
            MapSpawnObjectType.Container => stageData.MaxContainer,
            MapSpawnObjectType.Enemy => stageData.MaxEnemy,
            MapSpawnObjectType.Painting => stageData.MaxPainting,
            _ => 0
        };
    }
}
