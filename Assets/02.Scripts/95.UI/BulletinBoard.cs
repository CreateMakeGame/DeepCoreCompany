using UnityEngine;

public class BulletinBoard : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject contractBoardUI; // 에디터에서 띄울 의뢰서 UI 창 연결

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
        if (contractBoardUI != null)
        {
            // UI를 켜고, ContractManager.Instance.availableContracts 리스트를 받아와
            // 화면에 슬롯 형태로 의뢰서 목록을 뿌려줍니다.
            contractBoardUI.SetActive(true);

            // 마우스 커서 풀기 조작
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Debug.LogWarning("의뢰서 보드 UI 패널이 연결되지 않았습니다!");
        }
    }


}
