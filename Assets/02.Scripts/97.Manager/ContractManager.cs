using System;
using System.Collections.Generic;
using UnityEngine;

public class ContractManager : MonoBehaviour
{
    public static ContractManager Instance => GlobalManagers.Instance != null ? GlobalManagers.Instance.Contract : null;

    [Header("SO Templates")]
    [SerializeField] private List<ContractTemplateSO> contractTemplates = new List<ContractTemplateSO>();

    [Header("Progression Settings")]
    public  int totalCompletedContracts = 0; // 총 완료된 계약 수

    public Contract currentAcceptedContract; // 현재 계약

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

        // 해금된 회사들을 순회하면서 의뢰 생성
        foreach (CompanyType company in unlockedCompanies)
        {
            // 1. 해당 회사에 맞는 SO 템플릿 찾기
            ContractTemplateSO template = contractTemplates.Find(t => t.company == company);
            if (template == null || template.itemPool.Count == 0) continue;

            // 2. 새로운 실물 의뢰 인스턴스 생성
            Contract newContract = new Contract
            {
                contractID = System.Guid.NewGuid().ToString(),
                company = company,
                contractTitle = template.contractTitleTemplate,
                difficulty = template.difficulty,
                region = template.region,
                targetSceneName = template.targetSceneName,
                //deadline = UnityEngine.Random.Range(template.minDeadline, template.maxDeadline + 1),
                rewardMoney = UnityEngine.Random.Range(template.minRewardMoney, template.maxRewardMoney + 1),
                //penaltyMoney = template.penaltyMoney
            };

            // 3. 목표 종류 개수 결정 (예: 1종류 ~ 최대 5종류 중 랜덤)
            int targetKindsCount = UnityEngine.Random.Range(template.minTargetKinds, template.maxTargetKinds + 1);
            // 안전장치: 템플릿 아이템 풀에 들어있는 개수보다 많이 뽑을 순 없으므로 보정
            targetKindsCount = Mathf.Min(targetKindsCount, template.itemPool.Count);

            // 4. 아이템 풀 복사 후 무작위로 섞기 (셔플 연산으로 중복 제거)
            List<TargetItemPoolData> shuffledPool = new List<TargetItemPoolData>(template.itemPool);
            for (int i = 0; i < shuffledPool.Count; i++)
            {
                int rnd = UnityEngine.Random.Range(i, shuffledPool.Count);
                var temp = shuffledPool[i];
                shuffledPool[i] = shuffledPool[rnd];
                shuffledPool[rnd] = temp;
            }

            // 5. 섞인 풀에서 정해진 종류 개수만큼 서브 목표(`ContractTarget`) 생성해서 추가
            for (int i = 0; i < targetKindsCount; i++)
            {
                TargetItemPoolData poolData = shuffledPool[i];

                ContractTarget target = new ContractTarget
                {
                    itemName = poolData.itemName,
                    itemType = poolData.itemType,
                    // 각 아이템별로 지정된 수량 제한(최대 3개, 최대 100개 등) 내에서 무작위 결정!
                    targetQuantity = UnityEngine.Random.Range(poolData.minQuantity, poolData.maxQuantity + 1),
                    currentQuantity = 0
                };

                newContract.targets.Add(target);
            }

            availableContracts.Add(newContract);
        }

        Debug.Log($"게시판 갱신 완료! 진짜 SO 데이터 기반 의뢰서 생성됨.");
    }
}
