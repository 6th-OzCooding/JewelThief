using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AI;

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
        IReadOnlyList<SpawnArea> areas = _registry.GetAreas(AreaType.Floor);
        if (!_providers.TryGetValue(AreaType.Floor, out ISpawnPositionProvider provider))
        {
            Debug.LogWarning("Floor 스폰위치를 찾을 수 없습니다.");
            return;
        }

        StageEnemyData stageEnemyData = GameManager.DataTable.GetStageEnemyData(_stageData.StageEnemyId);

        if (stageEnemyData == null)
        {
            Debug.LogError("StageEnemyData를 찾을 수 없습니다.");
            return;
        }

        for (int i = 0; i < stageEnemyData.EnemyName.Count; i++)
        {
            string enemyPoolAddress = stageEnemyData.EnemyName[i];
            int monsterCount = stageEnemyData.EnemyCount[i];

            int spawnedCount = 0;
            int maxRetries = 30;
            int currentTry = 0;

            while (spawnedCount < monsterCount && currentTry < maxRetries)
            {
                currentTry++;
                if (!provider.GetSpawnInfo(areas, out SpawnInfo spawnInfo))
                {
                    Debug.LogWarning("스폰 위치를 찾지 못했습니다.");
                    continue;
                }

                if (NavMesh.SamplePosition(spawnInfo.Position, out NavMeshHit hit, 10.0f, NavMesh.AllAreas))
                {
                    GameManager.Pool.SpawnFromPool(enemyPoolAddress, hit.position);
                    spawnedCount++;
                }

                else
                {
                    Debug.LogError("제대로 된 위치를 찾지 못하여 재시도 합니다.");
                }
            }
        }
    }
}