using UnityEngine;

public enum ItemType
{
    Mineral,    // 일반 광물 (철, 금, 루비 등)
    Artifact,   // 고대 유물 (석판, 조각상 등)
    Sample,     // 생물 샘플 (알, 화석 등)
    Hazardous   // 위험 물질 (에너지 코어, 방사성 물질)
}


[CreateAssetMenu(fileName = "New Item Data", menuName = "ScriptableObjects/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("아이템 기본 정보")]
    public string itemID;               // 아이템 고유 ID
    public string itemName;             // 아이템 이름
    [TextArea]
    public string ItemDescription;      // 아이템 설명
    public Sprite itemIcon;             // 아이템 아이콘

    [Header("카테고리 설정")]
    //public ItemType itemType;           // 아이템 유형

    [Header("아이템 가치")]
    public int baseValue;               // 기본 납품 가치 (보상 계산용)

    [Header("비주얼 참조")]
    public GameObject fieldPrefab;    // 아이템 프리팹
}
