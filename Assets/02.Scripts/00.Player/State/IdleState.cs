using UnityEngine;

public class IdleState : IState
{
    private PlayerStateMachine stateMachine;

    public IdleState(PlayerStateMachine sm) => stateMachine = sm;

    public void Enter() { /* 가만히 서 있는 애니메이션 재생 */ }

    public void Update()
    {
        if (stateMachine.InputReader.IsJumpPressed && stateMachine.Mover.IsGrounded())
        {
            stateMachine.ChangeState(stateMachine.Jump);
            return;
        }

        // 입력이 들어오면 이동 상태로 전환
        if (stateMachine.InputReader.MoveDirection.sqrMagnitude > 0.01f)
        {
            stateMachine.ChangeState(stateMachine.Move);
        }
    }

    public void Exit() { }
}