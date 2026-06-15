using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Contract
{
    public string contractID;
    public CompanyType company;
    public string contractTitle;    // 의뢰 이름
    public ItemType targetItem;     // 목표 아이템
    public int targetQuantity;      // 목표 수량
    public int rewardMoney;         // 보상 금액

    [Header("UI Display Data")] 
    public string difficulty;     // 난이도
    public string region;         // 지역
    public int deadline;          // 기한 (일)
    public int penaltyMoney;      // 패널티 금액

    public bool isAccepted = false;  // 계약 수락 여부
}
public class ContractManager : MonoBehaviour
{
    public static ContractManager Instance => GlobalManagers.Instance != null ? GlobalManagers.Instance.Contract : null;
    
    [Header("Progression Settings")]
    public  int totalCompletedContracts = 0; // 총 완료된 계약 수

    [Header("Available Contracts on Board")]
    public List<Contract> availableContracts = new List<Contract>();        // 현재 게시판에 나와 있는 계약 목록
    public List<CompanyType> unlockedCompanies = new List<CompanyType>();   // 잠금 해제된 회사 목록

    private void Start()
    {
        UpdateUnlockedCompanies();
        GenerateNewContracts();
    }
    // 납품 성공 시 호출할 함수 (점수 올리고 회사 해금 체크)
    public void CompleteContract(Contract contract)
    {
        totalCompletedContracts++;
        // TODO: Inventory.Instance.Gold += contract.rewardMoney; (돈 지급)
        if (Inventory.Instance != null)
        {
            // Inventory 스크립트에 Gold 변수가 생기면 아래 주석을 해제하시면 됩니다.
            // Inventory.Instance.Gold += contract.rewardMoney; 
        }

        UpdateUnlockedCompanies(); // 새로운 회사가 해금될 조건인지 체크
        GenerateNewContracts();    // 게시판 의뢰서 목록 갱신
    }

    private void UpdateUnlockedCompanies()
    {
        unlockedCompanies.Clear();

        // 기본 제공 
        unlockedCompanies.Add(CompanyType.BlackstoneMining);

        if (totalCompletedContracts >= 2)
        {
            unlockedCompanies.Add(CompanyType.NationalMiseum);
        }
        if (totalCompletedContracts >= 5)
        {
            unlockedCompanies.Add(CompanyType.HelixBiolab);
        }
        if (totalCompletedContracts >= 10)
        {
            unlockedCompanies.Add(CompanyType.AegisDefense);
        }
    }
    public void GenerateNewContracts()
    {
        availableContracts.Clear();

        // 잠금 해제된 회사 목록을 기반으로 새로운 계약 생성
        // 해금된 회사당 1~2개씩 의뢰서 생성하기
        foreach (CompanyType company in unlockedCompanies)
        {
            ItemType randomType = ItemType.Mineral;
            if (company == CompanyType.NationalMiseum) randomType = ItemType.Artifact;
            else if (company == CompanyType.HelixBiolab) randomType = ItemType.Sample;
            else if (company == CompanyType.AegisDefense) randomType = ItemType.Hazardous;

            // 예시용 임시 의뢰 생성 (나중에 각 회사별 아이템 Pool에서 뽑도록 고도화 가능)
            Contract newContract = new Contract
            {
                contractID = Guid.NewGuid().ToString(),
                company = company,
                targetItem = randomType,
                targetQuantity = UnityEngine.Random.Range(2, 6),
                rewardMoney = UnityEngine.Random.Range(1000, 5000)
            };

            availableContracts.Add(newContract);
        }

        Debug.Log($"게시판 갱신 완료! 현재 참여 회사 수: {unlockedCompanies.Count}개");
    }
}
