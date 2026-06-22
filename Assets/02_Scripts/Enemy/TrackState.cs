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
    }

    public void UpdateState()
    {
        if (_enemy.TargetPlayer != null)
            _enemy.Nav.SetDestination(_enemy.TargetPlayer.transform.position);
    }

    public void ExitState() { }
}