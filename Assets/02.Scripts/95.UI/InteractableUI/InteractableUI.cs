using TMPro;
using UnityEngine;

public class InteractableUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI interactionText;

    public void ShowPrompt(string title, string prompt)
    {
        gameObject.SetActive(true);
        // 예: "[기업 의뢰 게시판] 확인하기 (E)" 형태로 출력
        //interactionText.text = $"[{title}] {prompt}";
        interactionText.text = $"{title}\n{prompt}";
    }

    public void HidePrompt()
    {
        gameObject.SetActive(false);
    }
}
