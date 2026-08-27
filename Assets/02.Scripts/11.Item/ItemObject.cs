using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ItemObject : MonoBehaviour, IInteractable
{
    [Header("아이템 데이터 연결")]
    [SerializeField] private ItemData itemData;

    [Header("물리 설정")]
    [Tooltip("체크 시 스폰되었을 때 땅속에 가만히 고정")]
    [SerializeField] private bool isBuriedOnSpawn = true;

    private Rigidbody rb;

    public InteractionType interactionType => InteractionType.Pickup;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (isBuriedOnSpawn && rb != null)
        {
            rb.isKinematic = true;
        }
    }

    // UI에 띄울 아이템 이름 전달
    public string GetInteractName()
    {
        return itemData != null ? itemData.itemName : "알 수 없는 아이템";
    }

    // UI에 띄울 상호작용 방법 전달
    public string GetInteractPrompt()
    {
        if (itemData == null) return "조사하기 (E)";

        return $"줍기 (E) - {itemData.weight}kg";
    }

    // 진짜로 주웠을 때의 처리
    public void Interact(GameObject player)
    {
        if (itemData == null) return;

        bool isSuccess = QuickSlotUI.Instance.TryAddItem(itemData);

        if (isSuccess)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 곡괭이로 땅을 파내거나, 공중에서 아이템을 버릴 때 호출하여 물리를 다시 켜는 함수
    /// </summary>
    public void UnfreezePhysics()
    {
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }
}