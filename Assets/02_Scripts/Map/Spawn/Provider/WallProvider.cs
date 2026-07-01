using System.Collections.Generic;
using UnityEngine;

public class WallProvider : ISpawnPositionProvider
{
    private int _spawnTryCount = 30;
    private float _rayDistance = 1.5f;
    private readonly float _wallOffset = 0f;

    private LayerMask _wallLayer;
    private LayerMask _obstacleLayer;

    private Vector3 _checkHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);
    private Vector3[] _directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

    public WallProvider()
    {
        _wallLayer = LayerMask.GetMask("Wall");
        _obstacleLayer = LayerMask.GetMask("Obstacle");
    }

    public SpawnInfo GetSpawnInfo(IReadOnlyList<SpawnArea> spawnAreas)
    {
        SpawnInfo spawnInfo = default;

        for (int i = 0; i < _spawnTryCount; i++)
        {
            SpawnArea area = spawnAreas[Random.Range(0, spawnAreas.Count)];
            Vector3 randomPoint = area.GetRandomPosition();

            var dir = _directions[Random.Range(0, _directions.Length)];

            if (!Physics.Raycast(randomPoint, dir, out RaycastHit hit, _rayDistance, _wallLayer, QueryTriggerInteraction.Ignore))
                continue;

            Vector3 position = hit.point + hit.normal * _wallOffset;
            // 오브젝트의 forward 방향, 로컬 Z축이 안 쪽임
            Quaternion rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);

            if (Physics.CheckBox(position, _checkHalfExtents, rotation, _obstacleLayer, QueryTriggerInteraction.Ignore))
                continue;

            spawnInfo = new SpawnInfo(position, rotation);
        }

        return spawnInfo;
    }
}
