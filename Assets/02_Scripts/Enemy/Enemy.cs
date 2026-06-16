using UnityEngine;

public class Enemy : MonoBehaviour
{
    // 적의 애니메이션을 담는 변수
    private Animator _anim;
    // 부채꼴을 위하여 원 콜리더를 생성
    private SphereCollider _col;

    // 적의 시야를 정하는 거리와 각도
    private float _viewRadius = 5.0f;
    private float _viewAngle = 150.0f;

    private float _moveSpeed = 3.0f;

    private void Start()
    {
        // 애니메이션을 찾고 그 애니메이션을 담을 변수인 _anim으로 지정
        _anim = GetComponent<Animator>();

        _col = GetComponent<SphereCollider>();
        _col.isTrigger = true;
        _col.radius = _viewRadius;
    }

    private void OnTriggerStay(Collider other)
    {
        // 플레이어의 태그를 일단 확인하기 => 바뀌는 경우에는 바꾸면 됨
        if (other.CompareTag("Player"))
        {
            Vector3 dirToTarget = (other.transform.position - transform.position).normalized;

            float angle = Vector3.Angle(transform.forward, dirToTarget);

            // 정면을 기준으로 양쪽을 계산하기 때문에 120도면 왼쪽60, 오른쪽 60으로 설정하기 위해 2로 나눔
            if (angle <= _viewAngle / 2)
            {
                // 시야각 안에 들어온 경우 행동을 작성하면 됨
                Debug.Log("플레이어 발견!");
                // 시야각에 들어왔으므로 플레이어를 향해 뛰어가는 애니메이션 적용
                _anim.SetBool("isRun", true);

                // 플레이어쪽으로 몸을 트는 코드
                Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);
                // 앞을 보며 달려가는 코드
                transform.Translate(Vector3.forward * _moveSpeed * Time.deltaTime);
            }

            else
            {
                _anim.SetBool("isRun", false);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_anim == null) return;

        Vector3 origin = transform.position + Vector3.up * 0.3f;
        Gizmos.color = Color.red;

        Vector3 forward = transform.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -_viewAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, _viewAngle / 2, 0) * forward;

        Gizmos.DrawLine(origin, origin + leftBoundary * _viewRadius);
        Gizmos.DrawLine(origin, origin + rightBoundary * _viewRadius);

        // 많이 자르면 예쁘게 보이지만, 시간과 자원을 많이 잡아먹으므로 적당히 25개로 자름 => 하나당 6으로 계산
        int segments = 25;
        float angleStep = _viewAngle / segments;
        Vector3 prevPoint = origin + leftBoundary * _viewRadius;

        for (int i = 1; i <= segments; i++)
        {
            // 왼쪽부터(- 60도부터 그리기 시작)
            float currentAngle = (-_viewAngle / 2) + (angleStep * i);
            Vector3 currentDir = Quaternion.Euler(0, currentAngle, 0) * forward;
            Vector3 currentPoint = origin + currentDir * _viewRadius;

            Gizmos.DrawLine(prevPoint, currentPoint);
            prevPoint = currentPoint;
        }
    }
}