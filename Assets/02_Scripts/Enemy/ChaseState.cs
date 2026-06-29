using UnityEngine;
using UnityEngine.AI;
public class ChaseState : IEnemyState
{
    // 시야각에 들어왔다면 (뛰기)
    private EnemyBase _enemy;

    private Vector3 _lastPosition;
    private float _stuckTimer = 0f;
    private const float STUCK_CHECK_INTERVAL = 1.5f;
    private const float MIN_MOVE_DISTANCE = 1.5f;

    private bool _isAvoiding = false;
    private float _avoidTimer = 0f;
    private const float AVOID_DURATION = 1.5f;

    public ChaseState(EnemyBase enemy) { _enemy = enemy; }

    public void EnterState()
    {
        _enemy.Anim.SetBool("isRun", true);
        _enemy.Nav.speed = _enemy.RunSpeed;

        // 시야 확보 상태이므로 최소 접근 거리(원거리 적은 멀리서 공격)를 다시 적용
        if (_enemy.Nav.isOnNavMesh)
        {
            _enemy.Nav.stoppingDistance = _enemy.MinApproachDistance;
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

        // 시야를 잃으면(장애물에 가림 등) Track으로 전환해 추적 유지
        if (!_enemy.HasLineOfSight)
        {
            _enemy.StateContext.TransitionTo(_enemy.StateContext.TrackState);
            return;
        }

        if (_isAvoiding)
        {
            _avoidTimer -= Time.deltaTime;
            if (_avoidTimer <= 0f)
            {
                _isAvoiding = false;
                _stuckTimer = 0f;
                _lastPosition = _enemy.transform.position;
            }
            return;
        }

        if (_enemy.TargetPlayer != null)
        {
            _enemy.Nav.SetDestination(_enemy.TargetPlayer.transform.position);
        }

        _stuckTimer += Time.deltaTime;
        if (_stuckTimer >= STUCK_CHECK_INTERVAL)
        {
            float moveDistance = Vector3.Distance(_enemy.transform.position, _lastPosition);

            if (moveDistance <= MIN_MOVE_DISTANCE)
            {
                Debug.Log("Chase: 문(장애물)에 막힘! 잠시 우회합니다.");
                _isAvoiding = true;
                _avoidTimer = AVOID_DURATION;
                SetAvoidDestination();
            }

            _stuckTimer = 0f;
            _lastPosition = _enemy.transform.position;
        }
    }

    public void ExitState() { }

    private void SetAvoidDestination()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized;
        float randomDist = Random.Range(3f, 5f);
        Vector3 randomDirection = new Vector3(randomCircle.x, 0, randomCircle.y) * randomDist;
        Vector3 targetPos = _enemy.transform.position + randomDirection;

        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            _enemy.Nav.SetDestination(hit.position);
        }
    }
}