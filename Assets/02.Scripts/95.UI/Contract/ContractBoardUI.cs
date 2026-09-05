using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ContractBoardUI : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject contractItemPrefab; // 방금 만든 프리팹을 넣는 곳

    [Header("Panel")]
    [SerializeField] private TextMeshProUGUI completeCountText;
    [SerializeField] private Button closeButton;

    [Header("Left_List_Panel")]
    [SerializeField] private Transform contentTransform; // Scroll View -> Viewport -> Content

    [Header("Right Detail Panel")]
    [SerializeField] private Image detailCompanyLogo;
    [SerializeField] private TextMeshProUGUI detailRightTitle;
    [SerializeField] private TextMeshProUGUI goalText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI regionText;
    //[SerializeField] private TextMeshProUGUI deadlineText;
    [SerializeField] private TextMeshProUGUI rewardText;
    //[SerializeField] private TextMeshProUGUI penaltyText;
    [SerializeField] private Button acceptButton;

    private List<GameObject> spawnedItems = new List<GameObject>();

    private Canvas boardCanvas;
    void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseBoard);
        }
    }
    private void OnDestroy()
    {
        // 씬이 끝나서 내가 파괴될 때는 등록을 해제합니다.
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UnregisterLocalUI<ContractBoardUI>();
        }
    }

    private void OnEnable()
    {
        RefreshBoard();
    }

    private void RefreshBoard()
    {
        // 기존에 생성된 아이템들을 모두 제거
        foreach (GameObject item in spawnedItems)
        {
            Destroy(item);
        }
        spawnedItems.Clear();

        if (ContractManager.Instance == null) return;

        // 상단 헤더에 총 완료 점수 반영하기
        if (completeCountText != null)
        {
            completeCountText.text = $"{ContractManager.Instance.totalCompletedContracts:00}";
        }

        foreach (Contract contract in ContractManager.Instance.availableContracts)
        {
            GameObject newItem = Instantiate(contractItemPrefab, contentTransform);
            spawnedItems.Add(newItem);

            ContractListItemUI listItemScript = newItem.GetComponent<ContractListItemUI>();
            if (listItemScript != null)
            {
                listItemScript.Setup(contract, this);
            }
        }

        // 초기 상태에는 우측 상세 창 정보를 비워두거나 첫 번째 항목 강제 선택 가능
    }

    public void SelectContract(Contract data)
    {
        if (data == null) return;

        detailRightTitle.text = $"[{data.company}] 의뢰서";

        string goalBuilder = "목적 :\n";
        string quantityBuilder = "요구 수량 :\n";

        for (int i = 0; i < data.targets.Count; i++)
        {
            ContractTarget target = data.targets[i];

            // 예: " • 철광석 (Mineral)"
            goalBuilder += $" • {target.itemName} ({target.itemType})";
            // 예: " • 5개 필요 (0/5)"
            quantityBuilder += $" • {target.targetQuantity}개 필요 ({target.currentQuantity}/{target.targetQuantity})";

            // 마지막 줄이 아니라면 줄바꿈(\n)을 추가하여 가독성을 높입니다.
            if (i < data.targets.Count - 1)
            {
                goalBuilder += "\n";
                quantityBuilder += "\n";
            }
        }

        if (goalText != null) goalText.text = goalBuilder;
        if (quantityText != null) quantityText.text = quantityBuilder;

        regionText.text = $"지역 : {data.region}";
        //deadlineText.text = $"기한 : {data.deadline}일";
        rewardText.text = $"보상 : {data.rewardMoney:#,##0} Credits";
        //penaltyText.text = $"패널티 : 기한 초과 시 평판 -{data.penaltyMoney}";

        if(acceptButton != null)
        {
            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(() => AcceptContract(data));
        }
        // LayoutRebuilder를 사용하여 레이아웃을 즉시 재빌드합니다.
        if (goalText != null && goalText.transform.parent != null)
        {
            RectTransform parentRect = goalText.transform.parent as RectTransform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
        }
    }
    private void AcceptContract(Contract data)
    {
        Debug.Log($"{data.contractTitle} 수락 완료! 환경 맵으로 이동 로직을 실행합니다.");
        data.isAccepted = true;

        // 2. 데이터에 저장된 씬 이름으로 이동합니다.
        if (ContractManager.Instance != null)
        {
            ContractManager.Instance.currentAcceptedContract = data;
        }

        if (!string.IsNullOrEmpty(data.targetSceneName))
        {
            SceneManager.LoadScene(data.targetSceneName);
        }
        CloseBoard();
    }
    public void OpenBoard()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenUI(gameObject);
        }
        else
        {
            gameObject.SetActive(true);
        }

    }
    public void CloseBoard()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseUI(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }

    }
}
