using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    [Header("UI 요소 연결")]
    [SerializeField] private GameObject optionPanel;    // OptionPanel
    [SerializeField] private Button optionQuitBtn;      // OptionQuit
    [SerializeField] private Button optionXBtn;         // OptionX

    private void Awake()
    {
        if (optionXBtn != null)
        {
            optionXBtn.onClick.AddListener(CloseOption);
        }

        if (optionQuitBtn != null)
        {
            optionQuitBtn.onClick.AddListener(() => { 
                UIManager.Instance.QuitGame(); 
            });
        }
    }

    private void Start()
    {
        UIManager.Instance?.RegisterLocalUI(this);
    }
    private void OnDestroy()
    {
        UIManager.Instance?.UnregisterLocalUI<OptionUI>();
    }

    // 옵션 창 켜기 / 끄기
    public void ToggleOption()
    {
        if(optionPanel == null) return;

        if(optionPanel.activeSelf)
        {
            CloseOption();
        }
        else
        {
            OpenOption();
        }
    }

    public void OpenOption()
    {
        if(optionPanel != null)
        {
           UIManager.Instance?.OpenUI(optionPanel);
        }
    }

    public void CloseOption()
    {
        if(optionPanel != null)
        {
            UIManager.Instance?.CloseUI(optionPanel);
        }
    }
}
