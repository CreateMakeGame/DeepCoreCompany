using UnityEngine;

[CreateAssetMenu(fileName = "New Item Data", menuName = "ScriptableObjects/Item DataSO")]
public class ItemDataSO : ScriptableObject
{
    [Header("아이템 기본 정보")]
    public string itemID;                   // 아이템 고유 ID
    public string itemName;                 // 아이템 이름
    [TextArea]
    public string ItemDescription;          // 아이템 설명
    public Sprite itemIcon;                 // 아이템 아이콘

    [Header("게임 플레이 데이터")]
    public int baseValue;                   // 기본 납품 가치 (보상 계산용)

    [Header("아이템 참조")]
    public GameObject fieldPrefab;          // 기본 아이템/구조물 프리팹

    [Header("특수 드롭 조건")]
    public bool isSpecialCondition;         // 조건
    public GameObject specialFieldPrefab;   // 조건 충족 시 대체할 드롭 프리팹

    /// <summary>
    /// 동굴이나 맵 상에 기본 스폰할 구조물/아이템 프리팹 가져오기
    /// </summary>
    public GameObject GetSpawnPrefab()
    {
        return fieldPrefab;
    }

    /// <summary>
    /// 오브젝트를 파내거나 파괴했을 때 바닥에 드롭되는 아이템 프리팹을 가져옵니다.
    /// </summary>
    public GameObject GetDropPrefab()
    {
        // 특수 조건 체크 시 드롭 아이템(specialFieldPrefab)을 반환
        if (isSpecialCondition && specialFieldPrefab != null)
        {
            return specialFieldPrefab;
        }
        return fieldPrefab;
    }
}