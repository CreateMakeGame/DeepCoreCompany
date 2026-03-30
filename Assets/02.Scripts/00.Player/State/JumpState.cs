using UnityEngine;

public class JumpState : IState
{
    private PlayerStateMachine stateMachine;
    public JumpState(PlayerStateMachine sm) => stateMachine = sm;


    public void Enter()
    {
        stateMachine.Mover.Jump();
    }

    public void Update()
    {
        stateMachine.Mover.Move(stateMachine.InputReader.MoveDirection);

        // y축 속도가 0보다 작을 때(내려오는 중) 땅에 닿으면 상태 전환
        if (stateMachine.Mover.IsGrounded() && stateMachine.Mover.GetVerticalVelocity() < 0)
        {
            // sqrMagnitude로 비교하여 입력이 있으면 Move 상태, 없으면 Idle 상태로 전환
            if (stateMachine.InputReader.MoveDirection.sqrMagnitude > 0.01f)
                stateMachine.ChangeState(stateMachine.Move);
            else
                stateMachine.ChangeState(stateMachine.Idle);
        }
    }

    public void Exit() { }
}
