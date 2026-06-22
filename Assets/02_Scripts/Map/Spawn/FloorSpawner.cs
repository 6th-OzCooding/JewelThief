using UnityEngine;

public class FloorSpawner : MonoBehaviour
{
    [SerializeField] private int _spawnCount = 3;
    [SerializeField] private int _spawnTryCount = 30;
    [SerializeField] private SpawnArea[] _spawnArea;
    [SerializeField] private GameObject _tempPrefab;
    [SerializeField] private LayerMask _floorLayer;
    [SerializeField] private LayerMask _obstacleLayer;
    [SerializeField] private float _rayDistance = 20f;
    [SerializeField] private Vector3 _checkHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);

    private int _spawnedCount = 0;

    public void SpawnObjectFromFloor()
    {
        while (_spawnedCount < _spawnCount)
        {
            for (int i = 0; i < _spawnTryCount; i++)
            {
                SpawnArea volume = _spawnArea[Random.Range(0, _spawnArea.Length)];
                Vector3 randomPoint = volume.GetRandomPosition();

                if (!Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, _rayDistance, _floorLayer))
                    continue;

                Vector3 spawnPos = hit.point;

                if (Physics.CheckBox(spawnPos, _checkHalfExtents, Quaternion.identity, _obstacleLayer))
                    continue;

                Instantiate(_tempPrefab, spawnPos, Quaternion.identity);
                _spawnedCount++;
                return;
            }
        }
    }
}

