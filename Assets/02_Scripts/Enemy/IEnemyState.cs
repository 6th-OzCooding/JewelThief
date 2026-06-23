public interface IEnemyState
{
    // 상태 초기 설정
    void EnterState();

    // 상태가 진행 중일 때, 실행되는 로직
    void UpdateState();

    // 상태가 종료될 때, 실행되는 작업
    void ExitState();
}