using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("스폰 위치")]
    [SerializeField] private Transform _spawnPoint;

    private GameObject _spawnedPlayer;

    public void TrySpawnPlayer()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (_spawnPoint == null)
        {
            Debug.LogError("SpawnPoint가 연결되지 않았습니다.");
            return;
        }

        GameObject playerPrefab = GameManager.Resource.GetLoadedAsset<GameObject>("Player");
        if (playerPrefab == null)
        {
            Debug.LogError("플레이어 프리팹을 로드하지 못했습니다.");
            return;
        }

        _spawnedPlayer = Instantiate(playerPrefab, _spawnPoint.position, _spawnPoint.rotation);
    }
}