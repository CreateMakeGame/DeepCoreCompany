using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;          // 내부의 Icon 이미지 컴포넌트
    [SerializeField] private TextMeshProUGUI quantityText; // 내부의 Text (TMP) 컴포넌트

    public void SetItem(ItemData itemData, int quantity)
    {
        if (itemData == null) return;

        // 아이콘 이미지 변경 및 활성화
        iconImage.sprite = itemData.itemIcon;
        iconImage.gameObject.SetActive(true);

        // 아이템 개수가 1개보다 많을 때만 숫자 띄우기
        if (quantity > 1)
        {
            quantityText.text = quantity.ToString();
            quantityText.gameObject.SetActive(true);
        }
        else
        {
            quantityText.gameObject.SetActive(false); // 1개일 때는 숫자 숨기기
        }
    }
}