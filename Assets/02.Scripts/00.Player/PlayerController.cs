using System;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    [Header("Input Channel")]
    [SerializeField] private InputReader inputReader;

    [Header("References")]
    [SerializeField] private Transform cameraTransform; // Main Camera 혹은 Vcam의 Transform

    #region Properties
    public Vector2 MoveDirection { get; private set; }
    public bool IsRunPressed { get; private set; }
    public bool IsJumpPressed { get; private set; }
    public bool IsDigPressed { get; private set; }
    public bool IsInteractPressed { get; private set; }     // 상호작용 버튼 키

    public Transform CameraTransform => cameraTransform;    // 카메라 참조
    #endregion
    private void OnEnable()
    {
        if (inputReader == null) return;

        // InputReader의 이벤트 구독
        inputReader.MoveEvent += OnMove;
        inputReader.JumpEvent += OnJump;
        inputReader.RunEvent += OnRun;
        inputReader.DigEvent += OnDig;
        inputReader.InteractEvent += OnInteract;
    }

    private void OnDisable()
    {
        if (inputReader == null) return;

        // 구독 해제 (메모리 누수 방지)
        inputReader.MoveEvent -= OnMove;
        inputReader.JumpEvent -= OnJump;
        inputReader.RunEvent -= OnRun;
        inputReader.DigEvent -= OnDig;
        inputReader.InteractEvent -= OnInteract;
    }

    private void LateUpdate() 
    {
        AlignPlayerWithCamera();
    }

    private void AlignPlayerWithCamera()
    {
        // Cursor.lockState가 Locked 상태가 아니면 UI가 열려있는 것이므로 회전을 무조건 멈춥니다.
        if (Cursor.lockState != CursorLockMode.Locked || Cursor.visible || cameraTransform == null)
            return;

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

    public void SetInputActive(bool isActive)
    {
        if (inputReader == null) return;

        if (isActive)
        {
            inputReader.EnablePlayerInput();
        }
        else
        {
            inputReader.DisableAllInput();

            // 입력 상태 초기화
            MoveDirection = Vector2.zero;
            IsRunPressed = false;
            IsJumpPressed = false;
            IsDigPressed = false;
            IsInteractPressed = false;
        }
    }

    #region 이벤트 구독
    private void OnMove(Vector2 dir) => MoveDirection = dir;
    private void OnJump(bool isPressed) => IsJumpPressed = isPressed;
    private void OnRun(bool isPressed) => IsRunPressed = isPressed;
    private void OnDig(bool isPressed) => IsDigPressed = isPressed;
    private void OnInteract(bool isPressed) => IsInteractPressed = isPressed;
    #endregion

}