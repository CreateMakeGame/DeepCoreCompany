using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    private Player_Actions controls;
    private Vector2 moveInput;

    #region Properties
    public Vector2 MoveDirection => moveInput;
    public bool IsRunPressed { get; private set; }
    public bool IsJumpPressed { get; private set; }
    #endregion

    private void Awake()
    {
        //  생성된 C# 클래스의 인스턴스를 만듭니다.
        controls = new Player_Actions();

        OnMove();
        OnJump();
    }

    private void OnEnable() => controls.Player.Enable(); // 활성화
    private void OnDisable() => controls.Player.Disable(); // 비활성화

    private void Update() { }

    #region 이벤트 구독
    private void OnMove()
    {
        //  WASD (Vector2) 입력이 발생할 때마다 변수에 저장합니다.
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    }
    private void OnJump()
    {
        // 점프 버튼이 눌렸을 때 로직을 실행합니다.
        controls.Player.Jump.started += ctx => IsJumpPressed = true;
        controls.Player.Jump.canceled += ctx => IsJumpPressed = false;
    }
    private void OnRun()
    {
        // 달리기 버튼이 눌렸을 때 로직을 실행합니다.
        controls.Player.Sprint.started += ctx => IsRunPressed = true;
        controls.Player.Sprint.canceled += ctx => IsRunPressed = false;
    }
    #endregion
}