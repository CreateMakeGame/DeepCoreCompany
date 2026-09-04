using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    [Header("UI 요소 연결")]
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

        if(gameObject.activeSelf)
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
        // 자기 자신(gameObject)을 UIManager에 전달
        UIManager.Instance?.OpenUI(gameObject);
    }

    public void CloseOption()
    {
        UIManager.Instance?.CloseUI(gameObject);
    }
}
