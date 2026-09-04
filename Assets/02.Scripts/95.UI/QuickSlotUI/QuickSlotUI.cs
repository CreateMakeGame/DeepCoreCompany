using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
public class QuickSlotUI : Singleton<QuickSlotUI>
{
    [Header("Input Channel")]
    [SerializeField] private InputReader inputReader;

    [Header("Slot UI References")]
    [SerializeField] private List<GameObject> slotObjects = new List<GameObject>();
    [SerializeField] private List<Image> highlightImages = new List<Image>();
    [SerializeField] private List<Image> itemIconImages = new List<Image>();

    [Header("Setting")]
    [SerializeField] private int currentSelectedIndex = 0;

    [Header("Drop Settings")]
    [SerializeField] private float forwardForce = 3f; // 앞으로 튀어나가는 힘
    [SerializeField] private float upwardForce = 1f; // 위로 튀어오르는 힘

    private ItemData[] slotItems;

    public int CurrentSelectedSlotIndex => currentSelectedIndex;

    protected override void Awake()
    {
        base.Awake();
        slotItems = new ItemData[slotObjects.Count];
    }
    void Start()
    {
        SelectSlot(0);
        UpdateAllSlotUI();
    }
    private void OnEnable()
    {
        if(inputReader == null)
        {
            Debug.LogError("InputReader is not assigned in QuickSlotUI.");
            return;
        }
        // 1~4 키 입력 이벤트 등록
        inputReader.QuickSlotEvent += OnQuickSlotKeyPressed;
        // 마우스 휠 스크롤 이벤트 등록
        inputReader.QuickSlotScrollEvent += OnQuickSlotScrolled;
        // 드랍 키 이벤트 등록
        inputReader.DropEvent += OnDropKeyPressed;
    }

    private void OnDisable()
    {
        inputReader.QuickSlotEvent -= OnQuickSlotKeyPressed;
        inputReader.QuickSlotScrollEvent -= OnQuickSlotScrolled;
        inputReader.DropEvent -= OnDropKeyPressed;
    }

    /// <summary>
    /// 아이템 획득 시 빈 퀵슬롯 탐색 후 1개 추가 (ItemObject에서 호출)
    /// </summary>
    /// <param name="context"></param>
    public bool TryAddItem(ItemData newItem)
    {
        // 1. 현재 하이라이트(선택)된 슬롯이 비어있다면 1순위로 채움
        if (slotItems[currentSelectedIndex] == null)
        {
            slotItems[currentSelectedIndex] = newItem;
            UpdateSlotUI(currentSelectedIndex);
            return true;
        }
        // 2. 현재 슬롯이 이미 채워져 있다면 다른 빈 슬롯 탐색
        for (int i = 0; i < slotItems.Length; i++)
        {
            if (slotItems[i] == null)
            {
                slotItems[i] = newItem;
                UpdateSlotUI(i);
                return true;
            }
        }
        return false; // 빈 슬롯이 없으면 false 반환
    }
    // Q 키 눌렀을 때 현재 선택된 슬롯의 아이템 드랍
    private void OnDropKeyPressed()
    {
        DropSelectedItem();
    }

    private void DropSelectedItem()
    {
        ItemData itemToDrop = slotItems[currentSelectedIndex];

        if (itemToDrop == null) return; // 빈 슬롯이면 무시
        
        Camera mainCamera = Camera.main;
        Vector3 dropDirection;
        Vector3 spawnPosition;

        if (mainCamera != null)
        {
            dropDirection = mainCamera.transform.forward;
            spawnPosition = mainCamera.transform.position + dropDirection * 2f;
        }
        else
        {
            // 카메라이 없을 경우 플레이어 기준
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Transform playerTrans = player != null ? player.transform : transform;

            dropDirection = playerTrans.forward;
            spawnPosition = playerTrans.position + dropDirection * 1.2f + Vector3.up * 0.5f;
        }

        // 1. 월드에 fieldPrefab 생성
        if (itemToDrop.fieldPrefab != null)
        {
            // 카메라 바라보는 방향을 바라보도록 생성
            GameObject droppedObject = Instantiate(itemToDrop.fieldPrefab, spawnPosition, Quaternion.LookRotation(dropDirection));

            if(droppedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                Vector3 dropImpulse = dropDirection * forwardForce + Vector3.up * upwardForce;
                rb.AddForce(dropImpulse, ForceMode.Impulse);
            }
        }

        // 2. 퀵슬롯 데이터 비우기 & UI 갱신
        slotItems[currentSelectedIndex] = null;
        UpdateSlotUI(currentSelectedIndex);
    }

    // 슬롯 UUI 아이콘 개별 갱신
    private void UpdateSlotUI(int index)
    {
        if (index < 0 || index >= itemIconImages.Count) return;

        if (slotItems[index] != null)
        {
            itemIconImages[index].sprite = slotItems[index].itemIcon;
            itemIconImages[index].gameObject.SetActive(true);
        }
        else
        {
            itemIconImages[index].sprite = null;
            itemIconImages[index].gameObject.SetActive(false);
        }
    }

    private void UpdateAllSlotUI()
    {
        for (int i = 0; i < slotItems.Length; i++)
        {
            UpdateSlotUI(i);
        }
    }

    // 숫자 키(1~4 등) 입력 이벤트 처리 (InputReader에서 int slotIndex 전달)
    private void OnQuickSlotKeyPressed(int slotIndex)
    {
        // 입력받은 인덱스(1, 2, 3, 4 등)를 배열 인덱스(0, 1, 2, 3)로 변환
        int index = slotIndex - 1;
        SelectSlot(index);
    }

    // 마우스 휠 스크롤 이벤트 처리
    private void OnQuickSlotScrolled(float scrollY)
    {
        if (scrollY > 0)
        {
            // 휠을 위로 돌림 -> 왼쪽 슬롯으로 이동
            int targetIndex = currentSelectedIndex - 1;
            // 인덱스가 0보다 작으면 마지막 슬롯으로 이동
            if (targetIndex < 0) targetIndex = slotObjects.Count - 1; // 마지막 슬롯으로 이동
            SelectSlot(targetIndex);
        }
        else if(scrollY < 0)
        {
            // 휠을 아래로 돌림 -> 오른쪽 슬롯으로 이동
            int targetIndex = currentSelectedIndex + 1;
            // 인덱스가 마지막 슬롯보다 크면 첫 슬롯으로 이동
            if (targetIndex >= slotObjects.Count) targetIndex = 0; // 첫 슬롯으로 이동

            SelectSlot(targetIndex);
        }
    }

    private void SelectSlot(int Index)
    {
        if (Index < 0 || Index >= slotObjects.Count) return;

        currentSelectedIndex = Index;

        for (int i = 0; i < highlightImages.Count; i++)
        {
            if (highlightImages[i] != null)
            {
                highlightImages[i].gameObject.SetActive(i == currentSelectedIndex);
            }
        }
    }
}
