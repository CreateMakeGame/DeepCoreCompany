using UnityEngine;
using System.Collections.Generic;

public class VoxelItemGenerator : MonoBehaviour
{
    [Header("Item Spawn Settings")]
    [SerializeField] private List<ItemSpawnData> itemSpawnList = new List<ItemSpawnData>();

    [Header("Cave Artiface Settings")]
    [SerializeField] private GameObject[] artifactPrefabs;
    [Range(0f, 1f)]
    [SerializeField] private float artifactSpawnChance = 0.7f;
    [Header("Artifact Burial Depth Settings")]
    [Tooltip("땅속에 파묻히는 최소 높이 오프셋 (0.5 = 절반 매몰)")]
    [Range(0.3f, 1.0f)]
    [SerializeField] private float minArtifactHeightOffset = 0.5f;

    [Tooltip("땅속에 파묻히는 최대 높이 오프셋 (0.9 = 살짝 매몰)")]
    [Range(0.3f, 1.0f)]
    [SerializeField] private float maxArtifactHeightOffset = 0.85f;

    public void ApplyItemVoxels(float[,,] densities, VoxelType[,,] voxelTypes,
        int width, int height, int depth, float surfaceLevel, VoxelSurfaceGenerator surfaceGen)
    {
        foreach (var spawnData in itemSpawnList)
        {
            if (spawnData.itemData == null) continue;

            // 해당 아이템이 생성될 수 있는 모든 복셀 좌표 수집
            List<Vector3Int> candidatePisition = new List<Vector3Int>();

            for (int x = 0; x <= width; x++)
            {
                for (int z = 0; z <= depth; z++)
                {
                    float surfaceHeight = surfaceGen != null ? surfaceGen.GetSurfaceHeight(x, z) : height;
                    int startY = Mathf.Clamp(Mathf.FloorToInt(surfaceHeight), 0, height);

                    for (int y = startY; y >= 0; y--)
                    {
                        if (densities[x, y, z] > surfaceLevel && voxelTypes[x, y, z] == VoxelType.Dirt)
                        {
                            float depthFromSurface = surfaceHeight - y;
                            if (depthFromSurface >= spawnData.minHeight && depthFromSurface <= spawnData.maxHeight)
                            {
                                candidatePisition.Add(new Vector3Int(x, y, z));
                            }
                        }
                    }
                }
            }
            // 후보지 중 무작위로 targetCount개 선택하여 아이템 배치
            int targetCount = Random.Range(spawnData.minCount, spawnData.maxCount + 1);
            int actualSpawnCount = Mathf.Min(targetCount, candidatePisition.Count);

            for (int i = 0; i < actualSpawnCount; i++)
            {
                int randomIndex = Random.Range(0, candidatePisition.Count);
                Vector3Int pos = candidatePisition[randomIndex];

                voxelTypes[pos.x, pos.y, pos.z] = spawnData.voxelType;
                // 중복 선택 방지를 위해 리스트에서 제거
                candidatePisition.RemoveAt(randomIndex);
            }
        }
    }

    /// <summary>
    /// 동굴 방(Chamber) 중심점들을 기반으로 동굴 바닥을 탐색하여 유물 프리팹을 배치
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public void SpawnCaveArtifacts(float[,,] densities, List<Vector3Int> chamberCenters, 
        int width, int height, int depth, float surfaceLevel)
    {
        if (artifactPrefabs == null || artifactPrefabs.Length == 0 || chamberCenters == null) return;

        // 기존 생성된 유물 정리용 컨테이너
        Transform artifactContainer = GetOrCreateContainer("[Group] Cave Artifacts", transform);
        ClearContainerChildren(artifactContainer);

        Vector3 offset = new Vector3(width / 2f, height / 2f, depth / 2f);

        foreach (var chamber in chamberCenters)
        {
            if (Random.value > artifactSpawnChance) continue;

            // 방 중심점이 실제로 뚫린 동굴(공기)인지 확인 (땅속이면 스폰하지 않음)
            if (densities[chamber.x, chamber.y, chamber.z] > surfaceLevel) continue;

            // 동굴 공기층에서 아래로 내려가며 가장 먼저 만나는 땅 복셀 감지
            int floorY = -1;
            for (int y = chamber.y; y >= 0; y--)
            {
                if (densities[chamber.x, y, chamber.z] > surfaceLevel)
                {
                    floorY = y;
                    break;
                }
            }

            // 땅 복셀 바로 위(floorY + 1.0f) 공기 공간에 정상 배치
            if (floorY != -1)
            {
                // 설정된 범위 내에서 랜덤하게 파묻히는 높이 계산
                float randomOffset = Random.Range(minArtifactHeightOffset, maxArtifactHeightOffset);

                Vector3 localPos = new Vector3(chamber.x, floorY + randomOffset, chamber.z) - offset;
                Vector3 worldSpawnPos = transform.TransformPoint(localPos);

                // X, Z축 회전 시 땅속에 묻히므로 Y축(세로축)으로만 자연스럽게 회전
                Quaternion randomRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                GameObject prefab = artifactPrefabs[Random.Range(0, artifactPrefabs.Length)];

                Instantiate(prefab, worldSpawnPos, randomRot, artifactContainer);
            }
        }
    }

    private Transform GetOrCreateContainer(string containerName, Transform parent)
    {
        Transform container = parent.Find(containerName);
        if (container == null)
        {
            GameObject go = new GameObject(containerName);
            container = go.transform;
            container.SetParent(parent);
            container.localPosition = Vector3.zero;
        }
        return container;
    }

    // 재생성 시 기존 오브젝트 중복 누적 방지용 삭제 함수
    private void ClearContainerChildren(Transform container)
    {
        if (container == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            GameObject child = container.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }

    public GameObject GetFieldPrefab(VoxelType type)
    {
        ItemSpawnData data = itemSpawnList.Find(s => s.voxelType == type);
        return data.itemData != null ? data.itemData.fieldPrefab : null;
    }

    public List<ItemSpawnData> GetSpawnList() => itemSpawnList;
}