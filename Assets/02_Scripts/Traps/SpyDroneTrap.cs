using UnityEngine;

public class SpyDroneTrap : MonoBehaviour
{
    [Header("데이터 정보")]
    [SerializeField] private int _trapId = 40000003;

    [Header("드론 비행 설정")]
    [SerializeField] private Transform[] _waypoints;      // 드론이 순찰할 경로점
    [SerializeField] private float _moveSpeed = 1f;        // 비행 속도 (이하 수치는 전부 임시 수치)
    [SerializeField] private float _rotationSpeed = 1f;    // 회전 속도

    [Header("감시 시야 설정")]
    [SerializeField] private float _viewDistance = 1f;     // 감지 거리
    [SerializeField] private float _viewAngle = 1f;        // 시야각
    [SerializeField] private LayerMask _targetLayer;        // 플레이어 레이어와 연동

    [Header("감지 패널티 설정")]
    [SerializeField] private float _timeReductionAmount = 1f; // 차감 시간

    private int _currentWaypointIndex = 0;
    private bool _isDisarmed = false;

    private void Update()
    {
        if (_isDisarmed) return;

        Patrol();    // 순찰 경로 이동 로직

        if (CheckPlayerInView())     // 플레이어 시야 감지 로직
        {
            TriggerDroneAlert();
        }
    }

    private void Patrol()
    {
        if (_waypoints == null || _waypoints.Length == 0) return;

        Transform targetPoint = _waypoints[_currentWaypointIndex];

        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, _moveSpeed * Time.deltaTime);    // 목표(웨이포인트)를 향해 이동하도록

        Vector3 direction = (targetPoint.position - transform.position).normalized;     // 드론이 이동 방향을 부드럽게 바라보도록 회전
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.2f)     // 웨이포인트에 거의 도달했다면 다음 웨이포인트로 목표 인덱스 교체
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;
        }
    }

    private bool CheckPlayerInView()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, _viewDistance, _targetLayer);

        if (targets.Length > 0)
        {
            Transform playerTransform = targets[0].transform;
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;

            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);     // 드론의 정면(transform.forward) 기준 각도 계산

            if (angleToPlayer < _viewAngle / 2f)
            {
                if (!Physics.Raycast(transform.position, directionToPlayer, _viewDistance, LayerMask.GetMask("Obstacle")))     // 장애물(벽)에 가려졌는지 체크
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void TriggerDroneAlert()
    {
        Debug.LogWarning($"[드론 경보] ID: {_trapId} - 드론이 플레이어를 탐지했습니다.");

        // GameManager.Instance.AlertManager.ReduceTimer(_timeReductionAmount);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SendMessage("ReduceTimer", _timeReductionAmount, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            Debug.Log($"[임시 디버그] 싱글톤 매니저가 없어 임시 수치 차감: {_timeReductionAmount}초");
        }
    }

    public void DisarmTrap()
    {
        _isDisarmed = true;

        Rigidbody rb = GetComponent<Rigidbody>();     // 드론이 무력화되면 비활성화돼 추락하도록 Rigidbody 제거
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        Debug.Log($"[함정 해제] ID: {_trapId} - 드론 무력화.");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 origin = transform.position;
        Gizmos.DrawWireSphere(origin, _viewDistance);

        Vector3 forward = transform.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -_viewAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, _viewAngle / 2, 0) * forward;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(origin, origin + leftBoundary * _viewDistance);
        Gizmos.DrawLine(origin, origin + rightBoundary * _viewDistance);
    }
}