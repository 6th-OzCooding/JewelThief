using UnityEngine;

public class LobbyController : MonoBehaviour
{
    [Header("로비 구성 요소 연결")]
    [SerializeField] private Transform _lobbySpawnPoint;
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private StageSelectController _stageSelectController;

    [Header("상점 진열 설정")]
    [SerializeField] private ShopItemDisplay _shopItemDisplayPrefab;
    [SerializeField] private Transform[] _shopDisplayAnchors;

    public void Enter()
    {
        if (_playerSpawner == null)
        {
            Debug.LogError("PlayerSpawner가 연결되지 않았습니다.");
            return null;
        }

        GameObject spawnedPlayer = _playerSpawner.TrySpawnPlayer(_lobbySpawnPoint.position, _lobbySpawnPoint.rotation);

        if (spawnedPlayer == null || _stageSelectController == null)
        {
            return;
        }

        PlayerInputHandler inputHandler = spawnedPlayer.GetComponentInChildren<PlayerInputHandler>();
        if (inputHandler != null)
        {
            _stageSelectController.SetPlayerInputHandler(inputHandler);
        }

        PlayerController playerController = spawnedPlayer.GetComponentInChildren<PlayerController>();
        if (playerController != null)
        {
            _stageSelectController.SetPlayerCameraTransform(playerController.CameraTransform);
            return playerController;
        }
        else
        {
            Debug.LogError("PlayerController를 찾지 못했습니다.");
            return null;
        }

        DisplayShopTools();
    }

    private void DisplayShopTools()
    {
        if (_shopItemDisplayPrefab == null || _shopDisplayAnchors == null)
        {
            Debug.LogError("상점 진열 프리팹 또는 앵커가 연결되지 않았습니다.");
            return;
        }

        var itemDataTable = GameManager.DataTable.GetItemDataTable();
        if (itemDataTable == null)
        {
            Debug.LogError("ItemDataTable이 null입니다.");
            return;
        }

        int anchorIndex = 0;
        foreach (ItemData itemData in itemDataTable.Values)
        {
            if (anchorIndex >= _shopDisplayAnchors.Length)
                break;

            if (itemData == null || itemData.GetItemType() != ItemType.Tool)
                continue;

            Transform anchor = _shopDisplayAnchors[anchorIndex];
            if (anchor == null)
            {
                anchorIndex++;
                continue;
            }

            ShopItemDisplay shopItemDisplay = Instantiate(_shopItemDisplayPrefab, anchor.position, anchor.rotation, anchor);
            shopItemDisplay.InitFromSpawner(itemData.Id);
            anchorIndex++;
        }
    }

    public void Exit()
    {
        // TODO (김경훈 - 26.06.22): 추후 본부에서 나갈 때 필요한 정리 로직

    }
}
