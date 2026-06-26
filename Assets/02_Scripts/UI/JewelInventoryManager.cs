using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JewelInventoryManager : MonoBehaviour
{
    public static JewelInventoryManager Instance { get; private set; }

    [Header("시각적 관리")]
    [SerializeField] private GameObject _puzzleVisualRoot; // 시각적 대상

    [Header("설정")]
    [SerializeField] private float _maxWeight = 50f;

    [Header("공간 및 스폰 설정")]
    [SerializeField] private Transform _pickupSpace; // 줍기 공간의 중심 트랜스폼
    [SerializeField] private LayerMask _gemLayerMask; // 보석 오브젝트들만 감지하기 위한 레이어 설정

    [Header("낙하 설정")]
    [SerializeField] private Transform _dropZoneCenter; // 보석 생성 지점
    [SerializeField] private float _limitX = 3f; // 움직일 수 있는 최대 거리

    [Header("외부 컴포넌트 연결")]
    [SerializeField] private BagOverloadDetector _overflowHandler; // 유효성 검사용
    [SerializeField] private Camera _puzzleCamera; // 카메라

    private Transform _playerTransform;
    private PlayerInputHandler _playerInput;

    // 주울때 Queue 저장
    private Queue<Jewel> _tempQueue = new Queue<Jewel>();
    // 가방 리스트
    private List<Jewel> _jewelsInBag = new List<Jewel>();
    // 임시 보관함 리스트
    private List<Jewel> _pickupAreaGems = new List<Jewel>();

    private bool _isAutoDropping = false; // 낙하 상태 확인

    private int _stageStartBagPrice = 0;
    private int _stageStartJewelCount = 0;

    public bool IsPuzzleActive
    {
        get
        {
            if (_puzzleVisualRoot != null) return _puzzleVisualRoot.activeSelf;
            return false;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _playerInput = FindAnyObjectByType<PlayerInputHandler>();

        if (_puzzleVisualRoot != null)
        {
            _puzzleVisualRoot.SetActive(false);
        }

        if (_puzzleCamera != null)
        {
            _puzzleCamera.depth = 10;
            _puzzleCamera.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("퍼즐 전용 카메라(_puzzleCamera)가 연결되지 않았습니다!");
        }
    }

    private void OnDestroy()
    {
        if (_playerInput != null)
        {
            _playerInput.OnJewelryInventoryToggleEvent -= TogglePuzzleInventory;
        }
    }

    private void Update()
    {
        if (IsPuzzleActive && !_isAutoDropping)
        {
            HandleMouseClick();
        }
    }

    public void InitializePlayer(Transform player, PlayerInputHandler input)
    {
        _playerTransform = player;

        if (_playerInput != null)
        {
            _playerInput.OnJewelryInventoryToggleEvent -= TogglePuzzleInventory;
        }

        _playerInput = input;

        if (_playerInput != null)
        {
            _playerInput.OnJewelryInventoryToggleEvent += TogglePuzzleInventory;
        }
    }

    private void TogglePuzzleInventory()
    {
        if (_puzzleVisualRoot == null) return;

        if (IsPuzzleActive)
        {
            ClosePuzzleInventory();
        }
        else
        {
            OpenPuzzleInventory();
        }
    }

    public void InitStageStartPrice()
    {
        _stageStartBagPrice = GetTotalBagPrice();
        _stageStartJewelCount = _jewelsInBag.Count;
    }

    public int GetCurrentStageScore()
    {
        int currentTotal = GetTotalBagPrice();
        int earnedScore = currentTotal - _stageStartBagPrice;

        if (earnedScore < 0)
        {
            earnedScore = 0;
        }

        return earnedScore;
    }


    // 줍기 제한
    public bool CanPickupJewel(ItemData itemData)
    {
        // 공간 확인
        if (_overflowHandler != null && !_overflowHandler.IsSpaceSafe)
        {
            Debug.Log("가방이 가득 차서 넘쳤습니다! 더 이상 보석을 주울 수 없습니다.");
            return false;
        }

        // 무게 확인
        float currentTotalWeight = GetTotalJewelWeight();
        if (currentTotalWeight + itemData.Weight > _maxWeight)
        {
            Debug.Log($"무게 제한 초과! 현재 무게: {currentTotalWeight}/{_maxWeight}");
            return false;
        }

        return true;
    }

    // Queue 형태 저장
    public void AddJewelToTempQueue(Jewel gem)
    {
        if (gem == null) return;

        _tempQueue.Enqueue(gem);
        gem.gameObject.SetActive(false);
    }

    // 인벤토리 열림
    public void OpenPuzzleInventory()
    {
        if (_puzzleVisualRoot != null) _puzzleVisualRoot.SetActive(true);
        if (_puzzleCamera != null) _puzzleCamera.gameObject.SetActive(true);

        if (GameManager.UI != null)
        {
            GameManager.UI.OpenUI(UIRootType.ContentUI, UIType.JewelInventoryUI);
        }

        if (GameManager.UI != null)
        {
            GameManager.UI.CloseUI(UIType.MainHUD);
        }

        if (_playerInput != null)
        {
            _playerInput.SetMode(PlayerInputMode.UIOnly);
        }

        if (GameManager.Instance != null)
        {
            // GameManager.Instance.PauseGameForPuzzle();
        }

        if (_tempQueue.Count > 0)
        {
            ProcessAutoDropAsync().Forget();
        }
    }

    // 자동 낙하
    private async UniTaskVoid ProcessAutoDropAsync()
    {
        _isAutoDropping = true;

        while (_tempQueue.Count > 0)
        {
            Jewel gem = _tempQueue.Dequeue();
            gem.gameObject.SetActive(true);

            DropGemAutomatically(gem);

            await UniTask.Delay(System.TimeSpan.FromSeconds(0.3f));
        }

        _isAutoDropping = false;
    }

    // 인벤토리 닫힘
    public void ClosePuzzleInventory()
    {
        if (_isAutoDropping)
        {
            Debug.LogWarning("보석들이 낙하 중입니다! 잠시만 기다려주세요.");
            return;
        }

        if (GameManager.UI != null)
        {
            GameManager.UI.CloseUI(UIType.JewelInventoryUI);
            GameManager.UI.OpenUI(UIRootType.MainUI, UIType.MainHUD);
        }

        if (_playerTransform == null)
        {
            Debug.LogError("Player Transform을 찾을 수 없습니다.");
            return;
        }

        Vector3 dropPosition = _playerTransform.position;

        foreach (var gem in _pickupAreaGems)
        {
            if (gem == null)
                continue;

            gem.gameObject.SetActive(true);

            gem.transform.position = dropPosition + new Vector3(Random.Range(-0.5f, 0.5f), 1f, Random.Range(-0.5f, 0.5f));

            var physics = gem.GetComponent<JewelPhysicsApplier>();

            if (physics != null)
            {
                physics.ExitPuzzleMode();
            }

            gem.gameObject.layer = LayerMask.NameToLayer("Default");

            Rigidbody rb = gem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
            }
        }

        _pickupAreaGems.Clear();

        if (_puzzleVisualRoot != null) _puzzleVisualRoot.SetActive(false);
        if (_puzzleCamera != null) _puzzleCamera.gameObject.SetActive(false);

        if (_playerInput != null)
        {
            _playerInput.SetMode(PlayerInputMode.Gameplay);
        }

        if (GameManager.Instance != null)
        {
            // GameManager.Instance.ResumeGameFromPuzzle();
        }
    }

    // 마우스 클릭
    private void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _puzzleCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _gemLayerMask))
            {
                Jewel gem = hit.collider.GetComponent<Jewel>();
                if (gem != null) ToggleJewelLocation(gem);
            }
        }
    }

    // 가방 <-> 임시 보관 스위칭
    private void ToggleJewelLocation(Jewel gem)
    {
        if (_jewelsInBag.Contains(gem))
        {
            MoveToPickupSpace(gem);
        }
        else if (_pickupAreaGems.Contains(gem))
        {
            _pickupAreaGems.Remove(gem);
            DropGemAutomatically(gem);
        }
    }

    // 실제 보석 랜덤 낙하
    private void DropGemAutomatically(Jewel gem)
    {
        var physics = gem.GetComponent<JewelPhysicsApplier>();

        if (physics == null) physics = gem.gameObject.AddComponent<JewelPhysicsApplier>();

        physics.EnterPuzzleMode();

        float randomX = Random.Range(-_limitX, _limitX);

        gem.transform.position = _dropZoneCenter.position + new Vector3(randomX, 0, 0);

        Rigidbody rb = gem.GetComponent<Rigidbody>();

        if (rb != null)
        { 
            rb.useGravity = true; 
        }

        if (!_jewelsInBag.Contains(gem))
        { 
            _jewelsInBag.Add(gem);
        }
    }

    // 가방 -> 임시 보관함
    private void MoveToPickupSpace(Jewel gem)
    {
        _jewelsInBag.Remove(gem);

        if (!_pickupAreaGems.Contains(gem)) _pickupAreaGems.Add(gem);

        Vector3 offset = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
        gem.transform.position = _pickupSpace.position + offset;

        var physics = gem.GetComponent<JewelPhysicsApplier>();
        if (physics != null) physics.ExitPuzzleMode();

        gem.gameObject.layer = LayerMask.NameToLayer("Default");

        Rigidbody rb = gem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }


    // 선 넘은 보석 -> 임시로 이동
    public void RemoveJewelFromBag(Jewel gem)
    {
        MoveToPickupSpace(gem);
    }

    // 무게 계산
    public float GetTotalJewelWeight()
    {
        float totalWeight = 0f;

        foreach (var gem in _tempQueue)
        {
            totalWeight += gem.Weight;
        }
        foreach (var gem in _jewelsInBag)
        {
            totalWeight += gem.Weight;
        }
        foreach (var gem in _pickupAreaGems)
        {
            totalWeight += gem.Weight;
        }

        return totalWeight;
    }

    // 가방안 보석 가격 계산
    public int GetTotalBagPrice()
    {
        int totalPrice = 0;

        foreach (var gem in _jewelsInBag)
        {
            totalPrice += gem.Price;
        }

        return totalPrice;
    }

    // 가방안 가장 비싼 보석 이름 찾기
    public string GetMostExpensiveJewelName()
    {
        if (_jewelsInBag.Count <= _stageStartJewelCount) return "없음";

        Jewel bestGem = null;
        int maxPrice = -1;

        for (int i = _stageStartJewelCount; i < _jewelsInBag.Count; i++)
        {
            Jewel gem = _jewelsInBag[i];
            if (gem.Price > maxPrice)
            {
                maxPrice = gem.Price;
                bestGem = gem;
            }
        }

        if (bestGem == null) return "없음";

        if (bestGem.Data != null) return bestGem.Data.Name;

        return "알 수 없음";
    }

    private void OnDrawGizmos()
    {
        if (_dropZoneCenter != null)
        {
            Gizmos.color = Color.cyan;

            Vector3 center = _dropZoneCenter.position;
            Vector3 leftLimit = center + Vector3.left * _limitX;
            Vector3 rightLimit = center + Vector3.right * _limitX;

            Gizmos.DrawLine(leftLimit, rightLimit);

            Gizmos.DrawLine(leftLimit + Vector3.up * 0.5f, leftLimit + Vector3.down * 0.5f);
            Gizmos.DrawLine(rightLimit + Vector3.up * 0.5f, rightLimit + Vector3.down * 0.5f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(center, 0.1f);
        }
    }

    public int GetStageScoreAndFinalize()
    {
        _tempQueue.Clear();
        _pickupAreaGems.Clear();

        int stageScore = GetCurrentStageScore();

        Debug.Log("스테이지 정산 완료! 이번 판 점수: " + stageScore);

        return stageScore;
    }

    // 경찰에게 잡혀 보석 몰수
    public void ClearAllJewelsOnCaught()
    {
        _tempQueue.Clear();

        foreach (var gem in _jewelsInBag)
        {
            if (gem != null) Destroy(gem.gameObject);
        }
        foreach (var gem in _pickupAreaGems)
        {
            if (gem != null) Destroy(gem.gameObject);
        }

        _jewelsInBag.Clear();
        _pickupAreaGems.Clear();

        Debug.Log("경찰에게 잡혔습니다! 모든 보석이 몰수되었습니다.");
    }

    /*  // 경찰에게 잡혔을때 이번 판에서 얻은 보석만 압수되는 경우
    public void ClearAllJewelsOnCaught()
    {
        _tempQueue.Clear();

        foreach (var gem in _pickupAreaGems)
        {
            if (gem != null) Destroy(gem.gameObject);
        }
        _pickupAreaGems.Clear();

        for (int i = _stageStartJewelCount; i < _jewelsInBag.Count; i++)
        {
            Jewel gem = _jewelsInBag[i];
            if (gem != null) Destroy(gem.gameObject);
        }

        int newGemCount = _jewelsInBag.Count - _stageStartJewelCount;
        if (newGemCount > 0)
        {
            _jewelsInBag.RemoveRange(_stageStartJewelCount, newGemCount);
        }

        Debug.Log("경찰에게 잡혔습니다! 이번 스테이지에서 얻은 보석만 몰수되었습니다.");
    }  */
}
