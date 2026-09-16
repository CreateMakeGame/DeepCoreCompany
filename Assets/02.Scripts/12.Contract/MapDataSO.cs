using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMapDAta", menuName = "UI/Map Data")]
public class MapDataSO : ScriptableObject
{
    public string mapName;
    public Sprite mapPreviewSprite;
    [Header("Danger")]
    public int dangerCount; // 표시할 위험도 아이콘 개수
    [Header("Items")]
    public List<ItemDataSO> mapItemList = new List<ItemDataSO>(); // 등장하는 n개의 아이템 데이터 리스트
}
