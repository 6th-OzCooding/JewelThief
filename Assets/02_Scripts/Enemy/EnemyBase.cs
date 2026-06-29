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
    private CancellationTokenSource _cts;
    public CancellationToken CancelToken => _cts != null ? _cts.Token : CancellationToken.None;
    public EnemyStateContext StateContext { get; private set; }

    public float WalkSpeed => _walkSpeed;
    public float RunSpeed => _runSpeed;

    // 상태 클래스가 전환 판단에 사용하는 거리값
    public float AttackRadius => _attackRadius;
    public float ViewRadius => _viewRadius;
    public float MinApproachDistance => _minApproachDistance; // 상태별 stoppingDistance 복구에 사용

    public GameObject TargetPlayer { get; private set; }

    public Vector3 DirToTarget { get; set; } = Vector3.zero; // 플레이어와의 방향 초기화
    public float DstToTarget { get; set; } = 0.0f; // 플레이어까지의 거리 초기화

    // 시야(Raycast) 확보 여부 => 상태 클래스가 Chase 전환을 판단할 때 사용
    public bool HasLineOfSight { get; private set; } = false;

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
    // 최소 접근 거리 ( 이 거리 이상으로 다가가지 못하게 수정)
    [SerializeField] private float _minApproachDistance = 2.0f;
    // 테이저와 곤봉을 위한 딜레이 변수
    [SerializeField] private float _attackDelay = 0f;
    // 플레이어의 스태미나를 줄이는 데미지 변수
    [SerializeField] private float _attackDamage = 0f;
    public float AttackDamage => _attackDamage;
    // 원거리 공격하는 애들한테만 쓰이는 쿨타임 중에 움직일 때, 사거리 안에 있으면 발만 움직이는 것 해결하기 위한 변수
    public bool IsAttackCooldown { get; private set; } = false;

    // 기본값은 0초로 잡음
    private float _attackTimer = 0f; 
    
    private float _detectTimer = 0.0f; // 탐지되고 나면 다시 초기화하기 위한 변수
    private float _detectDelay = 0.1f; // Collider로 탐지하는데 0.1초 제한을 두기 위한 변수

    private void Awake()
    {
        // 고정적인 컴포넌트 할당은 Awake에서 1번만 실행
        Nav = GetComponent<NavMeshAgent>();
        Anim = GetComponent<Animator>();
        Rb = GetComponent<Rigidbody>();

        if (Rb != null) Rb.isKinematic = true;
        StateContext = new EnemyStateContext(this);
    }

    private void OnEnable()
    {
        // 남아있을 수 있는 토큰들을 취소 후 다시 발급받는다.
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        // 플레이어에 관한 상태 리셋
        TargetPlayer = null;
        DirToTarget = Vector3.zero;
        DstToTarget = 0f;
        _attackTimer = 0f;
        _detectTimer = 0f;
        IsAttackCooldown = false;
        // NavMesh 초기화
        if (Nav != null)
        {
            Nav.stoppingDistance = _minApproachDistance;
            if (Nav.isOnNavMesh)
            {
                Nav.isStopped = false;
                Nav.ResetPath();
            }
        }
        // 애니메이션 초기화
        if (Anim != null)
        {
            Anim.speed = 1f;
            Anim.SetBool("isRun", false);
        }
        // 시작 상태는 Normal로 지정
        if (StateContext != null)
        {
            StateContext.Initialize(StateContext.NormalState);
        }
    }

    // Pool 반갑시 진행중이던 공격, 딜레이 타이머 종료
    private void OnDisable()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }
    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
        {
            // 정지 상태일 때는 애니메이션과 움직임을 멈춘다
            if (Anim != null) Anim.speed = 0f;
            if (Nav != null && Nav.isOnNavMesh) Nav.isStopped = true;

            return;
        }
        // 원거리 공격하는 몬스터는 멈추는 로직이 있으므로 예외처리
        if (StateContext.CurrentState != StateContext.AttackState)
        {
            if (Anim != null && Anim.speed == 0f) Anim.speed = 0f;
            if (Nav != null && Nav.isOnNavMesh && Nav.isStopped) Nav.isStopped = false;
        }
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
        // 게임이 일시정지일 때 상태 전환이나 이동 정지
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        if (Nav != null)
        {
            if (StateContext.CurrentState == StateContext.ChaseState && !IsAttackCooldown)
            {
                // 1. 플레이어가 공격 사거리 안에 들어왔을 때 (시야가 확보된 경우에만)
                if (DstToTarget <= _attackRadius && HasLineOfSight) // 벽 너머 공격 방지를 위해 HasLineOfSight 조건 추가
                {
                    // NavMesh 이동 강제 정지 (밀림 방지)
                    Nav.isStopped = true;
                    Nav.velocity = Vector3.zero;

                    // [핵심] Idle 애니메이션이 없으므로, 애니메이션 속도를 0으로 만들어 발을 멈춥니다.
                    Anim.speed = 0f;

                    // 플레이어를 바라보며 조준
                    if (DirToTarget != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(DirToTarget);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5.0f);
                    }

                    // 딜레이 타이머 굴리기
                    _attackTimer += Time.fixedDeltaTime;

                    // 딜레이 시간이 꽉 찼다면 (도망치지 않고 버텼다면) 공격 시작
                    if (_attackTimer >= _attackDelay)
                    {
                        Anim.speed = 1f; // 애니메이션 속도 원상복구
                        StateContext.TransitionTo(StateContext.AttackState);
                        _attackTimer = 0f;
                    }
                }
                // 2. 공격 딜레이 도중 플레이어가 사거리 밖으로 도망쳤을 때 (또는 그냥 추적 중일 때)
                else
                {
                    _attackTimer = 0f; // 공격 타이머 초기화 (공격 취소)

                    Nav.isStopped = false; // 다시 추적 시작

                    Anim.speed = 1f; // 걷기 애니메이션 다시 재생
                }
            }
            // ChaseState가 아닐 때 (Normal, Track 등)
            else
            {
                // AttackState가 아닐 때는 항상 애니메이션 정상 재생 + 이동 정지 해제
                if (StateContext.CurrentState != StateContext.AttackState)
                {
                    Anim.speed = 1f;
                    // Chase 사거리 진입 시 걸린 Nav.isStopped가 Track/Normal 전환 후에도 남아 이동이 멈추는 문제 해결
                    if (Nav.isOnNavMesh && Nav.isStopped) Nav.isStopped = false;
                }
            }
        }

        StateContext.Update();
    }
    // 공격상태에서 쿨타임 관련 메서드
    public void SetAttackCooldown(bool cooldown)
    {
        IsAttackCooldown = cooldown;
    }

    private void DetectPlayer()
    {
        DirToTarget = Vector3.zero;
        DstToTarget = 0.0f;
        HasLineOfSight = false; // 매 탐지마다 시야 확보 여부 초기화

        // 매 탐지마다 타겟을 초기화 (범위 밖으로 나가면 TargetPlayer가 null이 되게끔)
        TargetPlayer = null;

        Collider[] targetsInDetectRadius = Physics.OverlapSphere(transform.position, _detectRadius);

        for (int i = 0; i < targetsInDetectRadius.Length; i++)
        {
            Collider target = targetsInDetectRadius[i];

            if (target.CompareTag("Player"))
            {
                TargetPlayer = target.gameObject;

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
                                HasLineOfSight = true; //상태 전환 대신 시야 확보 플래그만 set
                            }
                        }
                    }
                }
                break; // 플레이어를 발견 했다면 빠져나오게
            }
        }
    }

    public async UniTaskVoid AttackRoutine()
    {
        // 만약 Enemy가 없어진다면 (오브젝트가 삭제된다면) 바로 공격 함수 종료
        if (CancelToken.IsCancellationRequested) return;

        Debug.Log("Enemy가 Player를 공격했습니다!");

        Anim.SetBool("isRun", false);
        Anim.SetTrigger("isAttack");

        SetAttackCooldown(true);

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
        // 딱 공격하는 순간 일시정지가 되면 멈출 수 있게 
        await UniTask.WaitWhile(() => GameManager.Instance != null && GameManager.Instance.IsPaused, cancellationToken: CancelToken);

        // 플레이어에게 공격을 했을 때, 데미지로 플레이어의 스태미나를 감소하게 하는 메서드
        if (TargetPlayer != null)
        {
            PlayerController player = TargetPlayer.GetComponent<PlayerController>();
            if (player != null)
            {
                player.OnPlayerHit();
            }
        }

        // 현재 애니메이션의 길이를 알아내어 기다림
        float currentAnimLength = 1.0f;
        if (Anim != null && Anim.layerCount > 0)
        {
            var stateInfo = Anim.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.length > 0) currentAnimLength = stateInfo.length;
        }

        // 공격 애니메이션이 끝날 때까지 대기
        await UniTask.Delay(TimeSpan.FromSeconds(currentAnimLength - 0.1f), cancellationToken: CancelToken);

        // 공격이 끝나면 상태를 다시 Normal로 변경(초기화)
        // 이후 탐지/전환은 Update의 주기적 DetectPlayer + NormalState의 전환 판단이 담당
        StateContext.TransitionTo(StateContext.NormalState); // 뒤따르던 DetectPlayer() 직접 호출 제거

        await UniTask.Delay(TimeSpan.FromSeconds(_attackDelay), cancellationToken: CancelToken);

        // 쿨타임이 끝나면 다시 공격이 가능하도록 해제합니다.
        SetAttackCooldown(false);
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