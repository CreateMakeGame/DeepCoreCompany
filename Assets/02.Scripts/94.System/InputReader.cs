using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "ScriptableObjects/Input/InputReader")]
public class InputReader : ScriptableObject, Player_Actions.IPlayerActions
{
    // ==========================================
    // C# UnityAction 이벤트 정의 (다른 스크립트들이 구독할 이벤트)
    // ==========================================
    // Vector2 축 입력 이벤트
    public event UnityAction<Vector2> MoveEvent = delegate { };
    public event UnityAction<Vector2> LookEvent = delegate { }; // 시선/마우스 이동
    public event UnityAction<float> QuickSlotScrollEvent = delegate { }; // 퀵슬롯 마우스 휠 스크롤

    // Started / Canceled 상태 전달용 bool 이벤트 정의
    public event UnityAction<bool> JumpEvent = delegate { };
    public event UnityAction<bool> RunEvent = delegate { };
    public event UnityAction<bool> DigEvent = delegate { };
    public event UnityAction<bool> InteractEvent = delegate { };

    // 버튼 단발성(Trigger) 이벤트
    public event UnityAction OptionEvent = delegate { };
    public event UnityAction InventoryEvent = delegate { };
    public event UnityAction DropEvent = delegate { };
    public event UnityAction<int> QuickSlotEvent = delegate { };    // 숫자키 1~9 등 퀵슬롯 지정

    private Player_Actions inputActions;

    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new Player_Actions();
            // Player_Actions.IPlayerActions 인터페이스의 콜백 대상을 이 클래스로 지정
            inputActions.Player.SetCallbacks(this);
        }
        EnablePlayerInput();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }
    /// <summary>
    /// 입력 활성화 / 비활성화 제어 함수
    /// </summary>
    public void EnablePlayerInput()
    {
        inputActions.Player.Enable();
    }
    public void DisableAllInput()
    {
        inputActions.Player.Disable();
    }


    /// <summary>
    /// Player_Actions.IPlayerActions 인터페이스 구현부
    /// </summary>
    /// <param name="context"></param>
    /// <exception cref="System.NotImplementedException"></exception>
    public void OnDig(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
            DigEvent.Invoke(true);
        else if (context.phase == InputActionPhase.Canceled)
            DigEvent.Invoke(false);
    }

    public void OnDrop(InputAction.CallbackContext context)
    {
        // 아이템 버리기 버튼
        if(context.phase == InputActionPhase.Performed)
            DropEvent.Invoke();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
            InteractEvent.Invoke(true);
        else if (context.phase == InputActionPhase.Canceled)
            InteractEvent.Invoke(false);
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
            InventoryEvent.Invoke();
        
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started)
        {
            JumpEvent.Invoke(true); // Jump 버튼 눌림
        }
        else if(context.phase == InputActionPhase.Canceled)
        {
            JumpEvent.Invoke(false); // Jump 버튼 뗌
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        // Look 액션이 비활성화(Disable)되어 있으면 회전 이벤트를 보내지 않음
        if (inputActions != null && !inputActions.Player.Look.enabled) return;
        // 마우스 이동 입력값 전달
        LookEvent.Invoke(context.ReadValue<Vector2>());
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // Move 액션이 비활성화되어 있으면 Vector2.zero를 전달하여 이동 정지
        if (inputActions != null && !inputActions.Player.Move.enabled)
        {
            MoveEvent.Invoke(Vector2.zero); // 이동 입력 비활성화 시 이동값을 0으로 전달
            return;
        }

        // UnityAction<Vector2> MoveEvent를 통해 구독자들에게 이동 입력값 전달
        MoveEvent.Invoke(context.ReadValue<Vector2>());
    }

    public void OnOption(InputAction.CallbackContext context)
    {
        // ESC 키는 떼어질 때가 아닌 눌렸을 때(Performed) 즉시 작동하도록 설정
        if (context.phase == InputActionPhase.Performed)
            OptionEvent.Invoke();
    }

    public void OnQuickSlot(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            // 숫자키 1~9를 눌렀을 때 QuickSlotEvent를 발생시키고, 인덱스를 전달함
            int slotIndex = (int)context.ReadValue<float>();
            QuickSlotEvent.Invoke(slotIndex);
        }
    }

    public void OnQuickSlotScroll(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
        {
            Vector2 scrollVector = context.ReadValue<Vector2>();
            // (위로 올리면 양수(+), 아래로 내리면 음수(-))
            QuickSlotScrollEvent.Invoke(scrollVector.y);
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            RunEvent.Invoke(true); // Run 버튼 눌림
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            RunEvent.Invoke(false); // Run 버튼 뗌
        }
    }
    /// <summary>
    /// UI가 열렸을 때 이동 및 시선 회전 등의 플레이어 조작 입력만 비활성화
    /// </summary>
    public void DisablePlayerControl()
    {
        inputActions.Player.Move.Disable();
        inputActions.Player.Look.Disable();
        inputActions.Player.Jump.Disable();
        inputActions.Player.Run.Disable();
        inputActions.Player.Dig.Disable();
    }

    /// <summary>
    /// UI가 열렸을 때 이동 및 시선 회전 등의 플레이어 조작 입력만 활성화
    /// </summary>
    public void EnablePlayerControl()
    {
        inputActions.Player.Move.Enable();
        inputActions.Player.Look.Enable();
        inputActions.Player.Jump.Enable();
        inputActions.Player.Run.Enable();
        inputActions.Player.Dig.Enable();
    }
}
