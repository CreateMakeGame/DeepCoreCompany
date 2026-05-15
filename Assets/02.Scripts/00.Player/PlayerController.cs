using System;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    private Player_Actions controls;
    private Vector2 moveInput;

    [Header("References")]
    [SerializeField] private Transform cameraTransform; // Main Camera 혹은 Vcam의 Transform

    #region Properties
    public Vector2 MoveDirection => moveInput;
    public bool IsRunPressed { get; private set; }
    public bool IsJumpPressed { get; private set; }
    public bool IsDigPressed { get; private set; }

    public Transform CameraTransform => cameraTransform;    // 카메라 참조
    #endregion

    private void Awake()
    {
        //  생성된 C# 클래스의 인스턴스를 만듭니다.
        controls = new Player_Actions();

        OnMove();
        OnJump();
        OnRun();
        OnDig();
    }
   

    private void OnEnable() => controls.Player.Enable(); // 활성화
    private void OnDisable() => controls.Player.Disable(); // 비활성화

    private void LateUpdate() 
    {
        AlignPlayerWithCamera();
    }

    private void AlignPlayerWithCamera()
    {
        if (cameraTransform == null) return;

        // 카메라의 전방(Forward) 방향에서 수평 평면(X, Z) 벡터만 추출합니다.
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0; // 수직 방향은 무시

        if (cameraForward.sqrMagnitude > 0.001f)
        {
            // 카메라가 바라보는 방향을 향해 몸통의 회전값을 설정
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            transform.rotation = targetRotation;
        }
    }

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
        controls.Player.Run.started += ctx => IsRunPressed = true;
        controls.Player.Run.canceled += ctx => IsRunPressed = false;
    }

    private void  OnDig()
    {
        // 파기 버튼이 눌렸을 때 로직을 실행합니다.
        controls.Player.Dig.started += ctx =>  IsDigPressed = true;
        controls.Player.Dig.canceled += ctx =>  IsDigPressed = false;
    }
    #endregion

}