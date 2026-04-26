using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerData data; // SO 파일 할당
    public PlayerData Data => data;

    public PlayerAnimationData AnimationData;
    public PlayerController playerController { get; private set; }
    public PlayerMover Mover { get; private set; }
    public Animator Animator { get; private set; }

    private IState currentState;
    private float lastDigTime = -10f; // 초기값을 충분히 과거로 설정

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
        return currentState != Dig &&
            playerController.IsDigPressed && Time.time >= lastDigTime + data.digCooldown;
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
