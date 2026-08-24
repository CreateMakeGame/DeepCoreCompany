using UnityEngine;

public class BulletinBoard : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject contractBoardUI; // 에디터에서 띄울 의뢰서 UI 창 연결

    public InteractionType interactionType => InteractionType.Inspect;

    public string GetInteractName()
    {
        return "기업 의뢰 게시판";     // UI 제목에 표시될 이름
    }

    public string GetInteractPrompt()
    {
        return "확인하기 (E)";      // 상호작용 안내 문구
    }

    public void Interact()
    {
        // 플레이어가 E키 등으로 게시판을 바라보고 상호작용했을 때 실행될 내용
        OpenContractBoard();
    }

    public void Interact(GameObject player)
    {
        OpenContractBoard();
    }

    private void OpenContractBoard()
    {
        ContractBoardUI board = UIManager.Instance.GetLocalUI<ContractBoardUI>();

        if (board != null)
        {
            board.OpenBoard();
        }
        else
        {
            Debug.LogWarning("의뢰서 보드 UI 패널이 연결되지 않았습니다!");
        }
    }
}
