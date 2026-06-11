using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    [SerializeField] private TextMeshProUGUI deadlineText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private TextMeshProUGUI penaltyText;
    [SerializeField] private Button acceptButton;

    private List<GameObject> spawnedItems = new List<GameObject>();

    void Start()
    {
        if(closeButton != null)
        {
            closeButton.onClick.AddListener(CloseBoard);
        }
        gameObject.SetActive(false); // 시작할 때는 UI를 꺼둡니다.
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
        goalText.text = $"목적 : {data.targetItem}";
        quantityText.text = quantityText.text = $"수량 : {data.targetQuantity}개 필요 (0/{data.targetQuantity})";
        regionText.text = $"지역 : {data.region}";
        deadlineText.text = $"기한 : {data.deadline}일";
        rewardText.text = $"보상 : {data.rewardMoney:#,##0} Credits";
        penaltyText.text = $"패널티 : 기한 초과 시 평판 -{data.penaltyMoney}";

        if(acceptButton != null)
        {
            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(() => AcceptContract(data));
        }
    }
    private void AcceptContract(Contract data)
    {
        Debug.Log($"{data.contractTitle} 수락 완료! 환경 맵으로 이동 로직을 실행합니다.");
        data.isAccepted = true;

        // TODO: SceneManager.LoadScene("행성 씬 이름"); 
        CloseBoard();
    }

    private void CloseBoard()
    {
        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
