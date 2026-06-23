using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class JewelPuzzleUIManager : MonoBehaviour
{
    public static JewelPuzzleUIManager Instance { get; private set; }

    [Header("UI 요소 연결")]
    [SerializeField] private TextMeshProUGUI _totalPriceText; // 총 가치 금액
    [SerializeField] private Button _confirmButton; // 확인 버튼
    [SerializeField] private GameObject _operationKey; // 조작 설명

    [Header("공간 및 스폰 설정")]
    [SerializeField] private Transform _pickupSpace; // 줍기 공간의 중심 트랜스폼
    [SerializeField] private LayerMask _gemLayerMask; // 보석 오브젝트들만 감지하기 위한 레이어 설정

    [Header("조종(크레인) 설정")]
    [SerializeField] private Transform _dropZoneCenter; // 보석 생성 지점
    [SerializeField] private float _moveSpeed = 5f; // 좌우 이동 속도
    [SerializeField] private float _limitX = 3f; // 움직일 수 있는 최대 거리

    [Header("외부 컴포넌트 연결")]
    [SerializeField] private BagOverloadDetector _overflowHandler; // 유효성 검사용
    [SerializeField] private Camera _puzzleCamera;// 카메라

    private PlayerInventory _playerInventory;
    // 현재 가방 안에 들어가 연산에 포함된 보석 리스트
    private List<ItemBase> _jewelsInBag = new List<ItemBase>();
    private ItemBase _activeGem;

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

        if (_puzzleCamera == null)
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
            Debug.LogError("씬에서 PlayerInventory를 찾을 수 없습니다! 플레이어 오브젝트가 씬에 있는지 확인하세요.");
        }
    }

    private void Start()
    {
        UpdateTotalPriceDisplay();

        if (_operationKey != null) _operationKey.SetActive(false);
        if (_confirmButton != null) _confirmButton.onClick.AddListener(ConfirmBagContents);
    }

    private void Update()
    {
        if (_activeGem == null)
        {
            HandleMouseClick();
        }
        else
        {
            HandleActiveGemMovement();
        }
    }

    // 마우스 클릭을 감지하고 보석 오프젝트를 판별
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

    // 보석의 위치를 가방 <-> 줍기 공간으로 서로 변경
    private void ToggleJewelLocation(ItemBase gem)
    {
        if (_jewelsInBag.Contains(gem)) MoveToPickupSpace(gem);
        else StartMovingGem(gem);
    }

    // 보석 조종 위치로 이동
    private void StartMovingGem(ItemBase gem)
    {
        var physics = gem.GetComponent<JewelPhysicsApplier>();
        if (physics == null) physics = gem.gameObject.AddComponent<JewelPhysicsApplier>();
        physics.EnterPuzzleMode();

        _activeGem = gem;
        gem.transform.position = _dropZoneCenter.position;
        if (_operationKey != null) _operationKey.SetActive(true);
    }

    // 좌우 이동 및 낙하
    private void HandleActiveGemMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");

        if (h != 0)
        {
            Vector3 pos = _activeGem.transform.position;
            pos.x = Mathf.Clamp(pos.x + h * _moveSpeed * Time.deltaTime, _dropZoneCenter.position.x - _limitX, _dropZoneCenter.position.x + _limitX);
            _activeGem.transform.position = pos;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            DropActiveGem();
        }
    }

    // 보석 낙하 처리
    private void DropActiveGem()
    {
        _activeGem.GetComponent<Rigidbody>().useGravity = true;
        _jewelsInBag.Add(_activeGem);
        _activeGem = null;
        if (_operationKey != null) _operationKey.SetActive(false);
        UpdateTotalPriceDisplay();
    }

    // 보석의 위치를 줍기공간에서 가방으로
    private void MoveToPickupSpace(ItemBase gem)
    {
        _jewelsInBag.Remove(gem);

        Vector3 offset = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
        gem.transform.position = _pickupSpace.position + offset;

        var physics = gem.GetComponent<JewelPhysicsApplier>();

        if (physics != null)
        {
            physics.ExitPuzzleMode();
        }

        gem.gameObject.layer = LayerMask.NameToLayer("Default");

        Rigidbody rb = gem.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        UpdateTotalPriceDisplay();
    }

    // 보석이 선을 넘었을때
    public void RemoveJewelFromBag(ItemBase gem)
    {
        if (_jewelsInBag.Contains(gem)) _jewelsInBag.Remove(gem);
        UpdateTotalPriceDisplay();
    }

    // 가방 상황 확정
    public void ConfirmBagContents()
    {
        if (_activeGem != null) return;

        List<ItemBase> remainingJewels = new List<ItemBase>();

        foreach (var gem in _jewelsInBag)
        {
            gem.GetComponent<JewelPhysicsApplier>()?.ExitPuzzleMode();

            ItemData itemData = GameManager.DataTable.GetItemDataTable().TryGetValue(gem.Id, out var data) ? data : null;

            if (itemData != null && _playerInventory.TryAcquireItem(itemData, HoldType.Pocket, out _, out _))
            {
                Destroy(gem.gameObject);
            }
            else
            {
                remainingJewels.Add(gem);
            }
        }
        _jewelsInBag.Clear();

        foreach (var gem in remainingJewels)
        {
            _jewelsInBag.Add(gem);
            MoveToPickupSpace(gem);
        }

        UpdateTotalPriceDisplay();
        ClosePuzzleUI();
    }

    public void ClosePuzzleUI()
    {
        ItemBase[] allGems = FindObjectsByType<ItemBase>(FindObjectsSortMode.None);
        foreach (var gem in allGems)
        {
            // 가방 리스트에 없는 보석(즉, 줍기 공간에 방치된 애들)만 삭제
            if (!_jewelsInBag.Contains(gem))
            {
                Destroy(gem.gameObject);
            }
        }

        gameObject.SetActive(false);
    }

    // 가방안 보석 가격 총 계산 UI 갱신
    private void UpdateTotalPriceDisplay()
    {
        if (_totalPriceText == null) return;
        _totalPriceText.text = $"총 가치: {CalcTotalValue():N0} Gold";
    }

    // 계산액 반환
    private int CalcTotalValue()
    {
        int total = 0;
        foreach (ItemBase gem in _jewelsInBag) total += gem.Price;
        return total;
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
}
