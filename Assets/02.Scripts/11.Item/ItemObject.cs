using UnityEngine;

public class ItemObject : MonoBehaviour, IInteractable
{
    [Header("아이템 데이터 연결")]
    [SerializeField] private ItemData itemData;

    // 1. UI에 띄울 아이템 이름 전달
    public string GetInteractName()
    {
        return itemData != null ? itemData.itemName : "알 수 없는 아이템";
    }

    // 2. UI에 띄울 상호작용 방법 전달
    public string GetInteractPrompt()
    {
        if (itemData == null) return "조사하기 (E)";

        // 무게 정보를 포함한 직관적인 UI 가이드
        return $"줍기 (E) - {itemData.weight}kg";
    }

    // 3. 진짜로 주웠을 때의 처리
    public void Interact(GameObject player)
    {
        if (itemData == null) return;

        // 싱글톤 인벤토리에 이 아이템 추가 요청
        //bool isSuccess = Inventory.Instance.AddItem(itemData);
        bool isSuccess = QuickSlotUI.Instance.TryAddItem(itemData);

        if (isSuccess)
        {
            // 추가 성공 시에만 월드에서 아이템 제거
            Destroy(gameObject);
        }
    }
}