using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    public void Setup(ItemDataSO itemData)
    {
        if (itemData != null && itemData.itemIcon != null)
        {
            iconImage.sprite = itemData.itemIcon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }
    }
}
