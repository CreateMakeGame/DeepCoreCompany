using UnityEngine;

public class BulletinBoard : MonoBehaviour, IInteractable
{
    [Header("Board Specific Map Data")]
    [SerializeField] private MapDataSO leftMapData;   // 이 게시판에서 보여줄 왼쪽 맵
    [SerializeField] private MapDataSO rightMapData;  // 이 게시판에서 보여줄 오른쪽 맵

    public InteractionType interactionType => InteractionType.Inspect;

    public string GetInteractName() => "기업 의뢰 게시판";
    public string GetInteractPrompt() => "확인하기 (E)";
    // 플레이어가 E키 등으로 게시판을 바라보고 상호작용했을 때 실행될 내용
    //public void Interact() => OpenContractBoard();
    public void Interact(GameObject player) => OpenContractBoard();

    private void OpenContractBoard()
    {
        MapSelectUI mapUI = UIManager.Instance.GetLocalUI<MapSelectUI>();

        if (mapUI != null)
        {
            mapUI.OpenUI(leftMapData, rightMapData);
        }
        else
        {
            Debug.LogWarning("ContractPanel(MapSelectController)을 UIManager에서 찾을 수 없습니다!");
        }
    }
}
