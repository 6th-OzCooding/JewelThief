using UnityEngine;

public class TrackState : IEnemyState
{
    // 거리 안에 있다면 플레이어 방향으로 걸어감
    private EnemyBase _enemy;

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

        // 시야는 없지만 탐지 범위 안에 있으므로 플레이어 위치로 이동
        _enemy.Nav.SetDestination(_enemy.TargetPlayer.transform.position);
    }

    public void ExitState() { }
}