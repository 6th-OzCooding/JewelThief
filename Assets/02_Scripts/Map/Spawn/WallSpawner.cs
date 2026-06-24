using System.Collections.Generic;
using UnityEngine;

public class WallSpawner
{
    private int _spawnCount = 3;
    private int _spawnTryCount = 30;

    private float _rayDistance = 20f;

    private LayerMask _wallLayer;
    private LayerMask _obstacleLayer;

    private Vector3 _checkHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);
    private Vector3[] _directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };


    public int SpawnObjectFromWall(IReadOnlyList<SpawnArea> spawnAreas)
    {
        if (null == spawnAreas)
        {
            Debug.LogWarning("Wall SpawnArea가 없습니다.");
            return 0;
        }
        
        if (spawnAreas.Count == 0)
        {
            Debug.LogWarning("Wall SpawnArea에 스폰할 수 있는 영역이 없습니다.");
            return 0;
        }
        int spawnedCount = 0;

        for (int j = 0; j < _spawnCount; j++)
        {
            bool result = TrySpawn(spawnAreas);

            if (result)
                spawnedCount++;
        }

        return spawnedCount;
    }

    private bool TrySpawn(IReadOnlyList<SpawnArea> spawnAreas)
    {
        for (int i = 0; i < _spawnTryCount; i++)
        {
            SpawnArea area = spawnAreas[Random.Range(0, spawnAreas.Count)];
            Vector3 randomPoint = area.GetRandomPosition();

            Vector3 dir = _directions[Random.Range(0, _directions.Length)];

            if (!Physics.Raycast(randomPoint, dir, out RaycastHit hit, _rayDistance, _wallLayer))
                continue;

            Vector3 spawnPos = hit.point;
            Quaternion spawnRot = Quaternion.LookRotation(-hit.normal);

            if (Physics.CheckBox(spawnPos, _checkHalfExtents, spawnRot, _obstacleLayer))
                continue;

            // TODO 네번째 매개변수에 mapRoot 넣기
            GameObject.Instantiate(Utils.ResourcesLoad<GameObject>("TestMapObject"), spawnPos, spawnRot);
            return true;
        }

        return false;
    }
}
