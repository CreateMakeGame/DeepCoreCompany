using UnityEngine;

public class ItemObject : MonoBehaviour
{
    [SerializeField] private ItemData itemData;   // 아이템 데이터 참조
    public ItemData Data => itemData;                 // 아이템 데이터 공개 프로퍼티

    public string GetInteractionText()
    {
        if (itemData == null) return "줍기 [E]";
        return $"{itemData.itemName} 줍기 [E] (무게: {itemData.weight}kg)";
    }
}
