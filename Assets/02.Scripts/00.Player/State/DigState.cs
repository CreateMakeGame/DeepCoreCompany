using UnityEngine;

public class DigState : IState
{
    private PlayerStateMachine stateMachine;
    private float animationEndTime;

    private VoxelTerrain terrain;
    private bool hasDug;    // 한 번만 파게 하기 위한 플래그
    private float digTime;  // 땅이 파이는 시점 기록

    private LayerMask targetLayer = LayerMask.GetMask("Plant");
    private float digRadius = 2f; // 굴착 반지름
    private float digStrength = 3f; // 깎아내는 힘을 약간 강화

    public DigState(PlayerStateMachine sm)
    {
        stateMachine = sm;

        // 씬 검색 비용 절감을 위해 생성자 시점에 미리 찾아서 캐싱
        terrain = Object.FindAnyObjectByType<VoxelTerrain>();

    }
    public void Enter()
    {
        // 씬에 지형이 나중에 생길 경우를 대비해 null 체크 후 1회 추가 갱신
        if (terrain == null)
            terrain = Object.FindAnyObjectByType<VoxelTerrain>();

        var status = stateMachine.Status;
        if (status != null)
        {
            if (!status.UseStamina(stateMachine.Data.digStaminaCost))
            {
                return;
            }
        }

        // 상체 레이어(Action Layer)의 애니메이션 재생
        stateMachine.Animator.SetTrigger(stateMachine.AnimationData.DigParameterHash);
        // 속도 배율 적용
        stateMachine.Animator.speed = stateMachine.Data.digSpeedMultiplier;

        // 실제 재생 시간 계산 (기본 시간 / 배율)
        float currentDuration = stateMachine.Data.baseDigDuration / stateMachine.Data.digSpeedMultiplier;
        animationEndTime = Time.time + currentDuration;

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
                Vector3 digPos = stateMachine.CurrerntDigTarget; // 굴착 목표 위치 (상태 간 공유용)
                terrain.Dig(digPos, digRadius, digStrength); // 반지름 1로 파기

                // 식물/오브젝트 파괴
                Collider[] hitColliders = Physics.OverlapSphere(digPos, digRadius, targetLayer);
                for(int i = 0; i < hitColliders.Length; i++)
                {
                    Collider col = hitColliders[i];
                    // VoxelTerrain 지형 메쉬 자체는 파괴되면 안 되므로 스킵
                    if (col.GetComponent<VoxelTerrain>() != null) continue;
                    // 풀/꽃/바위 등 오브젝트 파괴
                    Object.Destroy(col.gameObject);
                }
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