using UnityEngine;

public enum VoxelType
{
    Air,
    Dirt,
    Iron,
    Gold,
    Artifact,
    Sample,
    Hazardous,
}

[System.Serializable]
public struct ItemSpawnData
{
    public ItemData itemData;           // 생성할 ScriptableObject 아이템 데이터
    public VoxelType voxelType;         // 해당 아이템이 매립될 복셀 타입
    public int minCount;                // 아이템 최소 스폰 개수
    public int maxCount;                // 아이템 최대 스폰 개수
    public int minHeight;               // 생성 최소 높이
    public int maxHeight;               // 생성 최대 높이
}