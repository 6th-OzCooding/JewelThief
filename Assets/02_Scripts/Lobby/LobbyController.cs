using UnityEngine;

public class LobbyController : MonoBehaviour
{
    [Header("로비 구성 요소 연결")]
    [SerializeField] private Transform _lobbySpawnPoint;
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private StageSelectController _stageSelectController;

    public void Enter()
    {
        if (_playerSpawner == null)
        {
            Debug.LogError("PlayerSpawner가 연결되지 않았습니다.");
            return;
        }

        GameObject spawnedPlayer = _playerSpawner.TrySpawnPlayer(_lobbySpawnPoint.position, _lobbySpawnPoint.rotation);

        if (spawnedPlayer == null || _stageSelectController == null)
            return;

        PlayerInputHandler inputHandler = spawnedPlayer.GetComponentInChildren<PlayerInputHandler>();
        if (inputHandler != null)
        {
            _stageSelectController.SetPlayerInputHandler(inputHandler);
        }

        PlayerController playerController = spawnedPlayer.GetComponentInChildren<PlayerController>();
        if (playerController != null)
        {
            _stageSelectController.SetPlayerCameraTransform(playerController.CameraTransform);
        }
        else
        {
            Debug.LogError("PlayerController를 찾지 못했습니다.");
        }
    }

    public void Exit()
    {
        // TODO (김경훈 - 26.06.22): 추후 본부에서 나갈 때 필요한 정리 로직

    }
}