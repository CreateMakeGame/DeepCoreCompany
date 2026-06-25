using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewContractTemplate", menuName = "ScriptableObjects/Contract Template")]
public class ContractTemplateSO : ScriptableObject
{
    [Header("Base Info")]
    public CompanyType company;
    public string contractTitleTemplate; // 의뢰 제목 접두사 혹은 템플릿
    public string difficulty = "중";
    public string region = "A 구역";
    public string targetSceneName;

    [Header("Random Range Settings")]
    public int minDeadline = 3;
    public int maxDeadline = 7;
    public int minRewardMoney = 1000;
    public int maxRewardMoney = 5000;
    public int penaltyMoney = 500;

    [Header("Target Kinds Restrictions (Max 5)")]
    [Range(1, 5)] public int minTargetKinds = 1; // 한 의뢰에 등장할 최소 아이템 종류 수
    [Range(1, 5)] public int maxTargetKinds = 5; // 한 의뢰에 등장할 최대 아이템 종류 수

    [Header("Available Item Pool (회사별 아이템 후보군과 수량 제한)")]
    public List<TargetItemPoolData> itemPool = new List<TargetItemPoolData>();
}

[System.Serializable]
public class TargetItemPoolData
{
    public string itemName;       // 아이템 이름 (예: 금화, 에너지 코어 등)
    public ItemType itemType;     // 대분류 설정
    public int minQuantity = 1;   // 이 아이템이 선정되었을 때의 최소 수량
    public int maxQuantity = 10;  // 이 아이템이 선정되었을 때의 최대 수량
}