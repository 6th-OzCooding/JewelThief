using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    private Animator _anim;
    // Transform 대신 Rigidbody를 사용하기 위하여 => Transform은 벽에 끼는 현상 순간이동 하는 현상이 나타날 수 있으므로
    private Rigidbody _rb;

    // 시야각 생성 (조정 가능)
    [SerializeField] private float _viewRadius = 6.0f;
    [SerializeField] private float _viewAngle = 120.0f;

    // 플레이어를 '인지'하는 거리 (조정 가능)
    [SerializeField] private float _detectRadius = 12.0f;
    // 거리안에 없을 시 (걷는 속도), 거리 안에 있으면서 시야각 안에 플레이어가 있다면 (뛰는 속도)
    [SerializeField] private float _walkSpeed = 2.5f;
    [SerializeField] private float _runSpeed = 4.5f;
    // 공격 사거리 (조정 가능)
    [SerializeField] private float _attackRadius = 1.5f;
    
    // 플레이어를 공격하지 않으면 공격하지 않게끔 설정
    private bool _isAttacking = false;
    // 원래 FindPlayerInSight 메서드에 있던 변수들을 미리 설정
    private bool _isPlayerInDetectRange = false; // 거리 안에 들어왔는지
    private bool _isPlayerSpotted = false; // 플레이어가 탐지 되었는지 안되었는지
    private Vector3 _dirToTarget = Vector3.zero; // 플레이어와의 방향 초기화
    private float _dstToTarget = 0.0f; // 플레이어까지의 거리 초기화
    private float _detectTimer = 0.0f; // 탐지되고 나면 다시 초기화하기 위한 변수
    private float _detectDelay = 0.1f; // Collider로 탐지하는데 0.1초 제한을 두기 위한 변수

    private void Start()
    {
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // 0.1초마다 한 번씩만 시야 탐지 => 0.1초가 넘어가면 다시 초기화
        _detectTimer += Time.deltaTime;
        if (_detectTimer >= _detectDelay)
        {
            _detectTimer = 0.0f;
            DetectPlayer(); // 탐지 메서드 호출
        }
    }

    private void FixedUpdate()
    {
        if (_isPlayerSpotted && _dstToTarget <= _attackRadius)
        {
            EnemyAttack(); // 공격 메서드 호출
        }

        else if (!_isAttacking)
        {
            EnemyMovement(); // 이동 메서드 호출
        }
    }

    private void DetectPlayer()
    {
        // 매 탐지되는 순간마다 초기화
        _isPlayerInDetectRange = false;
        _isPlayerSpotted = false;
        _dirToTarget = Vector3.zero;
        _dstToTarget = 0.0f;

        Collider[] targetsInDetectRadius = Physics.OverlapSphere(transform.position, _detectRadius);

        for (int i = 0; i <targetsInDetectRadius.Length; i++)
        {
            Collider target = targetsInDetectRadius[i];

            if (target.CompareTag("Player"))
            {
                // 플레이어가 거리안에 들어왔으므로 true
                _isPlayerInDetectRange = true;

                // 플레이어와의 방향 확인
                _dirToTarget = (target.transform.position - transform.position);
                _dirToTarget.y = 0.0f;
                _dirToTarget.Normalize();
                
                // 플레이어와의 거리 확인
                _dstToTarget = Vector3.Distance(transform.position, target.transform.position);

                // 플레이어가 시야각(거리) 안에 들어왔는지 확인
                if (_dstToTarget <= _viewRadius)
                {
                    // 플레이어와의 각도 확인
                    float angle = Vector3.Angle(transform.forward, _dirToTarget);

                    if (angle <= _viewAngle / 2)
                    {
                        // 살짝 띄워서 RayCast를 쏜다.
                        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

                        if (Physics.Raycast(rayOrigin, _dirToTarget, out RaycastHit hit, _dstToTarget))
                        {
                            // 플레이어를 바로 탐지한 경우 (다른 물체가 가로막지 않은 경우)
                            if (hit.collider.CompareTag("Player"))
                            {
                                _isPlayerSpotted = true;
                            }
                        }
                    }
                }
                break; // 플레이어를 발견 했다면 빠져나오게
            }
        }
    }

    private void EnemyMovement()
    {
        if (_isPlayerSpotted) // 시야각에 들어왔다면
        {
            // 시야각에 들어왔으므로 달리기로 애니메이션 적용
            _anim.SetBool("isRun", true);

            Quaternion targetRotation = Quaternion.LookRotation(_dirToTarget);

            _rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 3.5f));
            // Transform 대신 MovePosition
            _rb.MovePosition(transform.position + transform.forward * _runSpeed * Time.fixedDeltaTime);
        }

        else if (_isPlayerInDetectRange) // 거리 안에 있다면 플레이어 방향으로 걸어감
        {
            // 거리 안에 들어왔으므로 걷기로 애니메이션 적용
            _anim.SetBool("isRun", false);

            Quaternion targetRotation = Quaternion.LookRotation(_dirToTarget);
            _rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 2.0f));
            // Transform 대신 MovePosition
            _rb.MovePosition(transform.position + transform.forward * _walkSpeed * Time.fixedDeltaTime);
        }

        else // 아무것도 아닌 경우 가던 방향으로 걸어감
        {
            _anim.SetBool("isRun", false);
            _rb.MovePosition(transform.position + transform.forward * _walkSpeed * Time.fixedDeltaTime);
        }
    }

    private void EnemyAttack() // 공격하는 메서드
    {
        _anim.SetBool("isRun", false); // 공격해야 하는 애니메이션으로 뛰는 애니메이션을 멈춘다.

        if (!_isAttacking)
        {
            // 공격 애니메이션 시작
            StartCoroutine(AttackRoutine());
        }

        // 공격할 때에 플레이어를 바라보며 공격하도록 설정
        Quaternion targetRotation = Quaternion.LookRotation(_dirToTarget);
        _rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 3.0f));
    }


    private IEnumerator AttackRoutine()
    {

        _isAttacking = true;

        Debug.Log("Enemy가 Player를 공격했습니다!");
        _anim.SetTrigger("isAttack");
        
        // 잠깐 기다리는 시간
        yield return new WaitForSeconds(0.1f);

        // 현재 애니메이션의 길이를 알아내어 기다림
        float currentAnimLength = _anim.GetCurrentAnimatorStateInfo(0).length;
        
        // 기다린 시간 삭제
        yield return new WaitForSeconds(currentAnimLength - 0.1f);

        _isAttacking = false;
    }

    private void OnDrawGizmos()
    {
        if (_anim == null && !Application.isPlaying) return;

        Vector3 origin = transform.position + Vector3.up * 0.3f;
        Vector3 forward = transform.forward;
        // 거리 안에 있는 걸 확인하기 위한 기즈모 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectRadius);
        
        // 시야각 안에 있는 걸 확인하기 위한 기즈모 (빨간색) => 부채꼴
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

        // 공격 사거리를 확인하기 위한 기즈모 (파란색) => 부채꼴
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(origin, origin + leftBoundary * _attackRadius);
        Gizmos.DrawLine(origin, origin + rightBoundary * _attackRadius);

        Vector3 prevAttactPoint = origin + leftBoundary * _attackRadius;

        for (int i = 1; i <= segments; i++)
        {
            // 왼쪽에서부터 시작 해서 부채꼴 모양으로 만든다.
            float currentAngle = (-_viewAngle / 2) + (angleStep * i);
            Vector3 currentDir = Quaternion.Euler(0, currentAngle, 0) * forward;
            Vector3 currentPoint = origin + currentDir * _attackRadius;

            Gizmos.DrawLine(prevAttactPoint, currentPoint);
            prevAttactPoint = currentPoint;
        }
    }
}