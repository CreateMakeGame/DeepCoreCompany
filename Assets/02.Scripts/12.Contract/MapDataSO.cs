using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMapData", menuName = "UI/Map Data")]
public class MapDataSO : ScriptableObject
{
    public string mapID;                // 맵 식별자
    public string mapName;              // 맵 이름
    public Sprite mapPreviewSprite;     // 카드 상단 프리뷰 이미지
    public string sceneName;            // 이동할 씬 이름

    [Header("Danger")]  
    public int dangerCount;             // 표시할 위험도 아이콘 개수

    [Header("Items")]
    public List<ItemDataSO> mapItemList = new List<ItemDataSO>(); // 등장하는 n개의 아이템 데이터 리스트
}
