using UnityEngine;

public enum VoxelType
{
    Air,
    Dirt,
    Iron,
    Gold,
    Crystals,
    Artifact,
    Sample,
    Hazardous,
}

[System.Serializable]
public struct ItemSpawnData
{
    public ItemDataSO itemData;           // 생성할 ScriptableObject 아이템 데이터
    public VoxelType voxelType;         // 해당 아이템이 매립될 복셀 타입
    public int minHeight;               // 생성 최소 깊이 (지표면 아래 거리)
    public int maxHeight;               // 생성 최대 깊이 (지표면 아래 거리)
}