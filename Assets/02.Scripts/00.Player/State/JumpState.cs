using UnityEngine;

public class JumpState : IState
{
    private PlayerStateMachine stateMachine;
    public JumpState(PlayerStateMachine sm) => stateMachine = sm;


    public void Enter()
    {
        stateMachine.Mover.Jump();
        stateMachine.Animator.SetTrigger(stateMachine.AnimationData.JumpParameterHash);
    }

    public void Update()
    {
        stateMachine.Mover.Move(stateMachine.playerController.MoveDirection);

        // y축 속도가 0보다 작을 때(내려오는 중) 땅에 닿으면 상태 전환
        if (stateMachine.Mover.IsGrounded() && stateMachine.Mover.GetVerticalVelocity() < 0)
        {
            // sqrMagnitude로 비교하여 입력이 있으면 Move 상태, 없으면 Idle 상태로 전환
            if (stateMachine.playerController.MoveDirection.sqrMagnitude > 0.01f)
            {
                if (stateMachine.CanRun())  // 달리기 스테미나 잔여량 체크하기
                    stateMachine.ChangeState(stateMachine.Run);
                else
                    stateMachine.ChangeState(stateMachine.Walk);
            }
            else
            {
                stateMachine.ChangeState(stateMachine.Idle);
            }   
        }
    }

    public void Exit() { }
}
