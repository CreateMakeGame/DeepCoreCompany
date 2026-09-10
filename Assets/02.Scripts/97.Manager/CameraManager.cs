using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : Singleton<CameraManager>
{
    [Header("Settings")]
    [SerializeField] private CinemachineInputAxisController axisController; // CinemachineInputAxisController 컴포넌트 참조

    [Range(0.01f, 10.0f)]
    public float horizontalSensitivity = 5f; // 수평 감도
    [Range(0.01f, 10.0f)]
    public float verticalSensitivity = 5f;   // 수직 감도 

    protected override void Awake()
    {
        base.Awake(); // Singleton 초기화
    }

    private void Start()
    {
        BindPlayerCamera();
    }

    /// <summary>
    /// 스폰된 플레이어의 1st Person Vcam에서 CinemachineInputAxisController를 찾아 연결합니다.
    /// </summary>
    public void BindPlayerCamera()
    {
        // 씬 내부(플레이어 프리팹 자식)의 CinemachineInputAxisController 탐색
        axisController = FindAnyObjectByType<CinemachineInputAxisController>();

        if (axisController != null)
        {
            ApplySensitivity();
        }
        else
        {
            Debug.LogWarning("[CameraManager] CinemachineInputAxisController를 찾지 못했습니다.");
        }
    }
    public void RegisterPlayerCamera(CinemachineInputAxisController controller)
    {
        axisController = controller;
        ApplySensitivity();
    }

    /// <summary>
    /// CinemachineInputAxisController의 Reader.Gain 값을 직접 수정하여 감도를 조절합니다.
    /// </summary>
    private void ApplySensitivity()
    {
        if (axisController == null) return;

        var controllers = axisController.Controllers;

        if (controllers != null && controllers.Count >= 2)
        {
            // Index 0: Look X (Pan)
            // InputAxisControllerBase 내부의 Controller 구조체에서 Input 필드가 Reader 타입입니다.
            controllers[0].Input.Gain = horizontalSensitivity; // 수평 감도 적용

            // Index 1: Look Y (Tilt)
            // 상하 반전 유지를 위해 음수(-) 처리
            controllers[1].Input.Gain = -verticalSensitivity;   // 수직 감도 적용
        }
    }

    // 외부(설정 UI 등)에서 감도를 변경할 때 호출하는 메서드
    public void UpdateSensitivity(float h, float v)
    {
        horizontalSensitivity = h;
        verticalSensitivity = v;
        ApplySensitivity();
    }

    // UI 열림 / 닫힘 시 Cinemachine 마우스 입력 제어 메서드
    public void SetCameraInputActive(bool isActive)
    {
        if (axisController != null)
        {
            // AxisController 컴포넌트를 비활성화하면 마우스 이동 입력을 즉시 멈춥니다.
            axisController.enabled = isActive;
        }
    }
}
