using UnityEngine;

public class SecurityCameraTrap : BaseDisarmableObejct
{
    [Header("카메라 회전 설정")]
    [SerializeField] private Transform _cameraHead;
    [SerializeField] private float _rotationAngle = 60f;
    [SerializeField] private float _rotationSpeed = 2f;

    [Header("감시 시야 설정")]
    [SerializeField] private float _viewDistance = 10f;
    [SerializeField] private float _viewAngle = 40f;
    [SerializeField] private LayerMask _targetLayer;

    [Header("감지 패널티 설정")]
    [SerializeField] private float _detectionRequiredTime = 1.5f;
    [SerializeField] private float _spDamagePerSecond = 5f;      // 감지 중일때 초당 sp 5씩 차감
    [SerializeField] private float _myTimeReductionAmount = 15f;

    private float _detectionTimer = 0f;

    protected override void LoadData(string id)
    {
        _disarmObjName = "보안 카메라";

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
        _detectionTimer = 0f;
    }

    private void Update()
    {
        if (_isDisarmed) return;

        RotateCameraHead();

        PlayerController detectedPlayer = GetPlayerInViewComponent();
        if (detectedPlayer != null)
        {
            detectedPlayer.TakePlayerSpDamagePerSecond(_spDamagePerSecond);

            _detectionTimer += Time.deltaTime;
            if (_detectionTimer >= _detectionRequiredTime)
            {
                TriggerCameraAlert();
                _detectionTimer = 0f;
            }
        }
        else
        {
            _detectionTimer = Mathf.Max(0f, _detectionTimer - Time.deltaTime);
        }
    }

    private void RotateCameraHead()
    {
        if (_cameraHead == null) return;
        float angle = Mathf.Sin(Time.time * _rotationSpeed) * (_rotationAngle / 2f);
        _cameraHead.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

    private PlayerController GetPlayerInViewComponent()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, _viewDistance, _targetLayer);
        if (targets.Length > 0)
        {
            PlayerController player = targets[0].GetComponent<PlayerController>();
            if (player != null)
            {
                Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
                float angleToPlayer = Vector3.Angle(_cameraHead.forward, directionToPlayer);

                if (angleToPlayer < _viewAngle / 2f)
                {
                    if (!Physics.Raycast(transform.position, directionToPlayer, _viewDistance, LayerMask.GetMask("Obstacle")))
                    {
                        return player;
                    }
                }
            }
        }
        return null;
    }

    private void TriggerCameraAlert()
    {
        Debug.LogWarning($"{_disarmObjName} (ID: {_disarmObjId})가 플레이어를 감지합니다.");
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
        Debug.Log($"ID: {_disarmObjId} - 무력화되어 더이상 플레이어를 감지하지 않습니다.");
        if (_cameraHead != null) _cameraHead.localRotation = Quaternion.Euler(45f, 0f, 0f);
    }
}