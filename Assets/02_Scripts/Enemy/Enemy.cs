using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Animator _anim;

    // 시야각 생성
    private float _viewRadius = 5.0f;
    private float _viewAngle = 120.0f;
    // 속도 조정 예정
    private float _moveSpeed = 3.0f;

    private void Start()
    {
        _anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // 플레이어를 인지했는지 계속 확인하는 함수
        FindPlayerInSight();
    }

    private void FindPlayerInSight()
    {
        bool isPlayerSpotted = false;

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, _viewRadius);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Collider target = targetsInViewRadius[i];

            if (target.CompareTag("Player"))
            {
                // 플레이어 방향 확인
                Vector3 dirToTarget = (target.transform.position - transform.position).normalized;

                // 플레이어와의 거리 확인
                float dstToTarget = Vector3.Distance(transform.position, target.transform.position);

                // 플레이어와의 각도 계산
                float angle = Vector3.Angle(transform.forward, dirToTarget);

                // 시야각 내에 들어왔는지 확인 (양 옆으로 계산 되기 때문에 2로 나눠준다)
                if (angle <= _viewAngle / 2)
                {
                    // 바닥에 바로 쏘면 문제가 생길 수 있으므로 살짝 띄운다.
                    Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;

                    if (Physics.Raycast(rayOrigin, dirToTarget, out RaycastHit hit, dstToTarget))
                    {
                        // 플레이어를 바로 탐지한 경우 (다른 물체가 가로막지 않은 경우)
                        if (hit.collider.CompareTag("Player"))
                        {
                            isPlayerSpotted = true;

                            Debug.Log("Raycast로 플레이어 발견!");
                            // 발견을 했다면 달리는 애니메이션 적용
                            _anim.SetBool("isRun", true);

                            // 플레이어 쪽으로 몸을 트는 코드
                            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
                            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);

                            transform.Translate(Vector3.forward * _moveSpeed * Time.deltaTime);

                            break;
                        }
                    }
                }
            }
        }
        // 시야각에서 사라지거나 못찾은 경우에는 달리지 않고 걷게 함
        if (!isPlayerSpotted)
        {
            _anim.SetBool("isRun", false);
        }
    }

    private void OnDrawGizmos()
    {
        if (_anim == null && !Application.isPlaying) return;

        Vector3 origin = transform.position + Vector3.up * 0.3f;
        Gizmos.color = Color.red;

        Vector3 forward = transform.forward;
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