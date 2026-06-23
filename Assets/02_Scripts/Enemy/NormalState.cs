using UnityEngine;

// 아무것도 아닌 경우 가던 방향으로 걸어감
public class NormalState : IEnemyState
{
    private EnemyBase _enemy;

    public NormalState(EnemyBase enemy) { _enemy = enemy; }

    public void EnterState()
    {
        _enemy.Anim.SetBool("isRun", false);
        _enemy.Nav.speed = _enemy.WalkSpeed;

        // 탐지 범위를 벗어났을 때 이전 목적지(플레이어 위치)를 지워줍니다.
        if (_enemy.Nav.hasPath) _enemy.Nav.ResetPath();
    }

    public void UpdateState()
    {
        _enemy.Nav.Move(_enemy.transform.forward * _enemy.WalkSpeed * Time.fixedDeltaTime);
    }

    public void ExitState() { }
}