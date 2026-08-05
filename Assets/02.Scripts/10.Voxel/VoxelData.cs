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
    [Range(0f, 1f)]
    public float spawnChance;           // 스폰 확률 (0 ~ 1)
    public int minHeight;               // 생성 최소 높이
    public int maxHeight;               // 생성 최대 높이
}