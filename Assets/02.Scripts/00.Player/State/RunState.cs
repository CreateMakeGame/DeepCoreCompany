using UnityEngine;

public class RunState : IState
{
    private PlayerStateMachine stateMachine;
    public RunState(PlayerStateMachine sm) => stateMachine = sm;

    public void Enter()
    {
        // 뛰기 상태 진입 시 이동 속도를 8로 상승 (원하는 수치로 조절)
        stateMachine.Mover.SetMoveSpeed(stateMachine.Data.runSpeed);

        stateMachine.Animator.SetFloat(stateMachine.AnimationData.SpeedParameterHash, 1.0f);
    }

    public void Update()
    {
        //stateMachine.Animator.SetFloat(stateMachine.AnimationData.SpeedParameterHash, 1.0f, 0.1f, Time.deltaTime);

        // 멈추면 대기 상태로 전환
        if (stateMachine.playerController.MoveDirection.sqrMagnitude <= 0.01f)
        {
            stateMachine.ChangeState(stateMachine.Idle);
            return;
        }

        if(!stateMachine.playerController.IsRunPressed)
        {
            stateMachine.ChangeState(stateMachine.Walk);
            return;
        }

        // 점프 입력
        if (stateMachine.playerController.IsJumpPressed && stateMachine.Mover.IsGrounded())
        {
            stateMachine.ChangeState(stateMachine.Jump);
            return;
        }

        stateMachine.Mover.Move(stateMachine.playerController.MoveDirection);

    }

    public void Exit() { }
}