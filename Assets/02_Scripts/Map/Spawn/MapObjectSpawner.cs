using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEditor.VersionControl;
using UnityEngine;


public class MapObjectSpawner
{
    private SpawnAreaRegister _registry;
    private MapSpawnRequestBuilder _requestBuilder;

    private Dictionary<AreaType, ISpawnPositionProvider> _providers;
    private StageData _stageData;

    public MapObjectSpawner()
    {
        _registry = new();
        _requestBuilder = new();

        _providers = new()
        {
            { AreaType.Floor, new FloorProvider() },
            { AreaType.Wall, new WallProvider() },
            { AreaType.Ceiling, new CeilingProvider() },
            { AreaType.FloorWall, new FloorWallProvider() }
        };

        GameManager.Alert.OnTimeUp += SpawnEnemy;
    }

    public void ObjectSpawnAfterMapGenerated(IEnumerable<MapTile> generatedTiles)
    {
        _registry.Clear();
        _registry.RegisterFromRoot(generatedTiles);

        var mapSpawnData = GameManager.DataTable.GetMapSpawnDataTable();
        _stageData = GameManager.DataTable.GetStageData(GameManager.Instance.SelectedStageId);

        List<MapSpawnRequest> requests = _requestBuilder.Build(mapSpawnData, _stageData);

        foreach (MapSpawnRequest request in requests)
        {
            Spawn(request);
        }
    }

    private void Spawn(MapSpawnRequest request)
    {
        if (!_providers.TryGetValue(request.SpawnData.GetAreaType(), out ISpawnPositionProvider provider))
        {
            Debug.LogWarning($"잘못된 스폰 영역: {request.SpawnData.GetAreaType()}");
            return;
        }

        IReadOnlyList<SpawnArea> areas = _registry.GetAreas(request.SpawnData.GetAreaType());
        int spawnedCount = 0;

        for (int i = 0; i < request.SpawnCount; i++)
        {
            if (!provider.GetSpawnInfo(areas, out SpawnInfo spawnInfo))
            {
                Debug.LogWarning($"스폰 Transform 얻기 실패: {request.SpawnData.Id}");
                continue;
            }

            if (!TrySpawn(request.SpawnData, spawnInfo.Position, spawnInfo.Rotation, spawnInfo.IsKinematic))
            {
                Debug.LogWarning($"스폰 실패: {request.SpawnData.Id}");
                continue;
            }

            spawnedCount++;
        }
    }

    private bool TrySpawn(MapSpawnData data, Vector3 position, Quaternion rotation, bool isKinematic = false)
    {
        string itemId = data.ItemId[Random.Range(0, data.ItemId.Count)];
        string poolAddress = data.PoolAddress;

        switch (data.GetInteractType())
        {
            case InteractType.Interact:
                GameManager.Pool.SpawnFromPool<BaseInteractableObject>(poolAddress, position, rotation).InitFromSpawner(itemId, isKinematic);
                return true;
            case InteractType.Disarm:
                GameManager.Pool.SpawnFromPool<BaseDisarmableObejct>(poolAddress, position, rotation).InitFromSpawner(itemId);
                return true;
            default:
                Debug.Log("잘못된 상호작용 타입: " + data.GetInteractType());
                return true;
        }
    }

    private void SpawnEnemy()
    {
        Debug.Log("적 스폰 시작");
        IReadOnlyList<SpawnArea> areas = _registry.GetAreas(AreaType.Floor);
        _providers.TryGetValue(AreaType.Floor, out ISpawnPositionProvider provider);

        for (int i = 0; i < _stageData.MaxEnemies.Count; i++)
        {
            int monsterCount = _stageData.MaxEnemies[i];
            int spawnedCount = 0;
            string enemyId = "Enemy_0" + $"{i + 1}";
            string enemyPoolAddress = GameManager.DataTable.GetEnemyData(enemyId).Name;

            for (int j = 0; j < monsterCount; j++)
            {
                if (!provider.GetSpawnInfo(areas, out SpawnInfo spawnInfo))
                {
                    Debug.LogWarning("스폰 Transform 얻기 실패");
                    continue;
                }

                GameManager.Pool.SpawnFromPool(enemyPoolAddress, spawnInfo.Position);

                spawnedCount++;
            }
        }
    }
}