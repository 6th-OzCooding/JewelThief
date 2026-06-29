public class ChaseState : IEnemyState
{
    // 시야각에 들어왔다면 (뛰기)
    private EnemyBase _enemy;

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

        // Chase→Attack 전환(사거리 진입/멈춤/조준)은 EnemyBase.FixedUpdate에서 처리
        if (_enemy.TargetPlayer != null)
            _enemy.Nav.SetDestination(_enemy.TargetPlayer.transform.position);
    }

    public void ExitState() { }
}