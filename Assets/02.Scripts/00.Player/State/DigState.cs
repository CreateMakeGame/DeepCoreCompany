using UnityEngine;
using UnityEngine.UIElements;

public class DigState : IState
{
    private PlayerStateMachine stateMachine;
    private float animationEndTime;

    private VoxelTerrain terrain;
    private bool hasDug;    // 한 번만 파게 하기 위한 플래그
    private float digTime;  // 땅이 파이는 시점 기록

    public DigState(PlayerStateMachine sm) => stateMachine = sm;

    public void Enter()
    {
        // 상체 레이어(Action Layer)의 애니메이션 재생
        stateMachine.Animator.SetTrigger(stateMachine.AnimationData.DigParameterHash);

        // 속도 배율 적용
        stateMachine.Animator.speed = stateMachine.Data.digSpeedMultiplier;

        // 실제 재생 시간 계산 (기본 시간 / 배율)
        float currentDuration = stateMachine.Data.baseDigDuration / stateMachine.Data.digSpeedMultiplier;
        animationEndTime = Time.time + currentDuration;

        terrain = Object.FindAnyObjectByType<VoxelTerrain>();
        hasDug = false;
        digTime = Time.time + (currentDuration * 0.5f); // 애니메이션 절반쯤에서 땅이 파이도록 설정
    }

    public void Update()
    {
        stateMachine.Mover.SetMoveSpeed(stateMachine.Data.baseSpeed);
        // 하체 이동은 계속 허용 (상하체 분리 마스크 덕분)
        stateMachine.Mover.Move(stateMachine.playerController.MoveDirection);

        if (!hasDug && Time.time >= digTime)
        {
            hasDug = true;
            if (terrain != null)
            {
                Vector3 digPos = stateMachine.transform.position + stateMachine.transform.forward * 1.0f; // 플레이어 앞쪽 1미터 지점
                terrain.Dig(digPos, 1.0f); // 반지름 1로 파기
            }
        }


        // 애니메이션 시간이 다 되면 복귀
        if (Time.time >= animationEndTime)
        {
            stateMachine.ReturnToLocomotion();
        }
    }

    public void Exit()
    {
        // 다른 상태(Idle/Walk)에 영향을 주지 않도록 속도 원복
        stateMachine.Animator.speed = 1.0f;
    }
}