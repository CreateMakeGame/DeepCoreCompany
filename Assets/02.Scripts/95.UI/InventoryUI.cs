using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject inventoryPanel;         // Background 오브젝트

    [Header("Inventory Grid (Right)")]
    [SerializeField] private TextMeshProUGUI slotValueText;     // SlotValue ("00 / 00 Slot")
    [SerializeField] private Transform slotGridTransform;       // SlotGrid 오브젝트
    [SerializeField] private GameObject slotPrefab;             // InventorySlot 프리팹

    [Header("Inventory Grid (Left)")] // 이름이 Left(계약창)로 매핑되어 있어 유지합니다.
    [SerializeField] private TextMeshProUGUI companyText;       // Compony 텍스트
    [SerializeField] private Image companyIcon;                 // ComponyIcon 이미지
    [SerializeField] private TextMeshProUGUI targetValueText;   // TargetValue (보상금 텍스트)
    [SerializeField] private Transform targetGridTransform;     // TargetGrid 오브젝트
    [SerializeField] private GameObject targetSlotPrefab;       // TargetSlot 프리팹

    [Header("State Panel (Bottom)")]
    [SerializeField] private TextMeshProUGUI weightText;        // Weight 텍스트 ("00.0 / 40.0 kg")
    [SerializeField] private Image weightBarFill;               // 무게 게이지 바의 Fill 이미지 (WeightBar)
    [SerializeField] private TextMeshProUGUI totalValueText;    // TotalValue 텍스트 (ValueMoney)

    private List<GameObject> spawnedInventorySlots = new List<GameObject>();
    private List<GameObject> spawnedTargetSlots = new List<GameObject>();
    private bool isInventoryOpen = false;

    // [추가] C# 입력 액션 클래스 변수
    private Player_Actions inputActions;

    private void Awake()
    {
        // 입력 액션 인스턴스 생성
        inputActions = new Player_Actions();
    }

    private void OnEnable()
    {
        // Inventory 액션이 실행(performed)될 때 토글 함수 연결
        inputActions.Player.Inventory.performed += OnInventoryPerformed;
        inputActions.Player.Inventory.Enable();
    }

    private void OnDisable()
    {
        // 오브젝트가 꺼질 때 이벤트 해제 및 액션 비활성화
        inputActions.Player.Inventory.performed -= OnInventoryPerformed;
        inputActions.Player.Inventory.Disable();
    }

    private void Start()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Input System의 C# 이벤트 콜백 함수
    private void OnInventoryPerformed(InputAction.CallbackContext context)
    {
        ToggleInventory();
    }

    // 인벤토리 열기/닫기 토글 기능 (외부 연동을 위해 public으로 변경)
    public void ToggleInventory()
    {
        // 패널이 할당되어 있지 않을 때만 예외 처리로 리턴합니다.
        if (inventoryPanel == null) return;

        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            // 인벤토리가 열릴 때: 마우스 풀고 화면 갱신
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            UpdateInventoryUI();
            UpdateContractUI();
        }
        else
        {
            // 인벤토리가 닫힐 때: 마우스 다시 가리기
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // 우측 가방 아이템 그리드 갱신
    private void UpdateInventoryUI()
    {
        foreach (var slot in spawnedInventorySlots) Destroy(slot);
        spawnedInventorySlots.Clear();

        int currentItemCount = 12;
        int maxSlotCount = 24;

        if (slotValueText != null)
            slotValueText.text = $"{currentItemCount:00} / {maxSlotCount:00} Slot";

        for (int i = 0; i < currentItemCount; i++)
        {
            if (slotPrefab == null || slotGridTransform == null) break;

            GameObject newSlot = Instantiate(slotPrefab, slotGridTransform);
            spawnedInventorySlots.Add(newSlot);

            TextMeshProUGUI countText = newSlot.GetComponentInChildren<TextMeshProUGUI>();
            if (countText != null) countText.text = "1";
        }

        // 하단 데이터 세팅 (샘플 데이터)
        UpdateStatePanel(18.5f, 40.0f, 5420);
    }

    // 좌측 계약(퀘스트) 정보 그리드 갱신
    public void UpdateContractUI()
    {
        foreach (var slot in spawnedTargetSlots) Destroy(slot);
        spawnedTargetSlots.Clear();

        if (companyText != null) companyText.text = "DeepCore Co.";
        if (targetValueText != null) targetValueText.text = "$ 12,000";

        int mockQuestCount = 3;
        for (int i = 0; i < mockQuestCount; i++)
        {
            if (targetSlotPrefab == null || targetGridTransform == null) break;

            GameObject newTargetSlot = Instantiate(targetSlotPrefab, targetGridTransform);
            spawnedTargetSlots.Add(newTargetSlot);

            TextMeshProUGUI targetName = newTargetSlot.transform.Find("TargetName")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI targetValue = newTargetSlot.transform.Find("TargetValue")?.GetComponent<TextMeshProUGUI>();

            if (targetName != null) targetName.text = "Egg egg egG";
            if (targetValue != null) targetValue.text = "0 / 3";
        }
    }

    // 하단 무게 및 가치 상태창 업데이트
    private void UpdateStatePanel(float currentWeight, float maxWeight, int totalValue)
    {
        if (weightText != null)
            weightText.text = $"{currentWeight:F1} / {maxWeight:F1} kg";

        if (totalValueText != null)
            totalValueText.text = $"${totalValue:#,##0}";

        if (weightBarFill != null)
        {
            float ratio = currentWeight / maxWeight;
            weightBarFill.fillAmount = ratio;

            if (ratio >= 0.9f) weightBarFill.color = new Color(1f, 0.22f, 0.22f);      // 위험 (레드)
            else if (ratio >= 0.7f) weightBarFill.color = new Color(1f, 0.8f, 0.14f);       // 주의 (옐로우)
            else weightBarFill.color = new Color(0.18f, 0.8f, 0.44f);    // 안전 (그린)
        }
    }
}