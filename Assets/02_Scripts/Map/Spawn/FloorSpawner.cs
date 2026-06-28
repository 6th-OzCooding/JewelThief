using System.Collections.Generic;
using UnityEngine;

public enum FloorSpawnerDirection
{
    Up,
    Down,
}

public class FloorSpawner
{
    private int _spawnCount = 3;
    private int _spawnTryCount = 30;

    private float _rayDistance = 20f;

    private LayerMask _targetLayer;
    private LayerMask _obstacleLayer;

    private Vector3 _checkHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);
    private Vector3 _rayDirection;

    private FloorSpawnerDirection _direction;

    public FloorSpawner(FloorSpawnerDirection direction)
    {
        _direction = direction;
        _rayDirection = _direction == FloorSpawnerDirection.Down ? Vector3.down : Vector3.up;

        if(_direction == FloorSpawnerDirection.Down)
            _targetLayer = LayerMask.GetMask("Floor");
        else
            _targetLayer = LayerMask.GetMask("Ceiling");

        _obstacleLayer = LayerMask.GetMask("Obstacle");
    }


    public int SpawnObjectFromFloor(IReadOnlyList<SpawnArea> spawnAreas)
    {
        if(null == spawnAreas)
        {
            Debug.LogWarning("Floor SpawnArea가 없습니다.");
            return 0;
        }

        if(spawnAreas.Count == 0)
        {
            Debug.LogWarning("Floor SpawnArea에 스폰할 수 있는 영역이 없습니다.");
            return 0;
        }

        int spawnedCount = 0;

        for (int j = 0; j < _spawnCount; j++)
        {
            bool result = TrySpawn(spawnAreas);

            if(result)
                spawnedCount++;
        }

        return spawnedCount;
    }

    private bool TrySpawn(IReadOnlyList<SpawnArea> spawnAreas)
    {
        GameObject prefab = Utils.ResourcesLoad<GameObject>("TestMapObject");
        if (prefab == null)
        {
            Debug.LogWarning("TestMapObject 프리팹을 찾을 수 없습니다. Resources 경로를 확인하세요.");
            return false;
        }

        for (int i = 0; i < _spawnTryCount; i++)
        {
            SpawnArea area = spawnAreas[Random.Range(0, spawnAreas.Count)];
            Vector3 randomPoint = area.GetRandomPosition();

            if (!Physics.Raycast(randomPoint, _rayDirection, out RaycastHit hit, _rayDistance, _targetLayer))
                continue;

            Vector3 spawnPos = hit.point;

            if (Physics.CheckBox(spawnPos, _checkHalfExtents, Quaternion.identity, _obstacleLayer))
                continue;

            // TODO 네번째 매개변수에 mapRoot 넣기
            GameObject.Instantiate(Utils.ResourcesLoad<GameObject>("TestMapObject"), spawnPos, Quaternion.identity);
            return true;
        }

        return false;
    }
}

