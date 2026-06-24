public class EnemyStateContext
{
    // 몬스터의 각 상태 인스턴스 (한 번만 생성해서 재사용)
    public IEnemyState NormalState;
    public IEnemyState TrackState;
    public IEnemyState ChaseState;
    public IEnemyState AttackState;


    private IEnemyState _currentState;
    public IEnemyState CurrentState => _currentState;

    public EnemyStateContext(EnemyBase enemy)
    {
        NormalState = new NormalState(enemy);
        TrackState = new TrackState(enemy);
        ChaseState = new ChaseState(enemy);
        AttackState = new AttackState(enemy);
    }

    // 초기 상태 설정
    public void Initialize(IEnemyState startingState)
    {
        _currentState = startingState;
        _currentState.EnterState();
    }

    // 상태 전환
    public void TransitionTo(IEnemyState nextState)
    {
        if (_currentState == nextState) return;

        _currentState?.ExitState();
        _currentState = nextState;
        _currentState?.EnterState();
    }

    // 현재 상태의 로직 실행
    public void Update()
    {
        _currentState?.UpdateState();
    }
}