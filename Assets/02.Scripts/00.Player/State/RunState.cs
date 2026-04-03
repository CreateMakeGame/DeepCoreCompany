using UnityEngine;

public class RunState : IState
{
    private PlayerStateMachine stateMachine;
    public RunState(PlayerStateMachine sm) => stateMachine = sm;

    public void Enter()
    {
        // 뛰기 상태 진입 시 이동 속도를 8로 상승 (원하는 수치로 조절)
        stateMachine.Mover.SetMoveSpeed(stateMachine.Data.runSpeed);
    }

    public void Update()
    {
        stateMachine.Mover.Move(stateMachine.playerController.MoveDirection);

        stateMachine.Animator.SetFloat(stateMachine.AnimationData.SpeedParameterHash, 1.0f, 0.1f, Time.deltaTime);

        if (stateMachine.playerController.IsJumpPressed && stateMachine.Mover.IsGrounded())
        {
            stateMachine.ChangeState(stateMachine.Jump);
            return;
        }

        // 멈추면 대기 상태로 전환
        if (stateMachine.playerController.MoveDirection.sqrMagnitude <= 0.01f)
        {
            stateMachine.ChangeState(stateMachine.Idle);
            return;
        }

        // [추가된 부분] 쉬프트 키를 떼면 다시 Move(걷기) 상태로 전환
        if (!stateMachine.playerController.IsRunPressed)
        {
            stateMachine.ChangeState(stateMachine.Walk); // 주의: Walk 상태 이름이 맞는지 확인하세요 (MoveState 라면 stateMachine.Move 로 변경)
        }
    }

    public void Exit()
    {
    }
}