using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("스폰 위치")]
    [SerializeField] private Transform _spawnPoint;

    private GameObject _spawnedPlayer;

    public GameObject TrySpawnPlayer(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        if (_spawnedPlayer != null)
        {
            _spawnedPlayer.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            return _spawnedPlayer;
        }

        return SpawnPlayer(spawnPosition, spawnRotation);
    }

    private GameObject SpawnPlayer(Vector3 position, Quaternion rotation)
    {
        GameObject playerPrefab = GameManager.Resource.GetLoadedAsset<GameObject>("Player");

        if (playerPrefab == null)
        {
            Debug.LogError("플레이어 프리팹을 로드 실패.");
            return null;
        }

        _spawnedPlayer = Instantiate(playerPrefab, position, rotation);
        return _spawnedPlayer;
    }
}