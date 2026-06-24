using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    public Animator Anim { get; private set; }
    // Transform 대신 Rigidbody를 사용하기 위하여 => Transform은 벽에 끼는 현상 순간이동 하는 현상이 나타날 수 있으므로
    public Rigidbody Rb { get; private set; }
    public NavMeshAgent Nav { get; private set; }
    // 취소 토큰을 미리 선언해두기
    public CancellationToken CancelToken { get; private set; }
    public EnemyStateContext StateContext { get; private set; }

    public float WalkSpeed => _walkSpeed;
    public float RunSpeed => _runSpeed;

    public GameObject TargetPlayer { get; private set; }

    public Vector3 DirToTarget { get; set; } = Vector3.zero; // 플레이어와의 방향 초기화
    public float DstToTarget { get; set; } = 0.0f; // 플레이어까지의 거리 초기화

    // 시야각 생성 (조정 가능)
    [SerializeField] private float _viewRadius = 6.0f;
    [SerializeField] private float _viewAngle = 120.0f;
    [SerializeField] private float _viewHeight = 0.5f;
    // 플레이어를 '인지'하는 거리 (조정 가능)
    [SerializeField] private float _detectRadius = 12.0f;
    // 거리안에 없을 시 (걷는 속도), 거리 안에 있으면서 시야각 안에 플레이어가 있다면 (뛰는 속도)
    [SerializeField] private float _walkSpeed = 2.5f;
    [SerializeField] private float _runSpeed = 4.5f;
    // 공격 사거리 (조정 가능)
    [SerializeField] private float _attackRadius = 1.5f;
    // 테이저와 곤봉을 위한 딜레이 변수
    [SerializeField] private float _attackDelay = 0f;
   
    // 기본값은 0초로 잡음
    private float _attackTimer = 0f; 
    
    private float _detectTimer = 0.0f; // 탐지되고 나면 다시 초기화하기 위한 변수
    private float _detectDelay = 0.1f; // Collider로 탐지하는데 0.1초 제한을 두기 위한 변수

    private void Awake()
    {
        // 토큰을 받은 오브젝트가 사라지면 받은 토큰을 없애도록
        CancelToken = this.GetCancellationTokenOnDestroy();
        Nav = GetComponent<NavMeshAgent>(); // NavMesh 할당

        // 상태 컨텍스트(관리자) 생성 및 주입
        StateContext = new EnemyStateContext(this);
    }

    private void Start()
    {
        Anim = GetComponent<Animator>();
        Rb = GetComponent<Rigidbody>();

        // 물리 충돌로 밀어내는 현상 방지
        if (Rb != null) Rb.isKinematic = true;

        // NavMesh 제동거리 설정: 플레이어 안으로 파고들어 밀어버리는 버그 방지
        if (Nav != null) Nav.stoppingDistance = _attackRadius;

        // 시작 상태를 Normal로 지정
        StateContext.Initialize(StateContext.NormalState);
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
        // 시야각에 들어오면서 공격사거리에 들어온 경우
        if (StateContext.CurrentState == StateContext.ChaseState && DstToTarget <= _attackRadius)
        {

            if (DirToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(DirToTarget);
                // AttackState와 동일한 속도(5.0f)로 부드럽게 회전
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5.0f);
            }

            _attackTimer += Time.fixedDeltaTime;
            // 근접은 바로 0초 원거리만 1.5초로 해놓기 (수정 가능)
            if (_attackTimer >= _attackDelay)
            {
                StateContext.TransitionTo(StateContext.AttackState);
                _attackTimer = 0f; // 초기화
            }
        }

        else
        {
            _attackTimer = 0f; // 범위에서 벗어나면 초기화
        }
        // 이미 공격중인 상태이면 공격 메서드 계속 호출 -> StateContext.Update()가 대신 처리함, 그 외는 움직이는 상태로 이동
        StateContext.Update();
    }

    private void DetectPlayer()
    {
        // 탐지한 경우 다음 상태를 Normal로 지정
        IEnemyState nextState = StateContext.NormalState;

        DirToTarget = Vector3.zero;
        DstToTarget = 0.0f;

        // 매 탐지마다 타겟을 초기화 (범위 밖으로 나가면 TargetPlayer가 null이 되게끔)
        TargetPlayer = null;

        Collider[] targetsInDetectRadius = Physics.OverlapSphere(transform.position, _detectRadius);

        for (int i = 0; i < targetsInDetectRadius.Length; i++)
        {
            Collider target = targetsInDetectRadius[i];

            if (target.CompareTag("Player"))
            {
                TargetPlayer = target.gameObject;

                // 플레이어가 거리안에 들어왔으므로 상태를 Track으로 변경
                nextState = StateContext.TrackState;

                // 플레이어와의 방향 확인
                Vector3 dir = (target.transform.position - transform.position);
                dir.y = 0.0f;
                DirToTarget = dir.normalized;

                // 플레이어와의 거리 확인
                DstToTarget = Vector3.Distance(transform.position, target.transform.position);

                // 플레이어가 시야각(거리) 안에 들어왔는지 확인
                if (DstToTarget <= _viewRadius)
                {
                    // 플레이어와의 각도 확인
                    float angle = Vector3.Angle(transform.forward, DirToTarget);

                    if (angle <= _viewAngle / 2)
                    {
                        // 살짝 띄워서 RayCast를 쏜다.
                        Vector3 rayOrigin = transform.position + Vector3.up * _viewHeight;

                        if (Physics.Raycast(rayOrigin, DirToTarget, out RaycastHit hit, DstToTarget))
                        {
                            // 플레이어를 바로 탐지한 경우 (다른 물체가 가로막지 않은 경우)
                            if (hit.collider.CompareTag("Player"))
                            {
                                nextState = StateContext.ChaseState;
                            }
                        }
                    }
                }
                break; // 플레이어를 발견 했다면 빠져나오게
            }
        }

        // 현재 공격중이 아닐 때, 새로운 상태를 다시 재적용
        if (StateContext.CurrentState != StateContext.AttackState)
        {
            StateContext.TransitionTo(nextState);
        }
    }

    public async UniTaskVoid AttackRoutine()
    {
        // 만약 Enemy가 없어진다면 (오브젝트가 삭제된다면) 바로 공격 함수 종료
        if (CancelToken.IsCancellationRequested) return;

        Debug.Log("Enemy가 Player를 공격했습니다!");
        Anim.SetTrigger("isAttack");

        // 곤봉을 던지는 Enemy 때문에 추가
        ThrowEnemy throwScript = GetComponent<ThrowEnemy>();
        if (throwScript != null)
        {
            throwScript.ThrowWeapon();
        }
        
        // 테이저 건을 쏘는 Enemy 때문에 추가
        ShootEnemy shootScript = GetComponent<ShootEnemy>();
        if (shootScript != null)
        {
            shootScript.Shoot(); // 쏘기 실행!
        }

        // 잠깐 기다리는 시간
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: CancelToken);

        // 현재 애니메이션의 길이를 알아내어 기다림
        float currentAnimLength = 1.0f;
        if (Anim != null && Anim.layerCount > 0)
        {
            var stateInfo = Anim.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.length > 0) currentAnimLength = stateInfo.length;
        }

        // 기다린 시간 삭제
        await UniTask.Delay(TimeSpan.FromSeconds(currentAnimLength - 0.1f), cancellationToken: CancelToken);

        // 공격이 끝나면 상태를 다시 Normal로 변경(초기화)
        StateContext.TransitionTo(StateContext.NormalState);
    }

    private void OnDrawGizmos()
    {
        if (Anim == null && !Application.isPlaying) return;

        Vector3 origin = transform.position + Vector3.up * _viewHeight;
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

        Vector3 prevAttackPoint = origin + leftBoundary * _attackRadius;

        for (int i = 1; i <= segments; i++)
        {
            // 왼쪽에서부터 시작 해서 부채꼴 모양으로 만든다.
            float currentAngle = (-_viewAngle / 2) + (angleStep * i);
            Vector3 currentDir = Quaternion.Euler(0, currentAngle, 0) * forward;
            Vector3 currentPoint = origin + currentDir * _attackRadius;

            Gizmos.DrawLine(prevAttackPoint, currentPoint);
            prevAttackPoint = currentPoint;
        }
    }
}