using UnityEngine;
using UnityEngine.AI;


public class NormalState : IEnemyState
{
    private EnemyBase _enemy;

    private float _minPatrolDistance = 3f; // 최소거리
    private float _maxPatrolDistance = 90f; // 최대거리
    public NormalState(EnemyBase enemy) { _enemy = enemy; }

    public void EnterState()
    {
        _enemy.Anim.SetBool("isRun", false);
        _enemy.Nav.speed = _enemy.WalkSpeed;

        // 탐지 범위를 벗어났을 때 이전 목적지(플레이어 위치)를 지워줍니다.
        if (_enemy.Nav.hasPath) _enemy.Nav.ResetPath();

        SetRandomDestination();
    }

    public void UpdateState()
    {
        if (!_enemy.Nav.pathPending)
        {
            // 목적지까지 남은 거리가 Agent의 정지 거리 근처라면 (Nav.stoppingDistance에 여유분 0.5f를 더해 줘서 멈칫거리지 않게 함)
            if (_enemy.Nav.remainingDistance <= _enemy.Nav.stoppingDistance + 0.5f)
            {
                // 도착하자마자 멈추지 않고 바로 다음 랜덤 위치를 찍어서 이동!
                SetRandomDestination();
            }
        }
    }

    public void ExitState()
    {
        // 플레이어를 발견해서(Track, Chase) 이 상태를 나갈 때는, 찍어뒀던 정찰 목적지를 초기화해서 플레이어 쪽으로 잘 갈 수 있게 해줌
        if (_enemy.Nav.hasPath)
        {
            _enemy.Nav.ResetPath();
        }
    }

    private void SetRandomDestination()
    {
        if (_enemy.Nav == null || !_enemy.Nav.isOnNavMesh)
            return;

        // 방향을 랜덤으로 찾고
        Vector2 randomCircle = Random.insideUnitCircle.normalized;
        // 최소거리와 최대 거리 내에서 위치 찍기
        float randomDist = Random.Range(_minPatrolDistance, _maxPatrolDistance);
        // 구한 방향과 거리를 3차원 좌표로 설정 (y는 0으로 고정시킬 것 공중에 있으면 안되므로)
        Vector3 randomDirection = new Vector3(randomCircle.x, 0, randomCircle.y) * randomDist;
        // 현재 위치에서 목표 위치 계산
        Vector3 targetPos = _enemy.transform.position + randomDirection;

        NavMeshHit hit;
        // 만약 네비 메시 바닥이 아닐 경우 그 근방에서 네비메쉬 바닥을 찾고 있다면 목적지로 설정
        if (NavMesh.SamplePosition(targetPos, out hit, 5f, NavMesh.AllAreas))
        {
            _enemy.Nav.SetDestination(hit.position);
        }
        else
        {
            // 맵 바깥이나 이상한 곳을 찍은 경우 다시 찾도록 설정
            SetRandomDestination();
        }
    }
}