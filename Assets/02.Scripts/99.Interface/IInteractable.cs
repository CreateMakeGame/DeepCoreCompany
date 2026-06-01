using UnityEngine;

public interface IInteractable
{
    // UI의 제목 부분에 들어갈 이름 (예: "철광석", "블랙스톤 대리인")
    string GetInteractName();

    // UI의 안내 문구에 들어갈 내용 (예: "줍기 (E)", "대화하기 (E)")
    string GetInteractPrompt();

    // 상호작용 키를 눌렀을 때 실행될 함수 (플레이어 정보를 넘겨줌)
    void Interact(GameObject player); 
}
