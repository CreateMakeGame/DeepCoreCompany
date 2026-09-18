using System.Collections.Generic;
using UnityEngine;

public enum MapSceneType
{
    Scene_Company,  // 모이는 장소
    Scene_Test,

}

[CreateAssetMenu(fileName = "NewMapData", menuName = "UI/Map Data")]
public class MapDataSO : ScriptableObject
{
    public string mapID;                // 맵 식별자
    public string mapName;              // 맵 이름
    public Sprite mapPreviewSprite;     // 카드 상단 프리뷰 이미지
    [Header("Scene Setting")]
    public MapSceneType sceneType;      // 이동할 씬 이름
    public string SceneName => sceneType.ToString();    // 씬을 불러올 때 srting으로 변환해서 전달

    [Header("Danger")]  
    public int dangerCount;             // 표시할 위험도 아이콘 개수

    [Header("Items")]
    public List<ItemDataSO> mapItemList = new List<ItemDataSO>(); // 등장하는 n개의 아이템 데이터 리스트
}
