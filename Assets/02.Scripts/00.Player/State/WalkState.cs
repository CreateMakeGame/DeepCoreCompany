using UnityEngine;

public class WalkState : IState
{
    private PlayerStateMachine stateMachine;
    public WalkState(PlayerStateMachine sm) => stateMachine = sm;

    public void Enter()
    {
        stateMachine.Mover.SetMoveSpeed(stateMachine.Data.baseSpeed);
        stateMachine.Animator.SetFloat(stateMachine.AnimationData.SpeedParameterHash, 0.5f);
    }
    public void Update()
    {
        stateMachine.Mover.Move(stateMachine.playerController.MoveDirection);

        //stateMachine.Animator.SetFloat(stateMachine.AnimationData.SpeedParameterHash, 0.5f, 0.1f, Time.deltaTime);

        // Float 파라미터 조절: "Speed" 값을 0.5(걷기)로 부드럽게(0.1초 동안) 변경
        if (stateMachine.playerController.IsJumpPressed && stateMachine.Mover.IsGrounded())
        {
            stateMachine.ChangeState(stateMachine.Jump);
            return;
        }

        // 달리기 상태 전환
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