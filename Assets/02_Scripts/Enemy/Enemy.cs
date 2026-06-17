using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Animator _anim;

    // 시야각 생성 (조정 가능)
    [SerializeField] private float _viewRadius = 6.0f;
    [SerializeField] private float _viewAngle = 180.0f;

    // 플레이어를 '인지'하는 거리 (조정 가능)
    [SerializeField] private float _detectRadius = 12.0f;
    // 거리안에 없을 시 (걷는 속도), 거리 안에 있으면서 시야각 안에 플레이어가 있다면 (뛰는 속도)
    [SerializeField] private float _walkSpeed = 2.5f;
    [SerializeField] private float _runSpeed = 4.5f;

    private void Start()
    {
        _anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // 플레이어를 인지했는지 확인하는 함수
        FindPlayerInSight();
    }

    private void FindPlayerInSight()
    {
        // 플레이어가 거리안에 있는지 확인하는 변수
        bool isPlayerInDetectRange = false;
        // 플레이어가 시야각 안에 들어왔는지 확인하는 변수
        bool isPlayerSpotted = false;

        Vector3 dirToTarget = Vector3.zero;

        Collider[] targetsInDetectRadius = Physics.OverlapSphere(transform.position, _detectRadius);

        for (int i = 0; i < targetsInDetectRadius.Length; i++)
        {
            Collider target = targetsInDetectRadius[i];

            if (target.CompareTag("Player"))
            {
                // 플레이어란 태그가 거리안에 들어왔다면
                isPlayerInDetectRange = true;
                // 플레이어 방향 확인
                dirToTarget = (target.transform.position - transform.position);
                dirToTarget.y = 0f;
                dirToTarget.Normalize();

                // 플레이어와의 거리 확인
                float dstToTarget = Vector3.Distance(transform.position, target.transform.position);

                // 플레이어가 시야각(거리) 안에 들어왔는지 확인
                if (dstToTarget <= _viewRadius)
                {
                    // 플레이어와의 각도 확인
                    float angle = Vector3.Angle(transform.forward, dirToTarget);

                    if (angle <= _viewAngle / 2)
                    {
                        // 바닥에 바로 쏘면 문제가 생길 수 있으므로 살짝 띄운다.
                        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

                        if (Physics.Raycast(rayOrigin, dirToTarget, out RaycastHit hit, dstToTarget))
                        {
                            // 플레이어를 바로 탐지한 경우 (다른 물체가 가로막지 않은 경우)
                            if (hit.collider.CompareTag("Player"))
                            {
                                isPlayerSpotted = true;
                            }
                        }
                    }
                }
                break; // 플레이어를 발견 했다면 종료
            }
        }
        // 플레이어를 탐지했다면 달리도록
        if (isPlayerSpotted)
        {
            _anim.SetBool("isRun", true);

            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3.5f);
            transform.position += dirToTarget * _runSpeed * Time.deltaTime;
        }
        // 시야각에서 탐지되지는 않았지만, 거리 안에 있는 경우
        else if (isPlayerInDetectRange)
        {
            _anim.SetBool("isRun", false);
            // 플레이어 쪽으로 몸을 돌려서 걸어가도록
            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2.0f);
            transform.position += dirToTarget * _walkSpeed * Time.deltaTime;
        }
        // 거리 안에 플레이어가 없는 경우
        else
        {
            _anim.SetBool("isRun", false);
            // 원래 가던 방향으로 계속 가도록
            transform.position += transform.forward * _walkSpeed * Time.deltaTime;
        }
    }

    private void OnDrawGizmos()
    {
        if (_anim == null && !Application.isPlaying) return;

        Vector3 origin = transform.position + Vector3.up * 0.3f;
        Vector3 forward = transform.forward;
        // 거리 안에 있는 걸 확인하기 위한 기즈모 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectRadius);
        // 시야각 안에 있는 걸 확인하기 위한 기즈모 (빨간색) => 시야각
        Gizmos.color = Color.red;
        Vector3 leftBoundary = Quaternion.Euler(0, -_viewAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, _viewAngle / 2, 0) * forward;

        Gizmos.DrawLine(origin, origin + leftBoundary * _viewRadius);
        Gizmos.DrawLine(origin, origin + rightBoundary * _viewRadius);

        int segments = 20;
        float angleStep = _viewAngle / segments;
        Vector3 prevPoint = origin + leftBoundary * _viewRadius;

        for (int i = 1; i <= segments; i++)
        {
            // 왼쪽에서부터 시작 해서 부채꼴 모양으로 만든다.
            float currentAngle = (-_viewAngle / 2) + (angleStep * i);
            Vector3 currentDir = Quaternion.Euler(0, currentAngle, 0) * forward;
            Vector3 currentPoint = origin + currentDir * _viewRadius;

            Gizmos.DrawLine(prevPoint, currentPoint);
            prevPoint = currentPoint;
        }
    }
}