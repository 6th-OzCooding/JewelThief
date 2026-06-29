using UnityEngine;

public class WeightSensorTrap : BaseDisarmableObejct
{
    [Header("압력 센서 무게 설정")]
    [SerializeField] private float _activationRequiredWeight = 10f;    // 압력판이 작동하기 위해 필요한 최소 무게
    [SerializeField] private float _myTimeReductionAmount = 15f;     // 제한 시간 차감

    [Header("발판 시각 연출")]
    [SerializeField] private Transform _pressurePlateMesh;      // 밟았을 때 발판이 살짝 아래로 내려가도록 설정
    [SerializeField] private float _pressedYOffset = -0.05f;         // 밟혔을 때 내려갈 높이

    [Header("안경 투시 기믹 설정")]
    [SerializeField] private MeshRenderer _plateRenderer;       // 안경 미착용 시 발판을 투명하게 숨길 렌더러
    [SerializeField] private string _visibleLayer = "Interactable";
    [SerializeField] private string _invisibleLayer = "Default";

    private Vector3 _initialPlateLocalPosition;
    private bool _isTrackingGlasses = false;

    protected override void LoadData(string id)
    {
        _disarmObjName = "무게 감지 압력판";

        if (_timeReductionAmountList == null)
        {
            _timeReductionAmountList = new System.Collections.Generic.List<float>();
        }

        _timeReductionAmountList.Clear();
        _timeReductionAmountList.Add(_myTimeReductionAmount);
    }

    protected override void OnInitalized()
    {
        base.OnInitalized();

        if (_pressurePlateMesh != null)     // 발판의 초기 위치 기억
        {
            _initialPlateLocalPosition = _pressurePlateMesh.localPosition;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        _isTrackingGlasses = true;
        UpdateTrapVisibility(false);
    }

    private void Update()
    {
        if (!_isInitialized || _isDisarmed || !_isTrackingGlasses) return;

        bool isWearingGlasses = CheckIfPlayerWearingGlasses();
        UpdateTrapVisibility(isWearingGlasses);
    }

    private void UpdateTrapVisibility(bool isVisible)
    {
        if (_isDisarmed) return;

        if (_plateRenderer != null)
        {
            _plateRenderer.enabled = isVisible;     // 안경이 없으면 투명해짐
        }

        string targetLayerName = isVisible ? _visibleLayer : _invisibleLayer;
        gameObject.layer = LayerMask.NameToLayer(targetLayerName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isDisarmed || !_isInitialized) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && player.Inventory != null)
        {
            float playerCurrentWeight = player.Inventory.GetTotalCarryWeight();

            Debug.Log($"현재 플레이어 무게: {playerCurrentWeight}kg / 작동 요구 무게: {_activationRequiredWeight}kg");

            if (playerCurrentWeight >= _activationRequiredWeight)     // 플레이어의 가방 무게가 작동 기준 수치 이상인지 확인
            {
                TriggerWeightPlateAlert();
            }
            else
            {
                Debug.Log($"작동 기준 중량 미만입니다.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            if (_pressurePlateMesh != null)
            {
                _pressurePlateMesh.localPosition = _initialPlateLocalPosition;
            }
        }
    }

    private void TriggerWeightPlateAlert()
    {
        Debug.LogWarning($"{_disarmObjName} (ID: {_disarmObjId})가 작동 기준 중량 이상의 무게를 감지했습니다.");

        if (_pressurePlateMesh != null)
        {
            _pressurePlateMesh.localPosition = _initialPlateLocalPosition + new Vector3(0f, _pressedYOffset, 0f);
        }

        if (GameManager.Instance != null)
        {
            float finalReduction = (_timeReductionAmountList != null && _timeReductionAmountList.Count > 0)
                ? _timeReductionAmountList[0]
                : _myTimeReductionAmount;

            GameManager.Instance.SendMessage("ReduceTimer", finalReduction, SendMessageOptions.DontRequireReceiver);
        }
    }

    protected override void OnDisarm()
    {
        Debug.Log($"ID: {_disarmObjId} - 무력화되어 더이상 작동하지 않습니다.");

        _isTrackingGlasses = false;

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        if (_plateRenderer != null) _plateRenderer.enabled = true;
        gameObject.layer = LayerMask.NameToLayer(_invisibleLayer);

        if (_pressurePlateMesh != null)
        {
            _pressurePlateMesh.localPosition = _initialPlateLocalPosition;
        }
    }

    private bool CheckIfPlayerWearingGlasses()
    {
        // 테스트용으로 G키를 누르고 있으면 안경을 쓴 것으로 간주
        if (Input.GetKey(KeyCode.G))
        {
            return true;
        }
        return false;
    }
}