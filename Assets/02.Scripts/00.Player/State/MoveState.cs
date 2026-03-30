using UnityEngine;

public class MoveState : IState
{
    private PlayerStateMachine stateMachine;
    public MoveState(PlayerStateMachine sm) => stateMachine = sm;

    public void Enter()
    {
        // 이동 애니메이션 재생

    }
    public void Update()
    {
        stateMachine.Mover.Move(stateMachine.InputReader.MoveDirection);

        if (stateMachine.InputReader.IsJumpPressed && stateMachine.Mover.IsGrounded())
        {
            stateMachine.ChangeState(stateMachine.Jump);
            return;
        }

        if (stateMachine.InputReader.MoveDirection.sqrMagnitude <= 0.01f)
        {
            stateMachine.ChangeState(stateMachine.Idle);
        }
    }

    public void Exit() { }
}
