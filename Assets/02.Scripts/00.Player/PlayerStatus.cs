using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    [Header("UI Imgage Ref")]
    [SerializeField] private Image hpImage;
    [SerializeField] private Image staminaImage;

    [Header("References")]
    [SerializeField] private PlayerStateMachine stateMachine;

    public float CurrentHp { get; private set; }
    public float CurrentStamina { get; private set; }

    private float regenTimer = 0f;       // 스테미너 회복 대기 시간 타이머

    private void Awake()
    {
        if (stateMachine == null)
        {
            stateMachine = GetComponent<PlayerStateMachine>();
        }
    }

    private void Start()
    {
        if (stateMachine != null && stateMachine.Data != null)
        {
            CurrentHp = stateMachine.Data.maxHp;
            CurrentStamina = stateMachine.Data.maxStamina;
        }
        else
        {
            Debug.LogError("PlayerStateMachine or PlayerData is not assigned.");
        }
        BindUIReferences();
    }


    private void Update()
    {
        // 혹시 Start 시점에 UI 바인딩을 실패했었다면 복구 시도
        if (hpImage == null || staminaImage == null)
        {
            BindUIReferences();
        }

        HandleStamina();
        UpdateStatusUI();
    }

    private void BindUIReferences()
    {
        if (UIManager.Instance == null || UIManager.Instance.MainHUDPanel == null)
        {
            Debug.LogWarning("[PlayerStatus] UIManager 또는 MainHUDPanel을 찾을 수 없습니다.");
            return;
        }
        GameObject hudRoot = UIManager.Instance.MainHUDPanel;

        foreach (Image img in hudRoot.GetComponentsInChildren<Image>(true))
        {
            if (img.name.Equals("HP", StringComparison.OrdinalIgnoreCase))
            {
                hpImage = img;
            }
            else if (img.name.Equals("Stamina", StringComparison.OrdinalIgnoreCase))
            {
                staminaImage = img;
            }
        }
        if (hpImage == null) Debug.LogWarning("[PlayerStatus] HP Image 바인딩 실패!");
        if (staminaImage == null) Debug.LogWarning("[PlayerStatus] Stamina Image 바인딩 실패!");
    }

    private void HandleStamina()
    {
        if (stateMachine == null || stateMachine.Data == null) return;

        bool isSprinting = stateMachine.CurrentState == stateMachine.Run;
        bool isMining = stateMachine.CurrentState == stateMachine.Dig;

        if (isSprinting || isMining)
        {
            CurrentStamina -= stateMachine.Data.staminaDrainRate * Time.deltaTime;
            CurrentStamina = Mathf.Clamp(CurrentStamina, 0f, stateMachine.Data.maxStamina);
            regenTimer = 0f; // 스테미너 소모 시 회복 대기 시간 초기화
        }
        else
        {
            // 스테민 회복 대기 및 회복 처리
            if (regenTimer >= stateMachine.Data.regenDelay)
            {
                CurrentStamina += stateMachine.Data.staminaRegenRate * Time.deltaTime;
                CurrentStamina = Mathf.Clamp(CurrentStamina, 0f, stateMachine.Data.maxStamina);
            }
            else
            {
                regenTimer += Time.deltaTime;
            }
        }
    }

    private void UpdateStatusUI()
    {
        if (stateMachine == null || stateMachine.Data == null) return;

        // 1. HP UI 갱신 (0.0 ~ 1.0)
        if (hpImage != null)
        {
            hpImage.fillAmount = CurrentHp / stateMachine.Data.maxHp;
        }

        // 2. Stamina UI 갱신 (우측 반원에 맞춰 0.0 ~ 0.5 매핑)
        if (staminaImage != null)
        {
            staminaImage.fillAmount = (CurrentStamina / stateMachine.Data.maxStamina) * 0.5f;
        }
    }

    // 스테미너가 남아있는지 확인
    public bool HasStamina() => CurrentStamina > 0f;

    // HP 감소 (데미지 처리)
    public void TakeDamage(float amount)
    {
        if (stateMachine == null || stateMachine.Data == null) return;
        CurrentHp = Mathf.Clamp(CurrentHp - amount, 0f, stateMachine.Data.maxHp);
    }
}
