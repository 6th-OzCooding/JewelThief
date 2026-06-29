using UnityEngine;
using UnityEngine.AI;

public class TrackState : IEnemyState
{
    // 거리 안에 있다면 플레이어 방향으로 걸어감
    private EnemyBase _enemy;

    private Vector3 _lastPosition;
    private float _stuckTimer = 0f;
    // 1.5초동안 1.5f 이하로 이동할 시 장애물이 있는 것으로 판단하게 하는 변수들
    private const float STUCK_CHECK_INTERVAL = 1f;
    private const float MIN_MOVE_DISTANCE = 1f;

    // 탐지도중 가다 벽에 부딪혔을 때, 임시로 다른 위치로 갔다가 다시 플레이어에게 돌아가게 하는 변수
    private bool _isAvoiding = false;
    private float _avoidTimer = 0f;
    private const float AVOID_DURATION = 2f;
    public TrackState(EnemyBase enemy) { _enemy = enemy; }

    public void EnterState()
    {
        _enemy.Anim.SetBool("isRun", false);
        _enemy.Nav.speed = _enemy.WalkSpeed;

        // 추적 중에는 시야 확보를 위해 플레이어에게 충분히 접근해야 하므로 정지 거리를 작게 설정
        if (_enemy.Nav.isOnNavMesh)
        {
            _enemy.Nav.stoppingDistance = 0.5f;
            _enemy.Nav.isStopped = false; // Chase에서 멈춤이 걸린 채 넘어왔을 수 있으므로 해제
        }

        _isAvoiding = false;
        _stuckTimer = 0f;
        _lastPosition = _enemy.transform.position;
    }

    public void UpdateState()
    {
        // 플레이어를 놓치면 Normal로 복귀
        if (_enemy.TargetPlayer == null)
        {
            _enemy.StateContext.TransitionTo(_enemy.StateContext.NormalState);
            return;
        }

        // 시야가 확보되면 Chase로 전환
        if (_enemy.HasLineOfSight)
        {
            _enemy.StateContext.TransitionTo(_enemy.StateContext.ChaseState);
            return;
        }

        if (_isAvoiding)
        {
            _avoidTimer -= Time.deltaTime;
            if (_avoidTimer <= 0f)
            {
                // 우회 시간이 끝나면 다시 플레이어 추적 재개
                _isAvoiding = false;
                _stuckTimer = 0f;
                _lastPosition = _enemy.transform.position;
            }
            return;
        }

        // 시야는 없지만 탐지 범위 안에 있으므로 플레이어 위치로 이동
        _enemy.Nav.SetDestination(_enemy.TargetPlayer.transform.position);

        _stuckTimer += Time.deltaTime;
        if (_stuckTimer >= STUCK_CHECK_INTERVAL)
        {
            float moveDistance = Vector3.Distance(_enemy.transform.position, _lastPosition);

            if (moveDistance <= MIN_MOVE_DISTANCE)
            {
                Debug.Log("Track: 문(장애물)에 막힘! 잠시 우회합니다.");
                _isAvoiding = true;
                _avoidTimer = AVOID_DURATION;
                SetAvoidDestination(); // 근처 다른 곳으로 피하기
            }

            _stuckTimer = 0f;
            _lastPosition = _enemy.transform.position;
        }
    }

    public void ExitState() { }

    private void SetAvoidDestination()
    {
        // 현재 위치 반경 10~20 근처 랜덤한 곳으로 위치를 찍어 비켜서게 만듭니다.
        Vector2 randomCircle = Random.insideUnitCircle.normalized;
        float randomDist = Random.Range(10f, 20f);
        Vector3 randomDirection = new Vector3(randomCircle.x, 0, randomCircle.y) * randomDist;
        Vector3 targetPos = _enemy.transform.position + randomDirection;

        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            _enemy.Nav.SetDestination(hit.position);
        }
    }
}