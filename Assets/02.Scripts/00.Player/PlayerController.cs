using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Player_Actions controls;
    private Vector2 moveInput;
    private bool jumpPressed;

    private void Awake()
    {
        // 1. 생성된 C# 클래스의 인스턴스를 만듭니다.
        controls = new Player_Actions();

        // 2. WASD (Vector2) 입력이 발생할 때마다 변수에 저장합니다.
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // 3. 점프 버튼이 눌렸을 때 로직을 실행합니다.
        controls.Player.Jump.started += ctx => OnJump();
    }

    private void OnEnable() => controls.Player.Enable(); // 활성화
    private void OnDisable() => controls.Player.Disable(); // 비활성화

    private void Update()
    {
        // 여기서 moveInput을 이용해 캐릭터를 이동시킵니다.
        Debug.Log($"이동 값: {moveInput}");
    }

    private void OnJump()
    {
        Debug.Log("점프!");
    }

    // FSM에서 이 값들을 참조할 수 있도록 public 프로퍼티를 열어줍니다.
    public Vector2 MoveDirection => moveInput;
}