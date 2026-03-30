using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public PlayerController InputReader { get; private set; }
    public PlayerMover Mover { get; private set; }

    private IState currentState;

    public IdleState Idle { get; private set; }
    public MoveState Move { get; private set; }
    public JumpState Jump { get; private set; }
    public void Awake()
    {
        InputReader = GetComponent<PlayerController>();
        Mover = GetComponent<PlayerMover>();

        Idle = new IdleState(this);
        Move = new MoveState(this);
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
