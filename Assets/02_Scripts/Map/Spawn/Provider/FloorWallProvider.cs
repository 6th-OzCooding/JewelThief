using System.Collections.Generic;
using UnityEngine;

public class FloorWallProvider : ISpawnPositionProvider
{
    private readonly int _spawnTryCount = 50;

    private readonly float _floorRayDistance = 5f;

    private readonly float _wallRayHeight = 0.6f;
    private readonly float _wallRayDistance = 1.5f;

    // 벽에서 얼마나 떨어진 곳에 오브젝트 중심을 둘 것인지
    // 프리팹 pivot이 중앙이고 깊이가 1이면 0.5 정도가 적절함
    private readonly float _wallOffset = 0f;

    // 바닥에서 얼마나 띄울 것인지
    // pivot이 바닥에 있으면 0, pivot이 중앙이면 오브젝트 높이의 절반
    private readonly float _floorOffset = 0f;

    private readonly LayerMask _floorLayer;
    private readonly LayerMask _wallLayer;
    private readonly LayerMask _obstacleLayer;

    private readonly Vector3 _checkHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);
    private Vector3[] _directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

    public FloorWallProvider()
    {
        _floorLayer = LayerMask.GetMask("Floor");
        _wallLayer = LayerMask.GetMask("Wall");
        _obstacleLayer = LayerMask.GetMask("Obstacle");
    }

    public bool GetSpawnInfo(IReadOnlyList<SpawnArea> spawnAreas, out SpawnInfo spawnInfo)
    {
        for (int i = 0; i < _spawnTryCount; i++)
        {
            SpawnArea area = spawnAreas[Random.Range(0, spawnAreas.Count)];
            Vector3 randomPoint = area.GetRandomPosition();

            if (!TryGetFloor(randomPoint, out RaycastHit baseFloorHit))
                continue;

            Vector3 basePosition = baseFloorHit.point;

            int startDirectionIndex = Random.Range(0, _directions.Length);

            for (int j = 0; j < _directions.Length; j++)
            {
                Vector3 dir = _directions[startDirectionIndex];

                Vector3 wallRayOrigin = basePosition + Vector3.up * _wallRayHeight;

                if (!Physics.Raycast(wallRayOrigin, dir, out RaycastHit wallHit, _wallRayDistance, _wallLayer, QueryTriggerInteraction.Ignore))
                    continue;

                Vector3 wallNormal = wallHit.normal;

                // 벽 표면에서 방 안쪽으로 살짝 민 위치
                Vector3 candidatePosition = wallHit.point + wallNormal * _wallOffset;

                // 최종 위치 아래에 실제 Floor가 있는지 다시 검사
                Vector3 finalFloorCheckPoint = candidatePosition;

                if (!TryGetFloor(finalFloorCheckPoint, out RaycastHit finalFloorHit))
                    continue;

                candidatePosition.y = finalFloorHit.point.y + _floorOffset;

                // 현재 WallProvider와 같은 회전 convention 사용
                // prefab의 local forward 방향이 반대로 되어 있으면 wallNormal로 바꾸면 됨
                Quaternion rotation = Quaternion.LookRotation(wallNormal, Vector3.up);

                if (Physics.CheckBox(candidatePosition, _checkHalfExtents, rotation, _obstacleLayer, QueryTriggerInteraction.Ignore))
                    continue;

                spawnInfo = new SpawnInfo(candidatePosition, rotation);
                return true;
            }
        }

        spawnInfo = default;
        return false;
    }

    private bool TryGetFloor(Vector3 startPosition, out RaycastHit hit)
    {
        return Physics.Raycast(startPosition, Vector3.down, out hit, _floorRayDistance, _floorLayer, QueryTriggerInteraction.Ignore);
    }
}
