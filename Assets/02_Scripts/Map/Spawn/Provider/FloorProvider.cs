using System.Collections.Generic;
using UnityEngine;

public class FloorProvider : ISpawnPositionProvider
{
    private int _spawnTryCount = 30;
    private float _rayDistance = 3f;

    private LayerMask _targetLayer;
    private LayerMask _obstacleLayer;

    private Vector3 _checkHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);

    public FloorProvider()
    {
        _targetLayer = LayerMask.GetMask("Floor");
        _obstacleLayer = LayerMask.GetMask("Obstacle");
    }

    public bool GetSpawnInfo(IReadOnlyList<SpawnArea> spawnAreas, out SpawnInfo transform)
    {
        for (int i = 0; i < _spawnTryCount; i++)
        {
            SpawnArea area = spawnAreas[Random.Range(0, spawnAreas.Count)];
            Vector3 randomPoint = area.GetRandomPosition();

            if (!Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, _rayDistance, _targetLayer))
                continue;

            Vector3 position = hit.point + hit.normal;

            if (Physics.CheckBox(position, _checkHalfExtents, Quaternion.identity, _obstacleLayer))
                continue;

            transform = new SpawnInfo(position, Quaternion.identity);
            return true;
        }

        transform = default;
        return false;
    }
}
