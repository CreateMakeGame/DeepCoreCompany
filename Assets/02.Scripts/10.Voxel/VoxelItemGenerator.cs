using UnityEngine;
using System.Collections.Generic;

public class VoxelItemGenerator : MonoBehaviour
{
    [Header("Item Spawn Settings")]
    [SerializeField] private List<ItemSpawnData> itemSpawnList = new List<ItemSpawnData>();

    public void ApplyItemVoxels(float[,,] densities, VoxelType[,,] voxelTypes,
        int width, int height, int depth, float surfaceLevel, VoxelSurfaceGenerator surfaceGen)
    {
        // 배열 범위 통일 (<= width, height, depth)
        for (int x = 0; x <= width; x++)
        {
            for (int y = 0; y <= height; y++)
            {
                for (int z = 0; z <= depth; z++)
                {
                    // 땅(복셀)인 경우에만 아이템 배치 검사
                    if (densities[x, y, z] > surfaceLevel && voxelTypes[x, y, z] != VoxelType.Air)
                    {
                        float surfaceHeight = surfaceGen != null ? surfaceGen.GetSurfaceHeight(x, z) : height;

                        // 복셀 좌표에 맞는 VoxelType 결정 (불필요한 foreach 제거!)
                        VoxelType itemType = DetermineVoxelType(y, surfaceHeight);

                        if (itemType != VoxelType.Dirt)
                        {
                            voxelTypes[x, y, z] = itemType;
                        }
                    }
                }
            }
        }
    }

    // 높이에 맞는 ItemData 기반의 VoxelType 결정
    private VoxelType DetermineVoxelType(int y, float surfaceHeight)
    {
        foreach (var spawnData in itemSpawnList)
        {
            if (spawnData.itemData == null) continue;

            float depthFromSurface = surfaceHeight - y;

            if (depthFromSurface >= spawnData.minHeight && depthFromSurface <= spawnData.maxHeight)
            {
                if (Random.value < spawnData.spawnChance)
                {
                    return spawnData.voxelType;
                }
            }
        }
        return VoxelType.Dirt; // 기본적으로 흙으로 설정
    }

    public GameObject GetFieldPrefab(VoxelType type)
    {
        ItemSpawnData data = itemSpawnList.Find(s => s.voxelType == type);
        return data.itemData != null ? data.itemData.fieldPrefab : null;
    }

    public List<ItemSpawnData> GetSpawnList() => itemSpawnList;
}