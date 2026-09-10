using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerData data; // SO 파일 할당
    public PlayerData Data => data;

    public PlayerAnimationData AnimationData;
    public PlayerController playerController { get; private set; }
    public PlayerMover Mover { get; private set; }
    public PlayerStatus Status { get; private set; }
    public Animator Animator { get; private set; }

    private IState currentState;
    public IState CurrentState => currentState;     // 현재 상태를 외부에서 읽기 전용으로 접근 가능
    private float lastDigTime = -10f;               // 초기값을 충분히 과거로 설정
    public Vector3 CurrerntDigTarget { get; set; } // 현재 굴착 목표 위치 (상태 간 공유용)

    #region States
    public IdleState Idle { get; private set; }
    public WalkState Walk { get; private set; }
    public RunState Run { get; private set; }
    public JumpState Jump { get; private set; }
    public DigState Dig { get; private set; }
    #endregion
    public void Awake()
    {
        playerController = GetComponent<PlayerController>();
        Mover = GetComponent<PlayerMover>();
        Status = GetComponent<PlayerStatus>();
        Animator = GetComponentInChildren<Animator>();
        AnimationData.Initialize();
        InitializeStates();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangeState(Idle);
    }

    // Update is called once per frame
    void Update()
    {
        currentState?.Update();

        if (CanDig())
        {
            lastDigTime = Time.time; // 굴착 시작 시점 기록
            ChangeState(Dig);
        }
    }
    public void ChangeState(IState newState)
    {
        if (currentState == newState) return; // 같은 상태로의 전환 방지

        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }


    private void InitializeStates()
    {
        Idle = new IdleState(this);
        Walk = new WalkState(this);
        Run = new RunState(this);
        Jump = new JumpState(this);
        Dig = new DigState(this);
    }

    private bool CanDig()
    {
        // 가장 가벼운 부울 조건(키 입력 여부)부터 먼저 평가하여 불필요한 연산 차단
        if (!playerController.IsDigPressed) return false;
        if (currentState == Dig) return false;
        if (Time.time < lastDigTime + data.digCooldown) return false;
        if (Status != null && !Status.HasStamina(data.digStaminaCost)) return false;

        if (playerController.CameraTransform != null)
        {
            Ray ray = new Ray(playerController.CameraTransform.position, playerController.CameraTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, data.digRange, data.groundLayer))
            {
                CurrerntDigTarget = hit.point;
                return true;
            }
        }
        return false;
    }

    public bool CanRun()
    {
        if (!playerController.IsRunPressed) return false;
        // 이동 하려는 방향 입력이 없으면 false
        if(playerController.MoveDirection.sqrMagnitude <= 0.01f) return false;
        // 최소 요구 스태미나(예: 1.0f)가 없으면 달리기 불가
        if (Status != null && !Status.HasStamina(1.0f)) return false;

        return true;
    }

    // 공통 복귀 로직 (이동 중이면 Walk, 아니면 Idle)
    public void ReturnToLocomotion()
    {
        if (playerController.MoveDirection != Vector2.zero)
            ChangeState(Walk);
        else
            ChangeState(Idle);
    }
}
