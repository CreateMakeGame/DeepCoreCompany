using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
public class QuickSlotUI : MonoBehaviour
{
    [Header("Slot UI References")]
    [SerializeField] private List<GameObject> slotObjects = new List<GameObject>();
    [SerializeField] private List<Image> highlightImages = new List<Image>();

    [Header("Setting")]
    [SerializeField] private int currentSelectedIndex = 0;

    private Player_Actions inputActions;

    public int CurrentSelectedSlotIndex => currentSelectedIndex;

    private void Awake()
    {
        inputActions = new Player_Actions();
    }
    void Start()
    {
        SelectSlot(0);
    }
    private void OnEnable()
    {
        inputActions.Enable();
        // 1~4 키 입력 이벤트 등록
        inputActions.Player.QuickSlot.performed += OnQuickSlotKeyPressed;
        // 마우스 휠 스크롤 이벤트 등록
        inputActions.Player.QuickSlotScroll.performed += OnQuickSlotScrolled;
    }

    private void OnDisable()
    {
        inputActions.Player.QuickSlot.performed -= OnQuickSlotKeyPressed;
        inputActions.Player.QuickSlotScroll.performed -= OnQuickSlotScrolled;
        inputActions.Disable();
    }
    // 1~4 키 입력 이벤트 처리
    private void OnQuickSlotKeyPressed(InputAction.CallbackContext context)
    {
        string keyPressed = context.control.name; // 눌린 키의 이름을 가져옴

        if (int.TryParse(keyPressed, out int slotNumber))
        {
            int index = slotNumber - 1; // 1~4 키를 0~3 인덱스로 변환
            SelectSlot(index);
        }
    }
    // 마우스 휠 스크롤 이벤트 처리
    private void OnQuickSlotScrolled(InputAction.CallbackContext context)
    {
        float scrollValue = context.ReadValue<Vector2>().y; // 마우스 휠 스크롤 값 가져오기

        if (scrollValue > 0)
        {
            // 휠을 위로 돌림 -> 왼쪽 슬롯으로 이동
            int targetIndex = currentSelectedIndex - 1;

            // 인덱스가 0보다 작으면 마지막 슬롯으로 이동
            if (targetIndex < 0) targetIndex = slotObjects.Count - 1; // 마지막 슬롯으로 이동

            SelectSlot(targetIndex);
        }
        else if(scrollValue < 0)
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
