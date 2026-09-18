using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapSelectUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI quotaText;
    public CardUI leftCardUI;
    public CardUI rightCardUI;

    [Header("Map Data Assets")]
    public MapDataSO leftMapData;
    public MapDataSO rightMapData;

    [Header("Button")]
    [SerializeField] private Button exitButton;

    GameManager gameManager;

    private void Awake()
    {
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(CloseUI);
        }
    }
    private void OnDestroy()
    {
        // 오브젝트 파괴 시 이벤트 해제 (메모리 누수 방지)
        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(CloseUI);
        }
    }
    private void Start()
    {
        InitCards();
    }

    public void InitCards(MapDataSO leftData = null, MapDataSO rightData = null)
    {
        // 넘겨받은 데이터가 있다면 우선 적용
        if (leftData != null) leftMapData = leftData;
        if (rightData != null) rightMapData = rightData;

        if (leftCardUI != null && leftMapData != null)
        {
            leftCardUI.Setup(leftMapData);
        }

        if (rightCardUI != null && rightMapData != null)
        {
            rightCardUI.Setup(rightMapData);
        }
    }

    public void OpenUI(MapDataSO leftData = null, MapDataSO rightData = null)
    {
        UpdateQuotaText();
        // UI 갱신
        InitCards(leftData, rightData);

        // UIManager 스택 등록 & 커서 활성화 처리
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenUI(gameObject);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    private void UpdateQuotaText()
    {
        if (quotaText != null)
        {
            quotaText.text = $"할당량 : {GameManager.Instance.currentQuota}";
        }
    }

    public void CloseUI()
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
