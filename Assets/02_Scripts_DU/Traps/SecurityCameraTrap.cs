using UnityEngine;

public class SecurityCameraTrap : MonoBehaviour
{
    [Header("데이터 정보")]
    [SerializeField] private int _trapId = 40000002;

    [Header("카메라 회전 설정")]
    [SerializeField] private Transform _cameraHead;
    [SerializeField] private float _rotationAngle = 1f;   //좌우 회전각
    [SerializeField] private float _rotationSpeed = 1f;   //회전 속도

    [Header("감시 시야 설정")]
    [SerializeField] private float _viewDistance = 1f;   //탐지 범위
    [SerializeField] private float _viewAngle = 1f;   // 시야각
    [SerializeField] private LayerMask _targetLayer;

    [Header("감지 패널티 설정")]
    [SerializeField] private float _detectionRequiredTime = 1f;  // 감지 완료까지 버텨야 하는 시간 (초)
    [SerializeField] private float _timeReductionAmount = 1f;   // 걸렸을 때 차감되는 제한시간

    private float _detectionTimer = 0f;
    private bool _isDisarmed = false;

    private void Update()
    {
        if (_isDisarmed) return;

        RotateCameraHead();   // 카메라 머리 회전

        if (CheckPlayerInView())   // 카메라에 플레이어가 탐지될 경우
        {
            _detectionTimer += Time.deltaTime;   // 감지 타이머 상승
            Debug.Log($"[카메라 경고] 플레이어 감지.  ({_detectionTimer:F1}/{_detectionRequiredTime}초)");

            // 카메라 경고 사운드 및 빨간 불빛 이펙트 켜기

            if (_detectionTimer >= _detectionRequiredTime)
            {
                TriggerCameraAlert();
                _detectionTimer = 0f; // 발동 후 타이머 초기화
            }
        }
        else
        {
            _detectionTimer = Mathf.Max(0f, _detectionTimer - Time.deltaTime);   // 플레이어가 감지 범위를 벗어나면 타이머 감소
        }
    }

    private void RotateCameraHead()
    {
        if (_cameraHead == null) return;

        float angle = Mathf.Sin(Time.time * _rotationSpeed) * (_rotationAngle / 2f);   // 지속적으로 좌우로 흔들림
        _cameraHead.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

    private bool CheckPlayerInView()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, _viewDistance, _targetLayer);   // 주변에 있는 콜라이더 탐색

        if (targets.Length > 0)
        {
            Transform playerTransform = targets[0].transform;
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;

            float angleToPlayer = Vector3.Angle(_cameraHead.forward, directionToPlayer);   // 카메라 정면과 플레이어 방향 사이의 각도 계산

            if (angleToPlayer < _viewAngle / 2f)   // 시야각 절반 내에 있고, 레이캐스트로 벽에 가려지지 않았는지 검사
            {
                if (!Physics.Raycast(transform.position, directionToPlayer, _viewDistance, LayerMask.GetMask("Obstacle")))
                {
                    return true;   // 감지 성공
                }
            }
        }
        return false;
    }

    private void TriggerCameraAlert()
    {
        Debug.LogWarning($"[카메라 발동] ID: {_trapId} - 침입자가 완전히 감지되었습니다. 사이렌 활성화.");

        // 카메라 경고 사운드 및 빨간 불빛 이펙트 켜기

        if (GameManager.Instance != null && GameManager.Instance.AlertManager != null)   // 경계 레벨 타이머 감소
        {
            GameManager.Instance.AlertManager.ReduceTimer(_timeReductionAmount);
        }
    }

    // 도구로 무력화할 때 호출될 함수
    public void DisarmTrap()
    {
        _isDisarmed = true;
        Debug.Log($"[함정 해제] ID: {_trapId} - 무력화 도구에 의해 카메라 전원이 차단되었습니다.");
    }

    private void OnDrawGizmos()
    {
        // 에디터 뷰에서 카메라 시야각 기즈모로 표현해주세요
        if (_cameraHead == null) return;

        Gizmos.color = Color.blue;
        Vector3 origin = transform.position;
        Gizmos.DrawWireSphere(origin, _viewDistance);

        Vector3 forward = _cameraHead.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -_viewAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, _viewAngle / 2, 0) * forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + leftBoundary * _viewDistance);
        Gizmos.DrawLine(origin, origin + rightBoundary * _viewDistance);
    }
}