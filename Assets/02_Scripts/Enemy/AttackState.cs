using UnityEngine;

// 공격 사거리에 들어와서 플레이어를 공격하는 상태
public class AttackState : IEnemyState
{
    private EnemyBase _enemy;

    public AttackState(EnemyBase enemy) { _enemy = enemy; }

    public void EnterState()
    {
        // 공격해야 하는 애니메이션으로 뛰는 애니메이션을 멈춘다.
        _enemy.Anim.SetBool("isRun", false);

        // 공격 순간 속도를 0으로 만들고 NavMesh를 꺼서 플레이어가 밀리는 현상 해결
        _enemy.Nav.velocity = Vector3.zero;
        
        if (_enemy.Nav.isOnNavMesh)
        {
            _enemy.Nav.isStopped = true;
        }
        
        if (_enemy.Nav.hasPath)
        {
            _enemy.Nav.ResetPath();
        }

        // 공격 애니메이션 시작
        _enemy.AttackRoutine().Forget();
    }

    public void UpdateState()
    {
        // 공격할 때에 플레이어를 바라보며 공격하도록 설정
        if (_enemy.DirToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_enemy.DirToTarget);
            // NavMesh와 충돌 없이 부드럽게 회전하도록 _rb 대신 transform 적용
            _enemy.transform.rotation = Quaternion.Slerp(_enemy.transform.rotation, targetRotation, Time.fixedDeltaTime * 10.0f);
        }
    }

    public void ExitState()
    {
        // 공격 상태에서 빠져나갈 때는 다시 다음 상태가 움직일 수 있도록 정지를 풀어줍니다.
        if (_enemy.Nav.isOnNavMesh)
        {
            _enemy.Nav.isStopped = false;
        }
    }
}