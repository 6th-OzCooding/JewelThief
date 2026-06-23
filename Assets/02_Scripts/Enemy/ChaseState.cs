using UnityEngine;

public class ChaseState : IEnemyState
{
    // 시야각에 들어왔다면 (뛰기)
    private EnemyBase _enemy;

    public ChaseState(EnemyBase enemy) { _enemy = enemy; }

    public void EnterState()
    {
        _enemy.Anim.SetBool("isRun", true);
        _enemy.Nav.speed = _enemy.RunSpeed;
    }

    public void UpdateState()
    {
        if (_enemy.TargetPlayer != null)
            _enemy.Nav.SetDestination(_enemy.TargetPlayer.transform.position);
    }

    public void ExitState() { }
}