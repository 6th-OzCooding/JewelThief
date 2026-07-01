using Cysharp.Threading.Tasks;
using System;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : SingletonBehaviour<GameManager>
{
    public static ResourceManager Resource { get { return Instance._resourceManager; } }
    public static SoundManager Sound { get { return Instance._soundManager; } }
    public static PoolManager Pool { get { return Instance._poolManager; } }
    public static AlertManager Alert { get { return Instance._alertManager; } }
    public static DataTable DataTable { get { return Instance._dataTable; } }
    public static UIManager UI { get { return Instance._uiManager; } }
    public static UserDataManager UserData { get { return Instance._userDataManager; } }
    public static ShopManager Shop { get { return Instance._shopManager; } }

    #region Manager Varialbes

    private ResourceManager _resourceManager = new();
    private SoundManager _soundManager = new();
    private PoolManager _poolManager = new();
    private AlertManager _alertManager = new();
    private DataTable _dataTable = new();
    private UIManager _uiManager = new();
    private UserDataManager _userDataManager = new();
    private ShopManager _shopManager = new();

    private WFCMapGeneration _wfcMapGeneration;
    private LobbyController _lobbyController;
    private GameObject _bagInventoryViewInstance;
    private BagInventoryViewController _bagInventoryViewController;

    #endregion

    #region Variables

    [Header("Test Options")]
    [SerializeField] private bool _skipStartupUIForTest;
    [Header("InGame Spawn")]
    [SerializeField] private float _inGameSpawnHeightOffset = 1f;
    [Header("Bag Inventory View")]
    [SerializeField] private string _bagInventoryViewPrefabAddress = "BagInventoryView";
    [SerializeField] private Vector3 _bagInventoryViewSpawnPosition = new Vector3(10000f, 10000f, 10000f);

    private bool _isInGame = false;
    private bool _isPaused = false;
    private int _stageStartBagPrice = 0;
    private readonly System.Collections.Generic.HashSet<InventoryItem> _stageStartCollectibleItems = new();

    public bool IsEnteringInGame { get; private set; } = false;

    private PlayerController _playerController;

    private Transform _mapRoot = null;
    private Transform _poolRoot = null;

    private GameObject _lobbyPrefab;
    private GameObject _lobbyInstance;

    private NavMeshSurface _navMeshSurface = null;

    private string[] _removeToolIdsWhenInGameExit = { "Item_Tool_MasterKey", };

    #endregion

    #region Events

    public event Action<string[]> OnExitInGame;
    public event Action OnPlayerCaught;

    #endregion

    #region Getters

    /// <summary>
    /// 현재 플레이어가 실제 스테이지 플레이 상태에 있는지 반환합니다.
    /// </summary>
    public bool IsInGame => _isInGame;

    /// <summary>
    /// 현재 게임플레이가 일시정지 상태인지 반환합니다.
    /// </summary>
    public bool IsPaused => _isPaused;

    #endregion

    #region Gold_Info

    // 전역 데이터 추가
    public int Gold { get; private set; }
    public string SelectedStageId;

    // 골드 증가 (판매소 등)
    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        Gold += amount;
    }

    // 골드 차감 시도 (상점 등)
    public bool TrySpendGold(int amount)
    {
        if (amount <= 0 || Gold < amount)
            return false;

        Gold -= amount;
        return true;
    }

    #endregion

    /// <summary>
    /// 데이터 드리븐 초기화 -> UIManager 초기화 -> 로딩(어드레서블 불러오기) -> 사운드 및 풀 초기화
    /// </summary>
    protected override void Init()
    {
        base.Init();

        _dataTable.LoadAllData();

        _userDataManager.Init();
        _userDataManager.LoadUserData();

        _uiManager.Init();
        InitAsync().Forget();

        AddGold(500);
    }

    private async UniTaskVoid InitAsync()
    {
        if (_skipStartupUIForTest)
        {
            await Resource.Init();
            InitNonAsync();

            _playerController = EnterLobby(true);
            UI.ShowStartupUIOnGameStart(_playerController);
            return;
        }

        UIBase loadingUIBase = UI.OpenLoadingUI();


        if (loadingUIBase == null)
        {
            throw new Exception("Failed to open loading UI");
        }
        else if (!loadingUIBase.TryGetComponent(out LoadingUI loadingUI))
        {
            throw new Exception("Failed to get LoadingUI component from loading UI");
        }
        else
        {
            await loadingUI.StartLoading();
        }

        InitNonAsync();

        UI.OpenUI(UIRootType.ContentUI, UIType.TitleUI);
    }

    private void InitNonAsync()
    {
        _soundManager.Init(this.gameObject);
        _wfcMapGeneration = new();
        PoolInit();
    }

    private void Update()
    {
        if(_isInGame && !_isPaused)
        {
            _alertManager.OnUpdate();
        }
    }

    #region GameFlow

    public PlayerController EnterLobby(bool isFirstEnter = false)
    {
        if(isFirstEnter)
            UI.CloseUI(UIType.TitleUI);

        if(isFirstEnter)
        {
            _lobbyPrefab = _resourceManager.GetLoadedAsset<GameObject>("Lobby");
            if (_lobbyPrefab == null)
            {
                Debug.LogError("Lobby 프리팹을 로드하지 못했습니다.");
                return null;
            }

            _lobbyInstance = Instantiate(_lobbyPrefab);
            _lobbyInstance.SetActive(true);

            GameObject bagInventoryViewPrefab = _resourceManager.GetLoadedAsset<GameObject>(_bagInventoryViewPrefabAddress);
            if (bagInventoryViewPrefab != null)
            {
                _bagInventoryViewInstance = Instantiate(bagInventoryViewPrefab, _bagInventoryViewSpawnPosition, Quaternion.identity);
                _bagInventoryViewController = _bagInventoryViewInstance.GetComponentInChildren<BagInventoryViewController>(true);

                if (_bagInventoryViewController == null)
                {
                    Debug.LogError("BagInventoryViewController가 가방 뷰 프리팹에 연결되지 않았습니다.");
                }
            }
            else
            {
                Debug.LogError("가방 뷰 프리팹을 찾을 수 없습니다.");
            }

            if (_lobbyInstance.TryGetComponent(out _lobbyController))
            {
                _playerController = _lobbyController.Enter();
                BindBagInventoryView(_playerController);
                return _playerController;
            }
            else
                Debug.LogError("Lobby 프리팹에 LobbyController 컴포넌트가 없습니다.");
        }
        else
        {
            if (_lobbyInstance == null || _lobbyController == null)
            {
                Debug.LogError("Lobby 인스턴스가 생성되지 않았습니다.");
                return null;
            }

            _lobbyInstance.SetActive(true);
            _playerController = _lobbyController.Enter();
            BindBagInventoryView(_playerController);
            return _playerController;
        }

        return null;
    }

    public void EnterInGame(string StageId)
    {
        EnterInGameAsync(StageId).Forget();
        _playerController.BlockStaminaConsume(false);
    }

    private async UniTaskVoid EnterInGameAsync(string stageId)
    {
        IsEnteringInGame = true;
        UI.OpenInGameLoadingUI();

        if (_lobbyInstance != null)
            _lobbyInstance.SetActive(false);

        await GenerateMap();

        RespawnPlayerToStartTile();

        StageData stageData = _dataTable.GetStageData(stageId);
        if (stageData != null)
        {
            _soundManager.PlayBGM(SoundId.BGM_PlayTheme);
            _alertManager.Init(stageData.TimeLimit);
        }

        InitStageStartBagPrice();

        _isInGame = true;
        _isPaused = false;

        await UniTask.Delay(3000);  // 플레이어 이동이 노출되는 것을 방지

        UI.CloseInGameLoadingUI();
        IsEnteringInGame = false;
    }

    private void RespawnPlayerToStartTile()
    {
        if (_playerController == null)
        {
            Debug.LogError("PlayerController가 없어 시작 좌표 재스폰을 건너뜁니다.");
            return;
        }

        Vector3 startPosition = _wfcMapGeneration.GetStartTileWorldPosition();
        _playerController.Teleport(startPosition + Vector3.up * _inGameSpawnHeightOffset);
    }

    public void ReturnToLobby()
    {
        _isInGame = false;
        _isPaused = false;
        _alertManager?.ResumeTimer();                           
        _playerController?.SetInputMode(PlayerInputMode.Gameplay);

        Pool.AllDespawnToPool();
        _wfcMapGeneration.Release();

        OnExitInGame?.Invoke(_removeToolIdsWhenInGameExit);

        EnterLobby();
        RespawnPlayerToLobby();
    }

    private void RespawnPlayerToLobby()
    {
        if (_playerController == null || _lobbyController == null)
            return;

        _playerController.Teleport(_lobbyController.SpawnPosition);
    }

    private void BindBagInventoryView(PlayerController playerController)
    {
        if (_bagInventoryViewController == null || playerController == null)
            return;

        PlayerInventory playerInventory = playerController.GetComponent<PlayerInventory>();
        PlayerInputHandler playerInput = playerController.GetComponent<PlayerInputHandler>();

        if (playerInventory == null || playerInput == null)
        {
            Debug.LogError("BagInventoryViewController에 연결할 PlayerInventory 또는 PlayerInputHandler를 찾지 못했습니다.");
            return;
        }

        _bagInventoryViewController.BindPlayer(playerInventory, playerInput);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    #endregion

    private async UniTask GenerateMap()
    {
        if (null == _mapRoot)
        {
            _mapRoot = Utils.CreateEmptyGameObject("MapRoot", this.gameObject.transform).transform;
            _navMeshSurface = Utils.GetOrAddComponent<NavMeshSurface>(_mapRoot.gameObject);
            _navMeshSurface.collectObjects = CollectObjects.Children;
            _navMeshSurface.layerMask = LayerMask.GetMask("Floor");
        }

        await _wfcMapGeneration.StartGenerateMap(_navMeshSurface, _mapRoot);
    }

    public void PauseGame()
    {
        _isPaused = true;
    }

    public void ResumeGame()
    {
        _isPaused = false;
    }

    public void GameOver()
    {
        HandleStageEnd(isCaught: true);
    }

    public void Escape()
    {
        HandleStageEnd(isCaught: false);
    }

    private void HandleStageEnd(bool isCaught)
    {
        _isPaused = true;
        Sound.StopBGM();
        _alertManager?.PauseTimer();
        _playerController?.SetInputMode(PlayerInputMode.UIOnly);

        int totalValue = GetCurrentStageBagScore();
        string bestGemName = GetMostExpensiveStageBagItemName();

        float remainingTime = _alertManager != null ? _alertManager.GetRemainingTime() : 0f;

        if (isCaught)
        {
            OnPlayerCaught?.Invoke();
            _playerController.ResetPlayerStat();
        }
        else
        {
        }

        ScorePopupUI scorePopupUI = UI.OpenScorePopupUI();
        if (scorePopupUI != null)
            scorePopupUI.DisplayScore(totalValue, bestGemName, remainingTime, isCaught: isCaught);
        else
            Debug.LogError("ScorePopupUI를 열지 못했습니다.");
    }

    private void InitStageStartBagPrice()
    {
        PlayerInventory playerInventory = GetPlayerInventory();
        if (playerInventory == null)
        {
            _stageStartBagPrice = 0;
            _stageStartCollectibleItems.Clear();
            return;
        }

        _stageStartBagPrice = GetTotalCarriedCollectiblePrice(playerInventory);
        CacheStageStartCollectibleItems(playerInventory);
    }

    private int GetCurrentStageBagScore()
    {
        PlayerInventory playerInventory = GetPlayerInventory();
        if (playerInventory == null)
            return 0;

        int earnedScore = GetTotalCarriedCollectiblePrice(playerInventory) - _stageStartBagPrice;
        return Mathf.Max(0, earnedScore);
    }

    private int GetTotalCarriedCollectiblePrice(PlayerInventory playerInventory)
    {
        if (playerInventory == null)
            return 0;

        int totalPrice = 0;
        System.Collections.Generic.IReadOnlyList<InventoryItem> bagItems = playerInventory.BagItems;

        for (int i = 0; i < bagItems.Count; i++)
        {
            if (!IsScoreCollectibleItem(bagItems[i]))
                continue;

            totalPrice += bagItems[i].ItemData.Price;
        }

        totalPrice += GetScoreCollectiblePrice(playerInventory.LeftHandItem);
        totalPrice += GetScoreCollectiblePrice(playerInventory.RightHandItem);
        return totalPrice;
    }

    private string GetMostExpensiveStageBagItemName()
    {
        PlayerInventory playerInventory = GetPlayerInventory();
        if (playerInventory == null)
            return "없음";

        System.Collections.Generic.IReadOnlyList<InventoryItem> bagItems = playerInventory.BagItems;
        if (GetCurrentStageBagScore() <= 0)
            return "없음";

        InventoryItem bestItem = null;
        int maxPrice = -1;

        for (int i = 0; i < bagItems.Count; i++)
        {
            UpdateBestNewScoreItem(bagItems[i], ref bestItem, ref maxPrice);
        }

        UpdateBestNewScoreItem(playerInventory.LeftHandItem, ref bestItem, ref maxPrice);
        UpdateBestNewScoreItem(playerInventory.RightHandItem, ref bestItem, ref maxPrice);

        return bestItem?.ItemData?.Name ?? "없음";
    }

    private void CacheStageStartCollectibleItems(PlayerInventory playerInventory)
    {
        _stageStartCollectibleItems.Clear();

        System.Collections.Generic.IReadOnlyList<InventoryItem> bagItems = playerInventory.BagItems;

        for (int i = 0; i < bagItems.Count; i++)
        {
            if (IsScoreCollectibleItem(bagItems[i]))
                _stageStartCollectibleItems.Add(bagItems[i]);
        }

        if (IsScoreCollectibleItem(playerInventory.LeftHandItem))
            _stageStartCollectibleItems.Add(playerInventory.LeftHandItem);

        if (IsScoreCollectibleItem(playerInventory.RightHandItem))
            _stageStartCollectibleItems.Add(playerInventory.RightHandItem);
    }

    private int GetScoreCollectiblePrice(InventoryItem inventoryItem)
    {
        if (!IsScoreCollectibleItem(inventoryItem))
            return 0;

        return inventoryItem.ItemData.Price;
    }

    private void UpdateBestNewScoreItem(InventoryItem inventoryItem, ref InventoryItem bestItem, ref int maxPrice)
    {
        if (!IsScoreCollectibleItem(inventoryItem) || _stageStartCollectibleItems.Contains(inventoryItem) || inventoryItem.ItemData.Price <= maxPrice)
            return;

        maxPrice = inventoryItem.ItemData.Price;
        bestItem = inventoryItem;
    }

    private bool IsScoreCollectibleItem(InventoryItem inventoryItem)
    {
        return inventoryItem?.ItemData != null && inventoryItem.ItemData.GetItemType() != ItemType.Tool;
    }

    private PlayerInventory GetPlayerInventory()
    {
        if (_playerController == null)
            return null;

        return _playerController.GetComponent<PlayerInventory>();
    }

    private void PoolInit()
    {
        if (null == _poolRoot)
        {
            _poolRoot = Utils.CreateEmptyGameObject("PoolRoot", this.gameObject.transform).transform;
        }

        _poolManager.Init(_poolRoot);
    }
}
