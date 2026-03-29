public interface IState
{
    void Enter();   // 상태에 진입할 때 1번 실행
    void Update();  // 상태 유지 중 매 프레임 실행
    void Exit();    // 상태를 빠져나갈 때 1번 실행
}