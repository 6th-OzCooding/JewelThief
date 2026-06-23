using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JewelPuzzleUIManager : MonoBehaviour
{
    public static JewelPuzzleUIManager Instance { get; private set; }

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
    [SerializeField] private Camera _puzzleCamera;// 카메라

    private PlayerInventory _playerInventory;
    // 주울때 Queue 저장
    private Queue<ItemBase> _tempQueue = new Queue<ItemBase>();
    // 가방 리스트
    private List<ItemBase> _jewelsInBag = new List<ItemBase>();
    // 임시 보관함 리스트
    private List<ItemBase> _pickupAreaGems = new List<ItemBase>();

    private bool _isAutoDropping = false; // 낙하 상태 확인

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

        if (_puzzleCamera != null)
        {
            _puzzleCamera.depth = 10;
        }
        else
        {
            Debug.LogError("퍼즐 전용 카메라(_puzzleCamera)가 연결되지 않았습니다!");
        }

        _playerInventory = FindAnyObjectByType<PlayerInventory>();

        if (_playerInventory == null)
        {
            Debug.LogError("씬에서 PlayerInventory를 찾을 수 없습니다! 플레이어가 씬에 있는지 확인하세요.");
        }

        var input = FindAnyObjectByType<PlayerInputHandler>();
        if (input != null)
        {
            input.OnInventoryToggleEvent += TogglePuzzleInventory;
        }
    }

    private void TogglePuzzleInventory()
    {
        if (gameObject.activeSelf)
        {
            ClosePuzzleInventory();
        }
        else
        {
            OpenPuzzleInventory();
        }
    }

    private void Update()
    {
        if (!_isAutoDropping)
        {
            HandleMouseClick();
        }
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
    public void AddJewelToTempQueue(ItemBase gem)
    {
        if (gem == null) return;

        _tempQueue.Enqueue(gem);
        gem.gameObject.SetActive(false);
    }

    // 인벤토리 열림
    public void OpenPuzzleInventory()
    {
        gameObject.SetActive(true);

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
            ItemBase gem = _tempQueue.Dequeue();
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

        if (_playerInventory == null) return;

        Vector3 dropPosition = _playerInventory.transform.position;

        foreach (var gem in _pickupAreaGems)
        {
            gem.gameObject.SetActive(true);
            gem.transform.position = dropPosition + new Vector3(Random.Range(-0.5f, 0.5f), 1f, Random.Range(-0.5f, 0.5f));

            var physics = gem.GetComponent<JewelPhysicsApplier>();
            if (physics != null) physics.ExitPuzzleMode();

            gem.gameObject.layer = LayerMask.NameToLayer("Default");

            Rigidbody rb = gem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
            }
        }

        _pickupAreaGems.Clear();
        gameObject.SetActive(false);
    }

    // 마우스 클릭
    private void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _puzzleCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _gemLayerMask))
            {
                ItemBase gem = hit.collider.GetComponent<ItemBase>();
                if (gem != null) ToggleJewelLocation(gem);
            }
        }
    }

    // 가방 <-> 임시 보관 스위칭
    private void ToggleJewelLocation(ItemBase gem)
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
    private void DropGemAutomatically(ItemBase gem)
    {
        var physics = gem.GetComponent<JewelPhysicsApplier>();
        if (physics == null) physics = gem.gameObject.AddComponent<JewelPhysicsApplier>();
        physics.EnterPuzzleMode();

        float randomX = Random.Range(-_limitX, _limitX);
        gem.transform.position = _dropZoneCenter.position + new Vector3(randomX, 0, 0);

        gem.GetComponent<Rigidbody>().useGravity = true;

        _jewelsInBag.Add(gem);
    }

    // 가방 -> 임시 보관함
    private void MoveToPickupSpace(ItemBase gem)
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
    public void RemoveJewelFromBag(ItemBase gem)
    {
        MoveToPickupSpace(gem);
    }

    // 무게 계산
    public float GetTotalJewelWeight()
    {
        float tempWeight = _tempQueue.Sum(g => g.Weight);
        float bagWeight = _jewelsInBag.Sum(g => g.Weight);
        float pickupWeight = _pickupAreaGems.Sum(g => g.Weight);
        return tempWeight + bagWeight + pickupWeight;
    }

    // 가방안 보석 가격 계산
    public int GetTotalBagPrice()
    {
        return _jewelsInBag.Sum(g => g.Price);
    }

    // 가방안 가장 비싼 보석 이름 찾기
    public string GetMostExpensiveJewelName()
    {
        var bestGem = _jewelsInBag.OrderByDescending(g => g.Price).FirstOrDefault();
        if (bestGem == null) return "없음";

        var data = GameManager.DataTable.GetItemData(bestGem.Id);
        return data != null ? data.Name : "알 수 없음";
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

    // 경찰에게 잡혀 강제 패배시 
    // 후추 or 후수
    public void ClearAllJewelsOnCaught()
    {
        _tempQueue.Clear();

        foreach (var gem in _jewelsInBag) if (gem != null) Destroy(gem.gameObject);
        foreach (var gem in _pickupAreaGems) if (gem != null) Destroy(gem.gameObject);

        _jewelsInBag.Clear();
        _pickupAreaGems.Clear();

        Debug.Log("경찰에게 잡혔습니다! 모든 보석이 몰수되었습니다.");
    }
}
