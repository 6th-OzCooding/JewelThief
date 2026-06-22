using UnityEngine;

public class LobbyController : MonoBehaviour
{
    [Header("로비 구성 요소 연결")]
    [SerializeField] private PlayerSpawner _playerSpawner;

    public void Enter()
    {
        if (_playerSpawner == null)
        {
            Debug.LogError("PlayerSpawner가 연결되지 않았습니다.");
            return;
        }

        _playerSpawner.TrySpawnPlayer();

        // TODO (김경훈 - 26.06.22): 추후 상점/스폰 위치 등 본부 진입 시 필요한 추가 초기화 로직
    }

    public void Exit()
    {
        // TODO (김경훈 - 26.06.22): 추후 본부에서 나갈 때 필요한 정리 로직

    }
}