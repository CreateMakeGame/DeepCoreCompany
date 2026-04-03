using UnityEngine;

public class WalkState : IState
{
    private PlayerStateMachine stateMachine;
    public WalkState(PlayerStateMachine sm) => stateMachine = sm;

    public void Enter()
    {
        stateMachine.Mover.SetMoveSpeed(stateMachine.Data.baseSpeed);
    }
    public void Update()
    {
        stateMachine.Mover.Move(stateMachine.playerController.MoveDirection);

        // Float 파라미터 조절: "Speed" 값을 0.5(걷기)로 부드럽게(0.1초 동안) 변경
        stateMachine.Animator.SetFloat(stateMachine.AnimationData.SpeedParameterHash, 0.5f, 0.1f, Time.deltaTime);

        if (stateMachine.playerController.IsJumpPressed && stateMachine.Mover.IsGrounded())
        {
            stateMachine.ChangeState(stateMachine.Jump);
            return;
        }

        if(stateMachine.playerController.IsRunPressed)
        {
            stateMachine.ChangeState(stateMachine.Run);
            return;
        }

        if (stateMachine.playerController.MoveDirection.sqrMagnitude <= 0.01f)
        {
            stateMachine.ChangeState(stateMachine.Idle);
        }
    }

    public void Exit()
    {
    }
}