using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ContractTarget
{
    public string itemName;      // 아이템 구체적 이름 (예: 금화, 철광석, 에너지 코어)
    public ItemType itemType;     // 아이템 대분류 (Mineral, Artifact, Sample, Hazardous)
    public int targetQuantity;    // 목표 수량
    public int currentQuantity;   // 현재 납품/수집된 수량 (인벤토리 연동용, 기본 0)
}

[System.Serializable]
public class Contract
{
    public string contractID;
    public CompanyType company;
    public string contractTitle;    // 의뢰 이름

    // 하나의 의뢰에 여러 목표(최대 5개)를 담을 수 있도록 리스트로 변경
    public List<ContractTarget> targets = new List<ContractTarget>();

    public int rewardMoney;         // 보상 금액

    [Header("UI Display Data")]
    public string difficulty;     // 난이도
    public string region;         // 지역
    public int deadline;          // 기한 (일)
    public int penaltyMoney;      // 패널티 금액

    public bool isAccepted = false;  // 계약 수락 여부
}