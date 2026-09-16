using System;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    [Header("UI Reference")]
    public Image mapPreviewImage;
    public Transform dangerContainer;
    public Transform itemSlotContainer;
    public Button enterButton;

    [Header("Prefab")]
    public GameObject dangerIconPrefab;
    public GameObject itemSlotPrefab;   // 아이템 슬롯 전용 프리팹 (ItemSlotUI 부착 필수)


    public void Setup(MapDataSO data)
    {
        // 맵 이미지 설정
        if (mapPreviewImage != null && data.mapPreviewSprite != null)
        {
            mapPreviewImage.sprite = data.mapPreviewSprite;
        }

        // 기존 생성된 아이콘 비우기
        ClearContainer(dangerContainer);
        ClearContainer(itemSlotContainer);

        // 위험도 아이콘 생성
        if (dangerIconPrefab != null)
        {
            for (int i = 0; i < data.dangerCount; i++)
            {
                Instantiate(dangerIconPrefab, dangerContainer);
            }
        }

        // 아이템 슬롯 생성 (MapDataSO에 등록된 n개의 ItemDataSO 출력)
        if (itemSlotPrefab != null && data.mapItemList != null)
        {
            foreach (ItemDataSO itemData in data.mapItemList)
            {
                GameObject slotObj = Instantiate(itemSlotPrefab, itemSlotContainer);
                ItemSlotUI slotUI = slotObj.GetComponent<ItemSlotUI>();
                if (slotUI != null)
                {
                    slotUI.Setup(itemData);
                }

            }
        }
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}
