using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject Background;             // Background 오브젝트

    [Header("Inventory Grid (Right)")]
    [SerializeField] private TextMeshProUGUI SlotValue;         // SlotValue ("00 / 00 Slot")
    [SerializeField] private Transform SlotGrid;                // SlotGrid 오브젝트
    [SerializeField] private GameObject inventorySlotPrefab;    // InventorySlot 프리팹

    [Header("Inventory Grid (Left)")] // 이름이 Left(계약창)로 매핑되어 있어 유지합니다.
    [SerializeField] private TextMeshProUGUI CompanyText;       // Compony 텍스트
    [SerializeField] private Image CompanyIcon;                 // ComponyIcon 이미지
    [SerializeField] private TextMeshProUGUI TargetValueText;   // TargetValue (보상금 텍스트)
    [SerializeField] private Transform TargetGridTransform;     // TargetGrid 오브젝트
    [SerializeField] private GameObject TargetSlotPrefab;       // TargetSlot 프리팹
    [Header("State Panel (Bottom)")]
    [SerializeField] private TextMeshProUGUI WeightText;        // Weight 텍스트 ("00.0 / 40.0 kg")
    [SerializeField] private Image WeightBar;                   // 무게 게이지 바의 Fill 이미지 (WeightBar)
    [SerializeField] private TextMeshProUGUI ValueMoney;        // TotalValue 텍스트 (ValueMoney)

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

        // 오브젝트가 꺼질 때 인벤토리 데이터 변경 이벤트 구독 해제
        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryChanged -= RefreshOpenedInventory;
    }

    private void Start()
    {
        if (Background != null) Background.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 실시간 인벤토리 변경 이벤트 구독 (창이 열려있을 때 실시간 갱신용)
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnInventoryChanged += RefreshOpenedInventory;
        }
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
        if (Background == null) return;

        isInventoryOpen = !isInventoryOpen;

        if (isInventoryOpen)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OpenUI(Background);
            }
            else
            {
                Background.SetActive(true);
            }
            UpdateInventoryUI();
            UpdateContractUI();
        }
        else
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CloseUI(Background);
            }
            else
            {
                Background.SetActive(false);
            }
        }
    }
    private void RefreshOpenedInventory()
    {
        if (isInventoryOpen)
        {
            UpdateInventoryUI();
        }
    }

    // 우측 가방 아이템 그리드 갱신
    private void UpdateInventoryUI()
    {
        foreach (var slot in spawnedInventorySlots) Destroy(slot);
        spawnedInventorySlots.Clear();

        if(Inventory.Instance == null) return;

        int currentItemCount = Inventory.Instance.items.Count;
        int maxSlotCount = 20; // 최대 슬롯 수 (예시)

        if (SlotValue != null) SlotValue.text = $"{currentItemCount:00} / {maxSlotCount:00} Slot";

        foreach(var item in Inventory.Instance.items)
        {
            if (inventorySlotPrefab == null || SlotGrid == null) break;

            GameObject newSlot = Instantiate(inventorySlotPrefab, SlotGrid);
            spawnedInventorySlots.Add(newSlot);

            InventorySlot slotScript = newSlot.GetComponent<InventorySlot>();
            if (slotScript != null)
            {
                slotScript.SetItem(item.data, item.quantity);
            }
        }

        // 하단 데이터 세팅에 실제 인벤토리 무게와 가치 전달
        UpdateStatePanel(Inventory.Instance.currentWeight, Inventory.Instance.maxWeight, Inventory.Instance.totalValue);
    }

    // 좌측 계약(퀘스트) 정보 그리드 갱신
    public void UpdateContractUI()
    {
        foreach (var slot in spawnedTargetSlots) Destroy(slot);
        spawnedTargetSlots.Clear();

        // 🔗 [수정] companyText -> CompanyText, targetValueText -> TargetValueText 변수명 일치
        if (CompanyText != null) CompanyText.text = "Blackstone Mining"; // 유저님의 기업명 반영
        if (TargetValueText != null) TargetValueText.text = "$ 12,000";

        int mockQuestCount = 3;
        for (int i = 0; i < mockQuestCount; i++)
        {
            // 🔗 [수정] targetSlotPrefab, targetGridTransform 변수명 일치
            if (TargetSlotPrefab == null || TargetGridTransform == null) break;

            GameObject newTargetSlot = Instantiate(TargetSlotPrefab, TargetGridTransform);
            spawnedTargetSlots.Add(newTargetSlot);

            TextMeshProUGUI targetName = newTargetSlot.transform.Find("TargetName")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI targetValue = newTargetSlot.transform.Find("TargetValue")?.GetComponent<TextMeshProUGUI>();

            if (targetName != null) targetName.text = "Iron Ore";
            if (targetValue != null) targetValue.text = "0 / 3";
        }
    }

    // 하단 무게 및 가치 상태창 업데이트
    private void UpdateStatePanel(float currentWeight, float maxWeight, int totalValue)
    {
        if (WeightText != null)
            WeightText.text = $"{currentWeight:F1} / {maxWeight:F1} kg";

        if (ValueMoney != null)
            ValueMoney.text = $"${totalValue:#,##0}";

        if (WeightBar != null)
        {
            float ratio = maxWeight > 0 ? (currentWeight / maxWeight) : 0;
            WeightBar.fillAmount = ratio;

            if (ratio >= 0.9f) WeightBar.color = new Color(1f, 0.22f, 0.22f);       // 위험 (레드)
            else if (ratio >= 0.7f) WeightBar.color = new Color(1f, 0.8f, 0.14f);   // 주의 (옐로우)
            else WeightBar.color = new Color(0.18f, 0.8f, 0.44f);     // 안전 (그린)
        }
    }
}