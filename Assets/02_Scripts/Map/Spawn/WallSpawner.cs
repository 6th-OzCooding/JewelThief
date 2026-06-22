using UnityEngine;

public class WallSpawner : MonoBehaviour
{
    [SerializeField] private int _spawnCount = 3;
    [SerializeField] private int _spawnTryCount = 30;
    [SerializeField] private SpawnArea[] _spawnArea;
    [SerializeField] private GameObject _tempPrefab;
    [SerializeField] private LayerMask _wallLayer;
    [SerializeField] private LayerMask _obstacleLayer;
    [SerializeField] private float _rayDistance = 20f;
    [SerializeField] private Vector3 _checkHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);

    private int _spawnedCount = 0;

    public void SpawnObjectFromWall()
    {
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

        for (int i = 0; i < _spawnTryCount; i++)
        {
            SpawnArea volume = _spawnArea[Random.Range(0, _spawnArea.Length)];
            Vector3 randomPoint = volume.GetRandomPosition();

            Vector3 dir = directions[Random.Range(0, directions.Length)];

            if (!Physics.Raycast(randomPoint, dir, out RaycastHit hit, _rayDistance, _wallLayer))
                continue;

            Vector3 spawnPos = hit.point;
            Quaternion spawnRot = Quaternion.LookRotation(-hit.normal);

            if (Physics.CheckBox(spawnPos, _checkHalfExtents, spawnRot, _obstacleLayer))
                continue;

            Instantiate(_tempPrefab, spawnPos, spawnRot);
            //_spawnedCount++;
            return;
        }
    }
}
