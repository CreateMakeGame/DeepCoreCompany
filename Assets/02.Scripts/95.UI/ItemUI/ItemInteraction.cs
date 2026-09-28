using UnityEngine;

[RequireComponent(typeof(ItemObject))]
public class ItemInteraction : MonoBehaviour, IInteractable
{
    private ItemObject itemObject;

    public InteractionType interactionType => InteractionType.Pickup;

    private void Awake()
    {
        itemObject = GetComponent<ItemObject>();
    }

    // UI에 띄울 아이템 이름 전달
    public string GetInteractName()
    {
        if (itemObject == null || itemObject.ItemData == null)
            return "알 수 없는 아이템";

        return itemObject.ItemData.itemName;
    }

    // UI에 띄울 상호작용 문구 전달
    public string GetInteractPrompt()
    {
        if (itemObject == null || itemObject.ItemData == null)
            return "조사하기 (E)";

        // 만약 땅에 묻혀있는 상태라면 UI 안내문 변경 (선택 사항)
        if (itemObject.IsBuried)
        {
            return "땅에 파묻혀 있음";
        }

        return "줍기 (E)";
    }

    // 실제 상호작용(E키 입력 시) 처리
    public void Interact(GameObject player)
    {
        if (itemObject == null || itemObject.ItemData == null) return;

        // 아직 파내지 않고 묻혀있는 상태라면 줍기 금지
        if (itemObject.IsBuried) return;

        bool isSuccess = QuickSlotUI.Instance.TryAddItem(itemObject.ItemData);

        if (isSuccess)
        {
            Destroy(gameObject);
        }
    }
}