using System.Collections.Generic;
using UnityEngine;

public class CeilingProvider : ISpawnPositionProvider
{
    private readonly int _spawnTryCount = 30;
    private readonly float _rayDistance = 5f;

    private readonly LayerMask _ceilingLayer;
    private readonly LayerMask _obstacleLayer;

    private readonly Vector3 _checkHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);

    public CeilingProvider()
    {
        _ceilingLayer = LayerMask.GetMask("Ceiling");
        _obstacleLayer = LayerMask.GetMask("Obstacle");
    }

    public SpawnInfo GetSpawnInfo(IReadOnlyList<SpawnArea> spawnAreas)
    {
        SpawnInfo spawnInfo = default;

        for (int i = 0; i < _spawnTryCount; i++)
        {
            SpawnArea area = spawnAreas[Random.Range(0, spawnAreas.Count)];
            Vector3 randomPoint = area.GetRandomPosition();

            if (!Physics.Raycast(randomPoint, Vector3.up, out RaycastHit hit, _rayDistance, _ceilingLayer, QueryTriggerInteraction.Ignore))
                continue;

            // 천장 아래면의 normal은 보통 Vector3.down 방향
            Vector3 position = hit.point;

            // 프리팹의 local up 방향을 천장 normal 방향으로 맞춤
            // 즉, 기본적으로 세워진 오브젝트를 천장에 거꾸로 붙이는 회전
            //Quaternion rotation = Quaternion.FromToRotation(Vector3.up, -hit.normal);

            if (Physics.CheckBox(position, _checkHalfExtents, Quaternion.identity, _obstacleLayer, QueryTriggerInteraction.Ignore))
                continue;

            spawnInfo = new SpawnInfo(position, Quaternion.identity);
        }

        return spawnInfo;
    }
}
