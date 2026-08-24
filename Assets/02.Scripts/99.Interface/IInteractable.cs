using UnityEngine;
public enum InteractionType
{
    None,       // 상호작용 없음 (UI 숨김)
    Default,    // 기본 상호작용 (기본 점 또는 손가락 모양)
    Pickup,     // 아이템 줍기 (손 모양 아이콘)
    Dig,        // 땅 파기 / 채굴 (곡괭이/삽 아이콘)
    Talk,       // 대화하기 (말풍선 아이콘)
    Inspect     // 조사하기 (돋보기 아이콘)
}

public interface IInteractable
{
    InteractionType interactionType { get; }
    // UI의 제목 부분에 들어갈 이름 (예: "철광석", "블랙스톤 대리인")
    string GetInteractName();

    // UI의 안내 문구에 들어갈 내용 (예: "줍기 (E)", "대화하기 (E)")
    string GetInteractPrompt();

    // 상호작용 키를 눌렀을 때 실행될 함수 (플레이어 정보를 넘겨줌)
    void Interact(GameObject player); 
}
