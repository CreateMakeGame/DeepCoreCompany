using UnityEngine;

[CreateAssetMenu(fileName = "New Item Data", menuName = "ScriptableObjects/Item DataSO")]
public class ItemDataSO : ScriptableObject
{
    [Header("아이템 기본 정보")]
    public string itemID;               // 아이템 고유 ID
    public string itemName;             // 아이템 이름
    [TextArea]
    public string ItemDescription;      // 아이템 설명
    public Sprite itemIcon;             // 아이템 아이콘

    [Header("게임 플레이 데이터")]
    public int baseValue;               // 기본 납품 가치 (보상 계산용)
    public float weight;                // 아이템 무게 (인벤토리 관리 및 이동 속도에 영향)

    [Header("비주얼 참조")]
    public GameObject fieldPrefab;    // 아이템 프리팹
}