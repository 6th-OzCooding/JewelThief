// 파일명: SpyDroneTrap.cs
using UnityEngine;

public class SpyDroneTrap : BaseDisarmableObejct
{
    [Header("드론 비행 설정")]
    [SerializeField] private Transform[] _waypoints;     // 순찰할 경로 지점들
    [SerializeField] private float _moveSpeed = 3f;     // 드론 이동 속도
    [SerializeField] private float _rotationSpeed = 5f;     // 회전 속도

    [Header("감시 시야 설정")]
    [SerializeField] private float _viewDistance = 8f;     // 시야 거리
    [SerializeField] private float _viewAngle = 60f;     // 시야각
    [SerializeField] private LayerMask _targetLayer;     // 플레이어를 감지 타겟으로 둠

    [Header("감지 패널티 설정")]
    [SerializeField] private float _spImmediateDamage = 10f;    // 드론에 플레이어가 감지될 경우 sp 차감
    [SerializeField] private float _myTimeReductionAmount = 20f;

    private int _currentWaypointIndex = 0;
    private float _alertCooldown = 2f;     // 경보 중복 발동 방지 쿨타임
    private float _cooldownTimer = 0f;

    protected override void LoadData(string id)
    {
        _disarmObjName = "감시 드론";

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
        _currentWaypointIndex = 0;
        _cooldownTimer = 0f;
    }

    private void Update()
    {
        if (_isDisarmed) return;

        Patrol();

        if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;

        PlayerController targetPlayer = GetPlayerInViewComponent();
        if (targetPlayer != null && _cooldownTimer <= 0f)
        {
            TriggerDroneAlert(targetPlayer);
            _cooldownTimer = _alertCooldown;     // 쿨타임 가동
        }
    }

    private void Patrol()
    {
        if (_waypoints == null || _waypoints.Length == 0) return;
        Transform targetWaypoint = _waypoints[_currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, _moveSpeed * Time.deltaTime);

        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.2f)
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;
        }
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
                float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

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

    private void TriggerDroneAlert(PlayerController player)
    {
        Debug.LogWarning($"{_disarmObjName} (ID: {_disarmObjId})이 플레이어를 감지했습니다.");

        player.TakePlayerSpDamage(_spImmediateDamage);

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
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;     // 무력화될 경우 리지드바디 비활성화 (땅에 추락)
            rb.isKinematic = false;
        }
    }
}