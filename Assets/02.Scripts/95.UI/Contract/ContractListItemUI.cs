using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContractListItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image companyLogoImage;
    [SerializeField] private TextMeshProUGUI companyTitleText; // [CompanyTitle]
    [SerializeField] private TextMeshProUGUI infoText;         // [InfoText] (난이도/지역/기한)
    [SerializeField] private TextMeshProUGUI rewardText;       // [RewardText]

    private Contract contractData;
    private ContractBoardUI contractBoardUI;

    public void Setup(Contract data, ContractBoardUI boardUI)
    {
        contractData = data;
        contractBoardUI = boardUI;

        //companyLogoImage.sprite = ContractManager.Instance.GetCompanyLogo(data.company);
        companyTitleText.text = $"[{data.company}] {data.contractTitle}";
        infoText.text = $"난이도: {data.difficulty}  |  지역: {data.region}";
        rewardText.text = $"{data.rewardMoney:#,##0} Cr";
    }

    public void OnClickItem()
    {
        if(contractBoardUI != null)
        {
            contractBoardUI.SelectContract(contractData);
        }
    }
}
