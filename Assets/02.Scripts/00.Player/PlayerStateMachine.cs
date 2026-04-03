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


    public IdleState Idle { get; private set; }
    public WalkState Walk { get; private set; }
    public RunState Run { get; private set; }
    public JumpState Jump { get; private set; }

    public void Awake()
    {
        playerController = GetComponent<PlayerController>();
        Mover = GetComponent<PlayerMover>();
        Animator = GetComponentInChildren<Animator>();
        AnimationData.Initialize();

        Idle = new IdleState(this);
        Walk = new WalkState(this);
        Run = new RunState(this);
        Jump = new JumpState(this);

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
    }

    public void ChangeState(IState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }
}
