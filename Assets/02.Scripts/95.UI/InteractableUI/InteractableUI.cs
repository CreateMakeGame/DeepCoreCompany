using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class InteractableUI : MonoBehaviour
{
    [System.Serializable]
    public struct InteractionIconData
    {
        public InteractionType interactionType;
        public Sprite iconSprite;
    }

    [Header("UI 구성요소")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI interactionText;

    [Header("아이콘 설정")]
    [SerializeField] private Sprite defaultIcon;                    // 기본 점 아이콘
    [SerializeField] private List<InteractionIconData> iconList;

    private Dictionary<InteractionType, Sprite> iconDict;

    private void Awake()
    {
        // Awake에서 Dictionary 사전 등록 수행
        iconDict = new Dictionary<InteractionType, Sprite>();

        if (iconList != null)
        {
            foreach (var data in iconList)
            {
                if (!iconDict.ContainsKey(data.interactionType))
                {
                    iconDict.Add(data.interactionType, data.iconSprite);
                }
            }
        }
    }
    private void Start()
    {
        HidePrompt();
    }

    public void ShowPrompt(InteractionType type, string title, string prompt)
    {
        gameObject.SetActive(true);

        // 아이콘 변경
        if (iconImage != null)
        {
            if (iconDict != null && iconDict.TryGetValue(type, out Sprite sprite) && sprite != null)
            {
                iconImage.sprite = sprite;
            }
            else iconImage.sprite = defaultIcon; // 등록된 게 없으면 기본 점 출력
        }

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(true);
            interactionText.text = $"{title}\n{prompt}";
        }
    }

    // 바라보지 않을 때 (점은 기본 모양으로 원복, 텍스트만 숨김)
    public void HidePrompt()
    {
        if (iconImage != null && defaultIcon != null)
        {
            iconImage.sprite = defaultIcon;
        }

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }
}
